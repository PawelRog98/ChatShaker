using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<User?> GetUserById(long id, CancellationToken cancellationToken)
        {
            return await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<User?> GetUserByPublicId(Guid publicId, CancellationToken cancellationToken)
        {
            return await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByPublicId(List<Guid> publicIds, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(x => publicIds.Contains(x.PublicId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsAnyUsers(CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(cancellationToken);
        }

        public async Task SaveNewUser(User user, CancellationToken cancellationToken)
        {
            await _context.AddAsync(user, cancellationToken);
            var result = await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine("SaveChanges result: " + result);
        }
    }
}
