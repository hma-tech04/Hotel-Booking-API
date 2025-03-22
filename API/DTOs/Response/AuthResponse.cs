namespace API.DTOs.Response;
public class AuthResponse{
    public string AccessToken {get; set;}
    public string RefeshToken {get; set;}

    public AuthResponse(string AccessToken, string RefeshToken){
        this.AccessToken = AccessToken;
        this.RefeshToken = RefeshToken;
    }
}