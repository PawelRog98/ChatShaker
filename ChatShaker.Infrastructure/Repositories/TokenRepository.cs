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
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;

        public TokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateToken(Token token, CancellationToken cancellationToken)
        {
            await _context.AddAsync(token, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Token?> GetTokenDataWithUser(string token, CancellationToken cancellationToken)
        {
            return await _context.Tokens
                .Include(x=>x.User)
                .ThenInclude(x=>x.Role)
                .FirstOrDefaultAsync(x => x.TokenData == token && x.ExpireDateTime > DateTime.UtcNow, cancellationToken);
        }

        public async Task DeleteToken(Token token, CancellationToken cancellationToken)
        {
            _context.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
