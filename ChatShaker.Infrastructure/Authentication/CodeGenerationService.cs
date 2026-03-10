using ChatShaker.Application.Interfaces;

namespace ChatShaker.Infrastructure.Authentication;

public class CodeGenerationService : ICodeGenerationService
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
    public string GenerateCode(int length = 16)
    {
        var random = new Random();
        
        return new string(Enumerable.Range(0, length)
            .Select(_ => Chars[random.Next(Chars.Length)])
            .ToArray());
    }
}