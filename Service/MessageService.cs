using EmailTriggerApp.Model;

namespace EmailTriggerApp.Service
{
    public class MessageService : IMessageService
    {
        public async Task<bool> SendAsync(string topic, EmailMessage message)
        {
            Console.WriteLine($"Sending email to: {message.Email}");
            return await Task.FromResult(true);
        }
    }
}
