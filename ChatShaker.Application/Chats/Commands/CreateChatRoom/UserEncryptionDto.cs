namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class UserEncryptionDto
{
    public Guid PublicId { get; set; }
    public string EncryptedUserKey { get; set; }
    public bool isHost { get; set; }
}
