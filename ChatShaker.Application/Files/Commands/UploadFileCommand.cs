using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Files.Commands;

public class UploadFileCommand : IRequest<string>
{
    public UploadFileCommand(UploadedFileDto file)
    {
        FileDto = file;
    }
    
    public UploadedFileDto FileDto { get; set; }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
{
    private readonly IFileResourceRepository _fileResourceRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IFileManager _fileManager;
    private readonly IUnitOfWork _unitOfWork;

    public UploadFileCommandHandler(
        IFileResourceRepository fileResourceRepository, 
        IChatRoomRepository chatRoomRepository,
        IFileManager fileManager, 
        IUnitOfWork unitOfWork)
    {
        _fileResourceRepository = fileResourceRepository;
        _chatRoomRepository = chatRoomRepository;
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
                throw new Exception("Chat room not found.");

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