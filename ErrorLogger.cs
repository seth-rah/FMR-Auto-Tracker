using System;
using System.Diagnostics;
using System.Security;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public static class ErrorLogger
    {
        private const string EventSourceName = "YuGiOh FMR Monitor";
        private const string EventLogName = "Application";

        static ErrorLogger()
        {
            EnsureEventSourceExists();
        }

        private static void EnsureEventSourceExists()
        {
            try
            {
                if (!EventLog.SourceExists(EventSourceName))
                {
                    EventLog.CreateEventSource(EventSourceName, EventLogName);
                }
            }
            catch
            {
            }
        }

        public static void LogError(string operation, Exception exception)
        {
            var message = $"[{DateTime.Now}] Operation: {operation}\nException: {exception.GetType().Name}\nMessage: {exception.Message}";

            Debug.WriteLine($"[ERROR] {message}");

            try
            {
                EventLog.WriteEntry(EventSourceName, message, EventLogEntryType.Error);
            }
            catch
            {
            }
        }
    }
}