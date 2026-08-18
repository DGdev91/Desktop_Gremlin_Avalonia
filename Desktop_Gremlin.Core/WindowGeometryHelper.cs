using Avalonia;
using Avalonia.Controls;

namespace DesktopGremlin
{
    public static class WindowGeometryHelper
    {
        public static PixelRect GetCombinedWorkingArea(Window window)
        {
            PixelRect combined = window.Screens.All[0].Bounds;
            for (int i = 1; i < window.Screens.All.Count; i++)
                combined = combined.Union(window.Screens.All[i].Bounds);
            return combined;
        }

        public static PixelRect? GetCurrentScreenWorkingArea(Window window)
        {
            return TopLevel.GetTopLevel(window)?.Screens.ScreenFromVisual(window)?.WorkingArea;
        }
    }
}
