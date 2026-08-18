using Android.Content;
using DesktopGremlin;
using System.IO;

namespace DesktopGremlin.Droid;

/// <summary>
/// SpriteSheet/Sounds/config.txt ship as compressed APK assets, which SpriteManager/MediaManager
/// (LibVLC needs a real file path) can't read directly. Extract them once to app-private storage
/// and point AppPaths.BaseDirectory there, so the rest of Core never has to know it's on Android.
/// </summary>
public static class AndroidAssetExtractor
{
    private const string MarkerFileName = ".extracted";

    public static string EnsureExtracted(Context context)
    {
        string baseDir = context.FilesDir!.AbsolutePath;
        string markerPath = Path.Combine(baseDir, MarkerFileName);

        if (!File.Exists(markerPath))
        {
            CopyAssetTree(context, "SpriteSheet", Path.Combine(baseDir, "SpriteSheet"));
            CopyAssetTree(context, "Sounds", Path.Combine(baseDir, "Sounds"));
            CopyAssetFile(context, "config.txt", Path.Combine(baseDir, "config.txt"));
            File.WriteAllText(markerPath, "1");
        }

        return baseDir;
    }

    private static void CopyAssetTree(Context context, string assetPath, string destDir)
    {
        string[]? entries = context.Assets!.List(assetPath);
        if (entries == null || entries.Length == 0)
        {
            // Leaf file, not a directory.
            CopyAssetFile(context, assetPath, destDir);
            return;
        }

        Directory.CreateDirectory(destDir);
        foreach (string entry in entries)
        {
            CopyAssetTree(context, $"{assetPath}/{entry}", Path.Combine(destDir, entry));
        }
    }

    private static void CopyAssetFile(Context context, string assetPath, string destPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        using var input = context.Assets!.Open(assetPath);
        using var output = File.Create(destPath);
        input.CopyTo(output);
    }
}
