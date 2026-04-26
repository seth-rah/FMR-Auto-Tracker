using System;
using System.Diagnostics;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public static class ErrorLogger
    {
        public static void LogError(string operation, Exception exception)
        {
            var message = $"[{DateTime.Now}] Operation: {operation}\nException: {exception.GetType().Name}\nMessage: {exception.Message}";
            Debug.WriteLine($"[ERROR] {message}");
        }
    }
}