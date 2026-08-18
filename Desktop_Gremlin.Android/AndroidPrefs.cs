using Android.Content;

namespace DesktopGremlin.Droid;

/// <summary>Small persisted settings that are Android-only (not part of Core's config.txt), so
/// they survive across app/service restarts without the user having to re-enter them.</summary>
public static class AndroidPrefs
{
    private const string PrefsName = "desktop_gremlin_prefs";
    private const string DisplayScaleKey = "display_scale";
    public const double DefaultDisplayScale = 0.5;

    public static double GetDisplayScale(Context context)
    {
        using var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        return prefs!.GetFloat(DisplayScaleKey, (float)DefaultDisplayScale);
    }

    public static void SetDisplayScale(Context context, double value)
    {
        using var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        using var editor = prefs!.Edit();
        editor!.PutFloat(DisplayScaleKey, (float)value);
        editor.Apply();
    }
}
