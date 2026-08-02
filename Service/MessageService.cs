using EmailTriggerApp.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailTriggerApp.Service
{
    public class MessageService : IMessageService
    {
        public async Task<bool> SendAsync(string topic, EmailMessage message)
        {


            // 1. Construct the email message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Your Sender Name", "your-email@gmail.com"));
            email.To.Add(new MailboxAddress("", message.Email));
            email.Subject = "Welcome to Our Platform!";

            email.Body = new TextPart("plain")
            {
                Text = "Hello! Thank you for signing up."
            };

            // Console feedback
            Console.WriteLine($"Sending email to: {message.Email}...");

            // 2. Connect and send via SMTP Client
            using var client = new SmtpClient();
            try
            {
                // Connect to your SMTP provider (e.g., Gmail, SendGrid, Mailtrap)
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Authenticate (use an App Password if using Gmail)
                await client.AuthenticateAsync("your-email@gmail.com", "your-app-password");

                // Send
                await client.SendAsync(email);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
            return await Task.FromResult(true);
        }
    }
}
