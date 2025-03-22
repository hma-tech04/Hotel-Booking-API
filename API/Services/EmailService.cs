using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp, string name)
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
        {
            Port = _emailSettings.Port,
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = "Xác nhận mật khẩu cho tài khoản HotelBooking",
            Body = $@"
                    <html>
                        <body>
                            <h2>Chào {name}!</h2>
                            <p>Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu cho tài khoản của bạn tại ứng dụng <strong>HotelBooking</strong>.</p>
                            <p>Để hoàn tất quá trình, vui lòng nhập mã OTP dưới đây vào ứng dụng của chúng tôi:</p>
                            <h3 style='color: #4CAF50; font-size: 20px; font-weight: bold;'>{otp}</h3>
                            <p>Mã OTP này sẽ hết hạn sau <strong>5 phút</strong>. Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.</p>
                            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                            <br>
                            <p><em>HotelBooking Team</em></p>
                        </body>
                    </html>",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
