using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace RateLimiting.Utils
{
    public static class HttpHelper
    {
        public static void TrySetRateLimitingKey(HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue(Constants.CustomHeaders.ApiKey, out StringValues apiKey);
            httpContext.Items[Constants.HttpItemsKeys.RateLimitingKey] = (string)apiKey;
        }

        public static string GetRateLimitingKey(HttpContext httpContext)
            => (string)httpContext.Items[Constants.HttpItemsKeys.RateLimitingKey]
            ?? string.Empty;

        public static string GetHttpRequestId(HttpContext httpContext)
        {
            lock (httpContext.Items)
            {
                string newRequestId = Guid.NewGuid().ToString();

                httpContext.Items.TryAdd(Constants.HttpItemsKeys.RequestId, newRequestId);

                return httpContext.Items[Constants.HttpItemsKeys.RequestId] as string;
            }
        }
    }
}
