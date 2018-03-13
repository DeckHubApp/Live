using System.Threading.Tasks;

namespace SlidableLive.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
