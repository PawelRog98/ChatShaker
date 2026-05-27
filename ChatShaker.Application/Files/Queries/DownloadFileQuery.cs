using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Files.Queries;

public class DownloadFileQuery : IRequest<FileResultModel>
{
    public DownloadFileQuery(Guid publicId, long userId)
    {
        PublicId = publicId;
        UserId = userId;
    }

    public Guid PublicId { get; set; }
    public long UserId { get; set; }
}

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, FileResultModel>
{
    private readonly IFileResourceRepository _fileResourceRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IFileManager _fileManager;

    public DownloadFileQueryHandler(IFileResourceRepository fileResourceRepository, 
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IFileManager fileManager)
    {
        _fileResourceRepository = fileResourceRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _fileManager = fileManager;
    }

    public async Task<FileResultModel> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        var fileResource = await _fileResourceRepository.Get(request.PublicId, cancellationToken);

        if (fileResource == null)
            throw new BadRequestException("File not found");

        if (fileResource.ChatRoomId.HasValue)
        {
            var isMember = await _chatRoomMembershipRepository.Exists(fileResource.ChatRoomId.Value, request.UserId, cancellationToken);
            if (!isMember)
                throw new ForbiddenException("You are not a member of the room this file belongs to.");
        }

        var stream = await _fileManager.DownloadFile(fileResource.FileName, fileResource.CreatedAtUtc);

        return new FileResultModel
        {
            FileStream = stream,
            FileName = fileResource.FileName,
            ContentType = fileResource.ContentType
        };
    }
}