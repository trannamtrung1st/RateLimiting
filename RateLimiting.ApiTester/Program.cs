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

            ParallelLoopResult result = Parallel.ForEach(Enumerable.Range(0, 15), (idx) =>
            {
                //CallTokenBucketEndpoint(httpClient, idx).Wait();

                //CallLeakyBucketEndpoint(httpClient, idx).Wait();
            });

            while (!result.IsCompleted)
            {
                await Task.Delay(1000);
            }
        }

        static async Task CallTokenBucketEndpoint(HttpClient httpClient, int idx)
        {
            string path = "/api/resources/token-bucket";

            Console.WriteLine($"Thread {idx} sending request to {path}!");

            HttpResponseMessage resp = await httpClient.GetAsync(path);

            if (resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Thread {idx} successfully sent request!");
            }
            else
            {
                Console.WriteLine($"Thread {idx} failed to send request!");
            }
        }

        static async Task CallLeakyBucketEndpoint(HttpClient httpClient, int idx)
        {
            string path = "/api/resources/leaky-bucket";

            Console.WriteLine($"Thread {idx} sending request to {path}!");

            HttpResponseMessage resp = await httpClient.PostAsync(path,
                new StringContent(
                    JsonConvert.SerializeObject($"Data {idx}"), Encoding.UTF8, "application/json"));

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
