using System.Security.Cryptography;
using System.Text;

public class SignatureService
{   
    private readonly string _secret;

    public SignatureService(IConfiguration configuration)
    {
        _secret = configuration["SignatureSecret"] ?? throw new ArgumentNullException("SignatureSecret", "SignatureSecret cannot be null"); 
    }

    public string GenerateSignature(string data)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
    public bool VerifySignature(string data, string receivedSignature)
    {
        string expectedSignature = GenerateSignature(data);
        return expectedSignature == receivedSignature;
    }
}
