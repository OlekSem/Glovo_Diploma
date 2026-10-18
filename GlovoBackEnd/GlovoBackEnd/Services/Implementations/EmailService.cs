using GlovoBackEnd.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GlovoBackEnd.Services.Implementations;

public class EmailService(IConfiguration configuration) : IEmailService
    {
        // Sends an email
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
        
            email.From.Add(new MailboxAddress(
                configuration["EmailSettings:SenderName"], 
                configuration["EmailSettings:SenderEmail"]));
            
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // 👇 FIX: Bypass strict revocation check locally if your OS blocks CRL/OCSP lookups
            smtp.CheckCertificateRevocation = false;
        
            await smtp.ConnectAsync(
                configuration["EmailSettings:Server"], 
                int.Parse(configuration["EmailSettings:Port"] ?? "587"), 
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                configuration["EmailSettings:Username"], 
                configuration["EmailSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }