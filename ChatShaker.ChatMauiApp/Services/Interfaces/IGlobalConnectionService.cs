using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

    public interface IGlobalConnectionService
    {
        event Action<ChatListItem>? OnRoomActivity;
        void Bind();
    }