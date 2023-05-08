using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting
{
    public interface IRateLimiter
    {
        bool IsAppliedFor(string algorithm);
        Task<bool> RequestAccess(string key, string requestId);
    }
}
