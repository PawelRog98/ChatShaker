namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class CreateChatRoomDto
{
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<UserEncryptionDto> Keys { get; set; }
}
