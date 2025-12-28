using System;

namespace CitiesRegional
{
    internal static class Logging
    {
        public static void Init(object src) { } 

        public static void LogInfo(string msg) => Console.WriteLine($"[Info] {msg}");
        public static void LogWarn(string msg) => Console.WriteLine($"[Warn] {msg}");
        public static void LogWarning(string msg) => Console.WriteLine($"[Warn] {msg}");
        public static void LogError(string msg) => Console.WriteLine($"[Error] {msg}");
        public static void LogDebug(string msg) => Console.WriteLine($"[Debug] {msg}");
    }
}
