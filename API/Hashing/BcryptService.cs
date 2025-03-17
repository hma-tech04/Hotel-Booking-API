namespace API.Hashing;
using BCrypt.Net;
public class BcryptService
{
    public string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Verify(password, passwordHash);
    }
}