namespace SlidableLive.Services
{
    public interface IUserInfo
    {
        bool IsAuthenticated { get; }
        string Name { get; }
    }
}