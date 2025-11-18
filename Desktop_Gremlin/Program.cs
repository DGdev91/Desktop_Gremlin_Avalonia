using Avalonia;
using System;

namespace Desktop_Gremlin
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<Desktop_Gremlin.App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
