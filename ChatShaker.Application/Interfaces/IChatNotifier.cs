using ChatShaker.Application.MessagesManagment.Commands.SendMessage;

namespace ChatShaker.Application.Interfaces;

public interface IChatNotifier
{
    Task MessageSent(Guid roomPublicId, MessageDto messageDto, CancellationToken cancellationToken);
    Task MessageRead(Guid roomPublicId, Guid messagePublicId, CancellationToken cancellationToken);
    Task UserAdded(Guid roomPublicId, Guid userPublicId, CancellationToken cancellationToken);
}
