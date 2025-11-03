using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;


namespace CITP.MovieApp.Infrastructure.Repositories

{
    public class UserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public virtual async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public virtual async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public virtual async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
