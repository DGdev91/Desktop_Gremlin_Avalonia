using DesktopGremlin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
public static class SpriteManager
{
    //total conversion from the previous Spritemanager.
    //I was debating to add caching or not, but I think its better to have it.  
    //but I do have to set some limits depending on how many sprites will be used
    private static string _currentCharacter = null;
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
    public static int PlayAnimation(string sheetName, string actionType, int currentFrame,int frameCount, System.Windows.Controls.Image targetImage, bool PlayOnce = false)
    {
        BitmapImage sheet = Get(sheetName, actionType);
        if (sheet == null)
        {
            return currentFrame;
        }       
        if (frameCount <= 0)
        {
            MainWindow.ErrorClose($"Error Animation: {sheetName} action: {actionType} has invalid frame count","Animation Error", true);
        }

        int x = (currentFrame % Settings.SpriteColumn) * Settings.FrameWidth;
        int y = (currentFrame / Settings.SpriteColumn) * Settings.FrameHeight;

        if (x + Settings.FrameWidth > sheet.PixelWidth ||y + Settings.FrameHeight > sheet.PixelHeight)
        {
            return currentFrame;
        }

        targetImage.Source = new CroppedBitmap(sheet, new Int32Rect(x, y, Settings.FrameWidth, Settings.FrameHeight));

        return (currentFrame + 1) % frameCount;
    }
    public static BitmapImage Get(string animationName, string actionType)
    {
        //if (_currentCharacter != Settings.StartingChar)
        //{
        //    ClearCache();
        //    _currentCharacter = Settings.StartingChar;
        //}
        string cacheKey = $"{animationName}_{actionType}";

        if (Settings.AllowCache && _spriteCache.TryGetValue(cacheKey, out BitmapImage cached))
        {
            return cached;
        }

        string fileName = GetFileName(animationName);

        if (fileName == null)
        {
            MainWindow.ErrorClose($"Error Animation: {animationName} is missing", "Animation Missing", false);
            return null;
        }

        BitmapImage sheet = LoadSprite(Settings.StartingChar, fileName, actionType);

        if (Settings.AllowCache && sheet != null)
        {
            _spriteCache[cacheKey] = sheet;
        }

        return sheet;
    }
    private static string GetFileName(string animationName)
    {
        return _fileNameMap.TryGetValue(animationName, out string fileName) ? fileName : null;
    }
          
    private static BitmapImage LoadSprite(string filefolder, string fileName, string action, string rootFolder = "Gremlins")
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"SpriteSheet", rootFolder, filefolder,action, fileName);
        if (!File.Exists(path))
            return null;
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

