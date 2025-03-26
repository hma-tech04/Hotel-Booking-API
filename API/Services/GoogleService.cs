using Google.Apis.Auth;

public class GoogleService
{
    private readonly string _clientID;
    public GoogleService(IConfiguration config)
    {
        _clientID = config["Authentication:Google:ClientId"] ?? string.Empty;
    }

    public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientID }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error verifying Google token: {ex.Message}");
            throw new CustomException(ErrorCode.Unauthorized, "Invalid Google token.", ex);
        }
    }
    
}
