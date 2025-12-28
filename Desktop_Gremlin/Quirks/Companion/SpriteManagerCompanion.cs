using DesktopGremlin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
public static class SpriteManagerCompanion
{
    private static readonly Dictionary<string, BitmapImage> _spriteCache = new Dictionary<string, BitmapImage>();
    private static readonly Dictionary<string, string> _fileNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["idle"] = "idle.png",
        ["idle2"] = "idle2.png",
        ["intro"] = "intro.png",
        ["runleft"] = "runLeft.png",
        ["runright"] = "runRight.png",
        ["runup"] = "runUp.png",
        ["rundown"] = "runDown.png",
        ["outro"] = "outro.png",
        ["grab"] = "grab.png",
        ["runidle"] = "runIdle.png",
        ["click"] = "click.png",
        ["hover"] = "hover.png",
        ["sleep"] = "sleep.png",
        ["fireleft"] = "fireLeft.png",
        ["fireright"] = "fireRight.png",
        ["reload"] = "reload.png",
        ["pat"] = "pat.png",
        ["upleft"] = "upLeft.png",
        ["upright"] = "upRight.png",
        ["downleft"] = "downLeft.png",
        ["downright"] = "downRight.png",
        ["walkleft"] = "walkLeft.png",
        ["walkright"] = "walkRight.png",
        ["walkdown"] = "walkDown.png",
        ["walkup"] = "walkUp.png",
        ["emote1"] = "emote1.png",
        ["emote2"] = "emote2.png",
        ["emote3"] = "emote3.png",
        ["emote4"] = "emote4.png",
        ["sleeping"] = "sleep.png",
        ["jumpscare"] = "jumpScare.png",
        ["poof"] = "poof.png"
    };
    public static int PlayAnimation(string sheetName, string actionType, int currentFrame, int frameCount, System.Windows.Controls.Image targetImage)
    {
        BitmapImage sheet = GetSpriteSheet(sheetName, actionType);
        if (sheet == null)
        {
            return -1;
        }

        if (frameCount <= 0)
        {
            MainWindow.ErrorClose($"Error Animation: {sheetName} action: {actionType} has invalid frame count", "Animation Error", true);
            return currentFrame;
        }

        int x = (currentFrame % 10) * 300;
        int y = (currentFrame / 10) * 300;

        if (x + QuirkSettings.CompanionWidth > sheet.PixelWidth || y + QuirkSettings.CompanionHeight > sheet.PixelHeight)
        {
            return currentFrame;
        }

        targetImage.Source = new CroppedBitmap(sheet, new Int32Rect(x, y,  300, 300));

        return (currentFrame + 1) % frameCount;
    }
    public static BitmapImage GetSpriteSheet(string animationName, string actionType)
    {
        string cacheKey = $"{animationName}_{actionType}";

        if (Settings.AllowCache && _spriteCache.TryGetValue(cacheKey, out BitmapImage cached))
        {
            return cached;
        }

        if (!_fileNameMap.TryGetValue(animationName, out string fileName))
        {
            MainWindow.ErrorClose($"Error Animation: {animationName} is missing", "Animation Missing", false);
            return null;
        }

        BitmapImage sheet = LoadSprite(QuirkSettings.CompanionChar, fileName, actionType);

        if (Settings.AllowCache && sheet != null)
        {
            _spriteCache[cacheKey] = sheet;
        }

        return sheet;
    }
    private static BitmapImage LoadSprite(string filefolder, string fileName, string action)
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet" ,"Companions", filefolder, action, fileName);

        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }
}