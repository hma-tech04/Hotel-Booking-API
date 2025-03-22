namespace API.DTOs.Response;

public class TokenResponse{
    public string Token {get; set;}
    public TokenResponse(string Token){
        this.Token = Token;
    }
}