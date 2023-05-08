using StackExchange.Redis;
using System;

namespace RateLimiting.Services
{
    public interface IRequestStore
    {
        void AddRequestToStore(string requestId, string data);
        string GetRequestData(string requestId);
    }

    public class RequestStore : IRequestStore
    {
        const string RequestKeyTemplate = "RequestData_{RequestId}";

        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RequestStore(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public void AddRequestToStore(string requestId, string data)
        {
            IDatabase db = _connectionMultiplexer.GetDatabase();

            db.StringSet(GetKey(requestId), data, expiry: TimeSpan.FromMinutes(5));
        }

        public string GetRequestData(string requestId)
        {
            IDatabase db = _connectionMultiplexer.GetDatabase();

            return db.StringGet(GetKey(requestId));
        }

        private string GetKey(string requestId) => RequestKeyTemplate.Replace("{RequestId}", requestId);
    }
}
