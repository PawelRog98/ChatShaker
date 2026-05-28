using ChatShaker.Application.MessagesManagment.Commands.SendMessage;

namespace ChatShaker.Api.Clients;

public interface IChatClient
{
    Task MessageSent(MessageDto messageDto);
    Task MessageDelivered(Guid messagePublicId);
    Task MessageRead(Guid messagePublicId);
    Task UserAdded(Guid userPublicId);
    Task UserIdentityChanged(Guid userPublicId);
}
