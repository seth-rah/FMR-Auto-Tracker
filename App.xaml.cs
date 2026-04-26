using System;
using System.Linq;
using System.Windows;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public sealed partial class App : Application
    {
        public static bool DebugMode { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            DebugMode = e.Args.Contains("--debug", StringComparer.OrdinalIgnoreCase);
            base.OnStartup(e);
        }
    }
}