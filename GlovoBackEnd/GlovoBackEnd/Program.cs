using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Amazon.Runtime;
using Amazon.S3;
using Core.Services;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Services.Implementations;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));


builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Glovo API",
        Version = "v1",
        Description = "Glovo Backend API Documentation"
    });

    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT signing key is not configured."))),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "username",
            
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // FIX 3: на цей момент context.Token завжди порожній,
                // тому перевіряємо заголовок напряму
                if (context.Request.Headers.ContainsKey("Authorization"))
                    return Task.CompletedTask;

                var path = context.Request.Path;
                var cookies = context.Request.Cookies;

                if (path.StartsWithSegments("/api/partner"))
                    context.Token = cookies["partner_access_token"];
                else if (path.StartsWithSegments("/api/client"))
                    context.Token = cookies["client_access_token"];
                else if (path.StartsWithSegments("/api/courier"))
                    context.Token = cookies["courier_access_token"];
                else if (path.StartsWithSegments("/api/admin"))
                    context.Token = cookies["admin_access_token"];
                else
                    context.Token = cookies["client_access_token"]
                                    ?? cookies["partner_access_token"]
                                    ?? cookies["courier_access_token"];

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var existing = identity.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToHashSet();

                    var extra = identity
                        .FindAll(c => c.Type is "role" or "roles")
                        .ToList();

                    foreach (var claim in extra)
                    {
                        if (existing.Add(claim.Value))
                            identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                    }
                }

                return Task.CompletedTask;
            }
            
        };
        
    });

try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrWhiteSpace(redisConnectionString))
        throw new InvalidOperationException("Redis connection string is not set in configuration.");

    var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to Redis - {ex.Message}");
    throw;
}

try
{
    var minioConfig = builder.Configuration.GetSection("MinIO");
    var accessKey = minioConfig["AccessKey"];
    var secretKey = minioConfig["SecretKey"];
    var endpoint = minioConfig["Endpoint"];

    if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("MinIO configuration section is incomplete (AccessKey/SecretKey/Endpoint).");

    var s3Client = new AmazonS3Client(
        new BasicAWSCredentials(accessKey, secretKey),
        new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        }
    );

    builder.Services.AddSingleton<IAmazonS3>(s3Client);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to configure MinIO/S3 client - {ex.Message}");
    throw;
}



builder.Services.AddAuthorization();
// Jwt Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<PartnerEntity>, PasswordHasher<PartnerEntity>>();
// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPartnerCompanyAccessService, PartnerCompanyAccessService>();
//Client Service
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IMinioImageService, MinioImageService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    var contactEmail = builder.Configuration["Geocoding:ContactEmail"]
        ?? builder.Configuration["Admin:Email"];
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        builder.Configuration["Geocoding:UserAgent"]
        ?? $"GlovoDiploma/1.0 (contact: {contactEmail ?? "not-configured"})");
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk");
    client.Timeout = TimeSpan.FromSeconds(10);
});





var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"] ?? "http://localhost:5173,http://localhost:5174,http://localhost:5175,http://localhost:8081,http://localhost:8082,http://16.192.175.236,http://16.192.175.236:8082,https://16.192.175.236";
var originsList = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(originsList)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



var app = builder.Build();

// 7. Automatic Database Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    dbContext.Database.Migrate();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Glovo API v1");

        options.RoutePrefix = "swagger";
    });
}


app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
