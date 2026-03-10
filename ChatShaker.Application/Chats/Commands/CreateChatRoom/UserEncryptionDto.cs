namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class UserEncryptionDto
{
    public Guid UserId { get; set; }
    public string EncryptedUserKey { get; set; }
    public bool IsHost { get; set; }
}
