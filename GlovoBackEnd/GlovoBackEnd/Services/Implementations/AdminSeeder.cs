namespace GlovoBackEnd.Services.Implementations;

using GlovoBackEnd.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

/// <summary>Creates the administrator role and, when configured, the initial admin account.</summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roles = services.GetRequiredService<RoleManager<RoleEntity>>();
        var users = services.GetRequiredService<UserManager<UserEntity>>();

        if (!await roles.RoleExistsAsync(ApplicationRoleNames.Administrator))
        {
            var result = await roles.CreateAsync(new RoleEntity { Name = ApplicationRoleNames.Administrator });
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unable to create Administrator role: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        var email = configuration["Admin:Email"]?.Trim();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new UserEntity
            {
                Id = Guid.NewGuid(),
                UserName = configuration["Admin:Username"]?.Trim() ?? email,
                Email = email,
                EmailConfirmed = true,
                FirstName = configuration["Admin:FirstName"]?.Trim() ?? "Admin",
                LastName = configuration["Admin:LastName"]?.Trim()
            };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unable to create configured admin: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        if (!await users.IsInRoleAsync(admin, ApplicationRoleNames.Administrator))
            await users.AddToRoleAsync(admin, ApplicationRoleNames.Administrator);
    }
}
