using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using MailKit.Net.Smtp;
using MimeKit;

namespace Kultura.Application.Repository.Concrete
{
    public class EmailService(EmailConfiguration _emailConfig) : IEmailService
    {
        public void SendEmail(Message message)
        {
            var emailMessage = CreatedEmailMessage(message);
            Send(emailMessage);
        }

        public MimeMessage CreatedEmailMessage(Message message)
        {
            var emailmessage = new MimeMessage();
            emailmessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailmessage.To.AddRange(message.To);
            emailmessage.Subject = message.Subject;
            emailmessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

            return emailmessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("X0AUTH2");
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                client.Send(mailMessage);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                client.Disconnect(true);
                client?.Dispose();
            }

        }
    }
}
