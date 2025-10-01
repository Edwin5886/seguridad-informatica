using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailHelper
{
    private readonly IConfiguration _config;

    public EmailHelper(IConfiguration config)
    {
        _config = config;
    }

    public void SendPasswordRecoveryEmail(string toEmail, string recoveryLink)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
        var smtpUser = _config["EmailSettings:SmtpUser"];
        var smtpPass = _config["EmailSettings:SmtpPass"];

        var mail = new MailMessage();
        mail.From = new MailAddress(smtpUser);
        mail.To.Add(toEmail);
        mail.Subject = "Recuperación de contraseña";
        mail.Body = $"Haz clic en el siguiente enlace para restablecer tu contraseña: {recoveryLink}";
        mail.IsBodyHtml = false;

        using var smtp = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
        smtp.Send(mail);
    }
}
