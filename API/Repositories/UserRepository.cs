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
    public async Task<User?> GetUseByIDrAsync(int id) {
        var user = await _context.Users.FindAsync(id);
        return user;
    }
    public async Task<User?> GetUserByEmailAsync(string email){
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<User> UpdateUserAsync(User user) {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}