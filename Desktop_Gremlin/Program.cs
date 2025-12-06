using Avalonia;
using System;

namespace DesktopGremlin
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
            => AppBuilder.Configure<DesktopGremlin.App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
