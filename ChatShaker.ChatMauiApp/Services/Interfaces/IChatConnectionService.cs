using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IChatConnectionService
{
    event Action<MessageDto>? OnMessageSent;
    event Action<Guid>? OMessageDelivered;
    event Action<Guid>? OnMessageRead;
    event Action<Guid>? OnUserAdded;

    void BindEvents();
    Task AddUser(Guid roomPublicId);
    Task SendMessage(MessageDto messageDto);
    Task JoinRoom(Guid roomPublicId);
    Task MarkAsRead(Guid messagePublicId);
    Task MarkAsDelivered(Guid messagePublicId);
}
