using System.Threading.Tasks;

namespace SlidableLive.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
