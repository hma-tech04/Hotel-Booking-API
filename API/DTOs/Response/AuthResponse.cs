namespace API.DTOs.Response;
public class AuthResponse{
    public string AccessToken {get; set;}

    public AuthResponse(string AccessToken){
        this.AccessToken = AccessToken;
    }
}