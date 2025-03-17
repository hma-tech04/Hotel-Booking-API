using API.Models;
namespace API.Repositories;
using API.Data;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly HotelBookingContext _context;

    public UserRepository(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task<User> AddUserAsync(User user){
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<User?> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id); 
        if (user == null)
        {
            return null;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<IEnumerable<User>> GetAllUserAsync(){
        return await _context.Users.ToListAsync();
    }
    public async Task<User?> GetUseByIDAsync(int id) {
        var user = await _context.Users.FindAsync(id);
        return user;
    }
    public async Task<User?> GetUserByEmailAsync(string email){
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<User?> UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.UserId);
        if (existingUser != null)
        {
            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return existingUser;
        }
        return null; 
    }

}