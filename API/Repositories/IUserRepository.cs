using API.Models;
namespace API.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUserAsync();
    Task<User?> GetUseByIDrAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> AddUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<User?> DeleteUserAsync(int id);
}
