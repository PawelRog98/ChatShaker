using ChatShaker.Application.Jobs.Triggered.Interfaces;
using ChatShaker.Domain.Models.Email;
using ChatShaker.Domain.Services;

namespace ChatShaker.Application.Jobs.Triggered;

public class SendVerificationCodeJob : ISendVerificationCodeJob
{
    private readonly IEmailService _emailService;

    public SendVerificationCodeJob(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task Execute(string email, string code)
    {
        var html =
            $"""
             <div align="center">
                 <h1>ChatShaker</h1>
                 <hr width="60%">
                 <h2>Account Verification</h2>
                 <p>Welcome! To complete your registration, please enter the following verification code in the application:</p>
                 <p>
                    <font size="6"><b>{code}</b></font>
                 </p>
                 <p><i>This code will expire in 3 hours.</i></p>
                 <hr width="60%">
                 <p><small>If you did not create an account with ChatShaker, please ignore this email.</small></p>
             </div>
             """;

        await _emailService.SendMessage(new EmailMessage
        {
            To = email,
            Subject = "ChatShaker Verification Code",
            HtmlBody = html
        });
    }
}