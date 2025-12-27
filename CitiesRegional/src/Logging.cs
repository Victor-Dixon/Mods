using System;
using System.Reflection;
using BepInEx.Logging;

namespace CitiesRegional
{
    /// <summary>
    /// Unified logging interface for CitiesRegional mod.
    /// Uses BepInEx logger when available, falls back to Unity Debug.Log.
    /// </summary>
    internal static class Logging
    {
        public static ManualLogSource LogSource { get; private set; } = null!;

        /// <summary>
        /// Initialize the logger with a BepInEx log source.
        /// Call this from plugin Awake().
        /// </summary>
        public static void Init(ManualLogSource src)
        {
            LogSource = src;
        }

        public static void LogInfo(string msg)
        {
            var fullMsg = $"[CitiesRegional] {msg}";
            if (LogSource != null)
                LogSource.LogInfo(fullMsg);
            else
                FallbackLog("Info", fullMsg);
        }

        public static void LogWarn(string msg) => LogWarning(msg);
        
        public static void LogWarning(string msg)
        {
            var fullMsg = $"[CitiesRegional] {msg}";
            if (LogSource != null)
                LogSource.LogWarning(fullMsg);
            else
                FallbackLog("Warning", fullMsg);
        }

        public static void LogError(string msg)
        {
            var fullMsg = $"[CitiesRegional] {msg}";
            if (LogSource != null)
                LogSource.LogError(fullMsg);
            else
                FallbackLog("Error", fullMsg);
        }

        public static void LogDebug(string msg)
        {
            var fullMsg = $"[CitiesRegional] {msg}";
            if (LogSource != null)
                LogSource.LogDebug(fullMsg);
            else
                FallbackLog("Debug", fullMsg);
        }

        private static void FallbackLog(string level, string message)
        {
            // In-game, Logging.Init() should always be called, so this is primarily for unit tests
            // and edge cases where BepInEx logging isn't available.
            if (TryUnityDebug(level, message))
                return;

            try
            {
                Console.WriteLine($"{level}: {message}");
            }
            catch
            {
                // ignore - last resort
            }
        }

        private static bool TryUnityDebug(string level, string message)
        {
            try
            {
                // Avoid compile-time dependency on UnityEngine in environments where it isn't present (unit tests).
                var debugType = Type.GetType("UnityEngine.Debug, UnityEngine.CoreModule", throwOnError: false);
                if (debugType == null) return false;

                var methodName = level switch
                {
                    "Warning" => "LogWarning",
                    "Error" => "LogError",
                    _ => "Log"
                };

                var method = debugType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);
                if (method == null) return false;

                method.Invoke(null, new object[] { message });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

