namespace RateLimiting.Services.RateLimiting
{
    public interface IRateLimiter
    {
        bool IsAppliedFor(string algorithm);
        bool RequestAccess(string key);
    }
}
