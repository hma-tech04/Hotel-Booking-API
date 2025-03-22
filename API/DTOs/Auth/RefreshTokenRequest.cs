namespace API.DTOs.Auth;
public class RefreshTokenRequest
{
    public int UserId { get; set; }
    public string RefreshToken { get; set; }

    public RefreshTokenRequest(int userId, string refreshToken)
    {
        UserId = userId;
        RefreshToken = refreshToken;
    }
}
