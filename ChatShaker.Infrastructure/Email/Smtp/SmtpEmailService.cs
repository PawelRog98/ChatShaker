using System.Net.Mail;
using ChatShaker.Domain.Models.Email;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using ChatShaker.Domain.Serivces;
using ChatShaker.Domain.Services;

namespace ChatShaker.Infrastructure.Email.Smtp;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(EmailSettings emailSettings, ILogger<SmtpEmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }
    public async Task SendMessage(EmailMessage message, CancellationToken token = default)
    {
        try
        {
            var mail = new MimeMessage();
            
            mail.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            
            mail.To.Add(MailboxAddress.Parse(message.To));
            mail.Subject = message.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.Body,
            };
            
            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls, token);
            
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, token);
            
            await smtp.SendAsync(mail, token);
            await smtp.DisconnectAsync(true, token);
            
            _logger.LogInformation("Email sent");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to: {message.To}");
            throw;
        }
    }
}