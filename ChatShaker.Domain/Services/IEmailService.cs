using ChatShaker.Domain.Models.Email;

namespace ChatShaker.Domain.Services;

public interface IEmailService
{
    Task SendMessage(EmailMessage message, CancellationToken token = default);
}