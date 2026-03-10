namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomDto
{
    public Guid? PublicId { get; set; }
    public string Name { get; set; }
    public DateTime CreateDateUtc { get; set; }
    public List<RoomKeyDataDto> Keys { get; set; }
}