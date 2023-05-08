using System;

namespace RateLimiting.Utils
{
    public static class ConsoleHelper
    {
        public static void WriteLineDefault(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}
