using System;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting
{
    public abstract class RateLimiter : IRateLimiter
    {
        protected const int DefaultMaxLockTryCount = 3;
        protected const int DefaultLockRetryDelay = 10;
        protected const int DefaultLockTimeOutMs = 7000;

        public abstract string Name { get; }

        public virtual bool IsAppliedFor(string algorithm) => Name == algorithm;
        public abstract Task<bool> RequestAccess(string resource, string key, string requestId);
        protected virtual string GetStoredKey(string resource, string key) => GetStoredKeyDefault(resource, Name, key);
        protected virtual string GetLockKey(string resource, string key) => $"Lock_{GetStoredKeyDefault(resource, Name, key)}";
        protected virtual string GetRandomLockValue() => Guid.NewGuid().ToString();

        public static string GetStoredKeyDefault(string resource, string limiterName, string key) => $"{nameof(RateLimiting)}_{resource}_{limiterName}_{key}";
    }
}
