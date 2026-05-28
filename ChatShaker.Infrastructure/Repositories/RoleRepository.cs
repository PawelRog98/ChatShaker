using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Role> GetIdByName(string name, CancellationToken cancellationToken)
    {
        return await _context.Roles.FirstOrDefaultAsync(x=>x.RoleName == name, cancellationToken);
    }
}