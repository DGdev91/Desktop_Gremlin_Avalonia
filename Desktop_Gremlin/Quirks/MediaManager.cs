using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Media;

public static class MediaManager
{
    private static Dictionary<string, DateTime> LastPlayed = new Dictionary<string, DateTime>();
    private static List<MediaPlayer> ActivePlayers = new List<MediaPlayer>();
    private static MediaPlayer mp = new MediaPlayer();
    public static void PlaySound(string fileName, string startChar, double delaySeconds = 0, double volume = 1.0)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Sounds", startChar, fileName);

        if (!File.Exists(path))
        {
            return;
        }
        if (delaySeconds > 0 && LastPlayed.TryGetValue(fileName, out DateTime lastTime) && (DateTime.Now - lastTime).TotalSeconds < delaySeconds)
        {
            return;
        }
        if (Settings.UseWPF)
        {
            PlayWpf(path);
        }
        else
        {
            PlaySoundPlayer(path);
        }
        LastPlayed[fileName] = DateTime.Now;
    }
    private static void PlayWpf(string path)
    {
        //MediaPlayer mp = new MediaPlayer(); //Instancing for when I want to bombared the user with random noice
        mp.Open(new Uri(path));
        mp.Volume = Settings.VolumeLevel;

        mp.MediaEnded += (s, e) =>
        {
            mp.Stop();
            mp.Close();
            ActivePlayers.Remove(mp);
        };

        ActivePlayers.Add(mp);
        mp.Play();
    }
    private static void PlaySoundPlayer(string path)
    {
        SoundPlayer sp = new SoundPlayer(path);
        sp.Play();
    }
 
}


