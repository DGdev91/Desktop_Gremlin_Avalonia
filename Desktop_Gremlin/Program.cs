using Avalonia;
using System;

namespace DesktopGremlin
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppErrors.Reporter = (message, title, close) => MainWindow.ErrorClose(message, title, close);

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
