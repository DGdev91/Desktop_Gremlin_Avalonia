using Android.Content;
using Avalonia;

namespace DesktopGremlin.Droid;

internal static class AndroidScreen
{
    public static PixelRect GetWorkingArea(Context context)
    {
        var metrics = context.Resources!.DisplayMetrics!;
        return new PixelRect(0, 0, metrics.WidthPixels, metrics.HeightPixels);
    }
}
