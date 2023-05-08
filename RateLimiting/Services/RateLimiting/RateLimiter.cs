using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting
{
    public abstract class RateLimiter : IRateLimiter
    {
        public abstract string Name { get; }

        public virtual bool IsAppliedFor(string algorithm) => Name == algorithm;
        protected virtual string GetStoredKey(string key) => GetStoredKeyDefault(Name, key);
        public abstract Task<bool> RequestAccess(string key, string requestId);

        public static string GetStoredKeyDefault(string limiterName, string key) => $"{limiterName}_{key}";
    }
}
