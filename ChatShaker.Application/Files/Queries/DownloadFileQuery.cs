using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Files.Queries;

public class DownloadFileQuery : IRequest<FileResultModel>
{
    public DownloadFileQuery(Guid publicId)
    {
        PublicId = publicId;
    }
    
    public Guid PublicId { get; set; }
}

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, FileResultModel>
{
    private readonly IFileResourceRepository _fileResourceRepository;
    private readonly IFileManager _fileManager;

    public DownloadFileQueryHandler(IFileResourceRepository fileResourceRepository, IFileManager fileManager)
    {
        _fileResourceRepository = fileResourceRepository;
        _fileManager = fileManager;
    }

    public async Task<FileResultModel> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        var fileResource = await _fileResourceRepository.Get(request.PublicId, cancellationToken);

        if (fileResource == null)
            throw new BadRequestException("File not found");

        var stream = await _fileManager.DownloadFile(fileResource.FileName, fileResource.CreatedAtUtc);

        return new FileResultModel
        {
            FileStream = stream,
            FileName = fileResource.FileName,
            ContentType = fileResource.ContentType
        };
    }
}