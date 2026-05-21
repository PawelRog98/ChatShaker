using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Tokens.Commands.DeleteExpiredTokens;

public class DeleteExpiredTokenCommand : IRequest<Unit>
{
    
}

public class DeleteExpiredTokenCommandHandler : IRequestHandler<DeleteExpiredTokenCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenRepository _tokenRepository;
    
    public DeleteExpiredTokenCommandHandler(IUnitOfWork unitOfWork, ITokenRepository tokenRepository)
    {
        _unitOfWork = unitOfWork;
        _tokenRepository = tokenRepository;
    }

    public async Task<Unit> Handle(DeleteExpiredTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            
            await _tokenRepository.DeleteExpiredTokens(cancellationToken);
            
            await _unitOfWork.Commit(cancellationToken);
            
            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}