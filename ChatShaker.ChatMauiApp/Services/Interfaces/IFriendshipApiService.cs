using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IFriendshipApiService
{
    Task<Response<object>> SendInvitation(string invitationCode);
    Task<Response<List<SentInvitationDto>>> GetSentInvitations();
}