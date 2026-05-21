namespace ChatShaker.Application.Jobs.Triggered.Interfaces;

public interface ISendVerificationCodeJob
{
    Task Execute(string email, string code);
}