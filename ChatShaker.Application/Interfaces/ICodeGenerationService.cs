namespace ChatShaker.Application.Interfaces;

public interface ICodeGenerationService
{
    string GenerateCode(int length = 16);
}