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
            catch (InvalidOperationException)
            {
                Debug.WriteLine($"[ErrorLogger] Event source already exists or log is unavailable: {EventSourceName}");
            }
            catch (SecurityException)
            {
                Debug.WriteLine($"[ErrorLogger] Insufficient permissions to create event source: {EventSourceName}");
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
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ErrorLogger] Failed to write to event log: {ex.Message}");
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine($"[ErrorLogger] Insufficient permissions to write to event log: {ex.Message}");
            }
        }
    }
}