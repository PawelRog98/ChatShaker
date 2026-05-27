using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;

using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Files.Commands;

public class UploadFileCommand : IRequest<string>
{
    public UploadFileCommand(UploadedFileDto file, long userId)
    {
        FileDto = file;
        UserId = userId;
    }

    public UploadedFileDto FileDto { get; set; }
    public long UserId { get; set; }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
{
    private readonly IFileResourceRepository _fileResourceRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IFileManager _fileManager;
    private readonly IUnitOfWork _unitOfWork;

    public UploadFileCommandHandler(
        IFileResourceRepository fileResourceRepository, 
        IChatRoomRepository chatRoomRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IFileManager fileManager, 
        IUnitOfWork unitOfWork)
    {
        _fileResourceRepository = fileResourceRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _fileManager = fileManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var room = await _chatRoomRepository.GetByPublicId(request.FileDto.RoomId, cancellationToken);
            if (room == null)
                throw new BadRequestException("Chat room not found.");

            var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this room.");

            var savedPath = await _fileManager.UploadEncryptedFile(request.FileDto.File);

            var fileResource = new FileResource
            {
                FileName = Path.GetFileName(savedPath),
                ContentType = request.FileDto.File.ContentType,
                ChatRoomId = room.Id,
                Size = request.FileDto.File.Length,
                CreatedAtUtc = DateTime.UtcNow,
                TypeEnum = Domain.Enums.FileType.Encrypted
            };

            await _fileResourceRepository.Add(fileResource, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);

            return fileResource.PublicId.ToString();
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}