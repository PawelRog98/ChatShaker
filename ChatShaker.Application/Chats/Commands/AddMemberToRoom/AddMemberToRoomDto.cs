namespace ChatShaker.Application.Chats.Commands.AddMemberToRoom;

public class AddMemberToRoomDto
{
    public Guid UserToAddPublicId { get; set; }
    public Guid RoomPublicId { get; set; }
    public string EncryptedKey { get; set; }
}
