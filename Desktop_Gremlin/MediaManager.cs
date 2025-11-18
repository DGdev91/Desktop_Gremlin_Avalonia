using System;
using System.Collections.Generic;
using System.IO;
using LibVLCSharp.Shared;

public static class MediaManager
{
    private static LibVLC _libVLC;
    private static MediaPlayer _mediaPlayer;
    private static Dictionary<string, DateTime> LastPlayed = new Dictionary<string, DateTime>();

    static MediaManager()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
    }
    
    public static void PlaySound(string fileName, double delaySeconds = 0)
    {
        string path = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Sounds", Settings.StartingChar, fileName);

        if (!File.Exists(path))
            return;

        if (delaySeconds > 0)
        {
            if (LastPlayed.TryGetValue(fileName, out DateTime lastTime))
            {
                if ((DateTime.Now - lastTime).TotalSeconds < delaySeconds)
                    return;
            }
        }
        Media media = new Media(_libVLC, new Uri(path));
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
        LastPlayed[fileName] = DateTime.Now;
    }
}
