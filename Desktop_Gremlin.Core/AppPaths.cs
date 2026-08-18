using System;

namespace DesktopGremlin
{
    /// <summary>
    /// Base directory sprite sheets, sounds and config.txt are loaded from. Defaults to the
    /// executable's directory (desktop). Mobile hosts extract their bundled assets to a real
    /// filesystem directory once at startup and point this at it, so the rest of the app never
    /// needs to know it isn't running on desktop.
    /// </summary>
    public static class AppPaths
    {
        public static string BaseDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    }
}
