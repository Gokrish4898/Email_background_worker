using EmailTriggerApp.Model;

namespace EmailTriggerApp.Service
{
    public interface IMessageService
    {
        Task<bool> SendAsync(string topic, EmailMessage message);
    }
}
