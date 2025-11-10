namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class CreateChatRoomDto
{
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<UserEncryptionDto> Users { get; set; }
}
