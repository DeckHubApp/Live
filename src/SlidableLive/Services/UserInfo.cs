using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SlidableLive.Services
{
    public class UserInfo : IUserInfo
    {
        private readonly HttpContext _context;

        public UserInfo(IHttpContextAccessor httpContextAccessor)
        {
            _context = httpContextAccessor.HttpContext;
        }

        public bool IsAuthenticated => _context.User.Identity.IsAuthenticated;
        public string Name => _context.User?.FindFirstValue(ClaimTypes.Name);
    }
}