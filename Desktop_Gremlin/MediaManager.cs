using System;
using System.Collections.Generic;
using System.IO;
using LibVLCSharp.Shared;

public static class MediaManager
{
    private static LibVLC _libVLC;
    private static Dictionary<string, DateTime> LastPlayed = new Dictionary<string, DateTime>();
    private static MediaPlayer player;

    static MediaManager()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        player = new MediaPlayer(_libVLC);
    }
    
    public static void PlaySound(string fileName, string startChar, double delaySeconds = 0, double volume = 1.0)
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", startChar, fileName);

        if (!File.Exists(path)) return;

        if (delaySeconds > 0 &&
            LastPlayed.TryGetValue(fileName, out DateTime lastTime) &&
            (DateTime.Now - lastTime).TotalSeconds < delaySeconds)
        {
            return;
        }
        Media media = new Media(_libVLC, new Uri(path));
        player.Media = media;
        player.Volume = (int)Math.Round(Settings.VolumeLevel);   
        player.Play();
        LastPlayed[fileName] = DateTime.Now;
    }
}

