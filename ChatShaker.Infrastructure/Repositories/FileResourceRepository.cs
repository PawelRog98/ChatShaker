using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class FileResourceRepository : IFileResourceRepository
{
    private readonly AppDbContext _context;
    
    public FileResourceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task Add(FileResource fileResource, CancellationToken cancellationToken)
    {
        await _context.FileResources.AddAsync(fileResource, cancellationToken);
    }

    public async Task<FileResource?> Get(Guid publicId, CancellationToken cancellationToken)
    {
        return await  _context.FileResources.FirstOrDefaultAsync(x=>x.PublicId == publicId, cancellationToken);
    }
}