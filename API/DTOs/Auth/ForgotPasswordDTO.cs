using System.ComponentModel.DataAnnotations;

public class ForgotPasswordDTO{

    [EmailAddress]
    public required string Email {get; set;}
}