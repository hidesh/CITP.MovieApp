using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public Task<User?> GetByUsernameAsync(string username)
            => _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task<User?> GetByEmailAsync(string email)
            => _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // ✅ REQUIRED FOR /api/Auth/me
        public Task<User?> GetByIdAsync(int id)
            => _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

        public async Task AddAsync(User user)
            => await _context.Users.AddAsync(user);

        public async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }
}