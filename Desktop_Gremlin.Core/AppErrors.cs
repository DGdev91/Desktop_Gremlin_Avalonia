using System;

namespace DesktopGremlin
{
    /// <summary>
    /// Error reporting seam so Core logic doesn't depend on any platform's UI toolkit.
    /// Each host (desktop, Android, ...) sets Reporter once at startup to show its own dialog/toast.
    /// </summary>
    public static class AppErrors
    {
        public static Action<string, string, bool> Reporter { get; set; } = (message, title, close) =>
        {
            Console.Error.WriteLine($"{title}: {message}");
            if (close)
            {
                Environment.Exit(1);
            }
        };

        public static void Report(string message, string title, bool close)
        {
            Reporter(message, title, close);
        }
    }
}
