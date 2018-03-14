using System.Security.Claims;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace SlidableLive.Services
{
    public class UserInfo : IUserInfo
    {
        private readonly HttpContext _context;

        [UsedImplicitly]
        public UserInfo(IHttpContextAccessor httpContextAccessor)
        {
            _context = httpContextAccessor.HttpContext;
        }

        public bool IsAuthenticated => _context.User.Identity.IsAuthenticated;
        public string Name => _context.User?.FindFirstValue(ClaimTypes.Name);
        public string Handle => _context.User?.FindFirstValue(SlidableClaimTypes.Handle);
    }
}