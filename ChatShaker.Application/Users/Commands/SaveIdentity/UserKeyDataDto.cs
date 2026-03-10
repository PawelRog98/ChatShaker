namespace ChatShaker.Application.Users.Commands.SaveIdentity;

public class UserKeyDataDto
{
    public Guid? PublicUserId {get; set;} 
    public string PublicKey { get; set; }
    public string DeviceId { get; set; }
}