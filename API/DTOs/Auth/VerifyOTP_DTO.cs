using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;
public class VerifyOTP_DTO{

    [EmailAddress]
    public required string Email {get; set;}

    [StringLength(6, ErrorMessage = "OTP must be 6 characters long.")]
    public required string OTP {get; set;}
}