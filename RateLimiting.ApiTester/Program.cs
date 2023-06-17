using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            List<(string Path, bool IsPost)> endpoints = new List<(string, bool)>()
            {
                ("/api/resources/token-bucket", false),
                ("/api/resources/leaky-bucket", true),
                ("/api/resources/fixed-window-counter", false),
                ("/api/resources/sliding-window-logs", false),
                ("/api/resources/sliding-window-counter-1", false),
                ("/api/resources/sliding-window-counter-2", false)
            };

            foreach (var endpoint in endpoints)
            {
                (string path, bool isPost) = endpoint;
                Console.WriteLine($"Testing endpoint: {path}");
                Console.WriteLine($"============================");

                ParallelLoopResult result = Parallel.ForEach(Enumerable.Range(0, 15), (idx) =>
                {
                    CallEndpoint(httpClient, path, idx, isPost).Wait();
                });

                while (!result.IsCompleted)
                {
                    await Task.Delay(1000);
                }

                Console.WriteLine($"============================");
                Console.WriteLine("Press enter to continue!");
                Console.ReadLine();
                Console.WriteLine("\n\n\n");
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
