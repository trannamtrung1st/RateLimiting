namespace RateLimiting.Services.RateLimiting
{
    public abstract class RateLimiter : IRateLimiter
    {
        public abstract string Name { get; }

        public virtual bool IsAppliedFor(string algorithm) => Name == algorithm;
        protected virtual string GetStoredKey(string key) => $"{Name}_{key}";
        public abstract bool RequestAccess(string key);
    }
}
