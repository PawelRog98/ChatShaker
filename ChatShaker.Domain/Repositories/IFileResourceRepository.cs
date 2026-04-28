using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IFileResourceRepository
{
    Task Add(FileResource fileResource, CancellationToken cancellationToken);
    Task<FileResource?> Get(Guid publicId, CancellationToken cancellationToken);
}