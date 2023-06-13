using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiting.ApiTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000", UriKind.Absolute);
            httpClient.DefaultRequestHeaders.Add("ApiKey", "ApiTester");

            var tokenBucket = (path: "/api/resources/token-bucket", isPost: false);
            var leakyBucket = (path: "/api/resources/leaky-bucket", isPost: true);
            var fixedWindowCounter = (path: "/api/resources/fixed-window-counter", isPost: false);
            var slidingWindowLogs = (path: "/api/resources/sliding-window-logs", isPost: false);
            var slidingWindowCounter1 = (path: "/api/resources/sliding-window-counter-1", isPost: false);
            var slidingWindowCounter2 = (path: "/api/resources/sliding-window-counter-2", isPost: false);

            ParallelLoopResult result = Parallel.ForEach(Enumerable.Range(0, 15), (idx) =>
            {
                (string path, bool isPost) = slidingWindowCounter2;

                CallEndpoint(httpClient, path, idx, isPost).Wait();
            });

            while (!result.IsCompleted)
            {
                await Task.Delay(1000);
            }
        }

        static async Task CallEndpoint(HttpClient httpClient, string path, int idx, bool isPost)
        {
            Console.WriteLine($"Thread {idx} sending request to {path}!");

            HttpResponseMessage resp;

            if (isPost)
            {
                resp = await httpClient.PostAsync(path,
                    new StringContent(
                        JsonConvert.SerializeObject($"Data {idx}"), Encoding.UTF8, "application/json"));
            }
            else
            {
                resp = await httpClient.GetAsync(path);
            }

            if (resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Thread {idx} successfully sent request!");
            }
            else
            {
                Console.WriteLine($"Thread {idx} failed to send request!");
            }
        }
    }
}
