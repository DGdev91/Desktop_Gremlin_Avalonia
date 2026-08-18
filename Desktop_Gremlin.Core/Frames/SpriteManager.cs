using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace DesktopGremlin
{
    public static class SpriteManager
    {
        private static string _currentCharacter = null;
        private static readonly Dictionary<string, Bitmap> _spriteCache = new Dictionary<string, Bitmap>();
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
        public enum CharacterType
        {
            Gremlin,
            Companion,
            Summon
        }
        public static int PlayAnimation(string sheetName,string actionType , int currentFrame, int frameCount, Image targetImage, string character, bool PlayOnce = false, CharacterType characterType = CharacterType.Gremlin)
        {
            int frameWidth = characterType == CharacterType.Companion ? QuirkSettings.CompanionWidth : Settings.FrameWidth;
            int frameHeight = characterType == CharacterType.Companion ? QuirkSettings.CompanionHeight : Settings.FrameHeight;
            Bitmap sheet = GetSpriteSheet(sheetName, actionType, character, characterType);

            if (sheet == null)
            {
                return -1;
            }

            // frameCount <= 0 just means this character doesn't have this particular animation
            // configured - a common, intentional choice (e.g. Exu's config sets HOVER=0, PAT=0,
            // EMOTE2=0, EMOTE4=0), not a broken/missing asset. Skip it quietly instead of treating
            // it as fatal - that used to pop an error dialog that closes the whole app the moment
            // the mouse merely hovers over a character that opted out of a hover animation.
            if (frameCount <= 0)
            {
                return currentFrame;
            }

            int x = (currentFrame % Settings.SpriteColumn) * frameWidth;
            int y = (currentFrame / Settings.SpriteColumn) * frameHeight;

            if (x + frameWidth > sheet.PixelSize.Width ||y + frameHeight > sheet.PixelSize.Height)
            {
                return currentFrame;
            }
            if (!Settings.AllowCache)
            {
                CroppedBitmap oldImage = targetImage.Source as CroppedBitmap;
                if (oldImage != null) oldImage.Dispose();
            }
            targetImage.Source = new CroppedBitmap(sheet, new PixelRect(x, y, frameWidth, frameHeight));
            return (currentFrame + 1) % frameCount;
        }
        public static Bitmap GetSpriteSheet(string animationName, string actionType, string character, CharacterType characterType = CharacterType.Gremlin)
        {
            string cacheKey = $"{animationName}_{actionType}_{character}";

            if (Settings.AllowCache && _spriteCache.TryGetValue(cacheKey, out Bitmap cached))
            {
                return cached;
            }

            if (!_fileNameMap.TryGetValue(animationName, out string fileName))
            {
                AppErrors.Report($"Error Animation: {animationName} is missing", "Animation Missing", false);
                return null;
            }

            string rootFolder = "Gremlins";
            switch (characterType)
            {
                case CharacterType.Companion:
                    rootFolder = "Companions";
                    break;
                case CharacterType.Summon:
                    rootFolder = "Summons";
                    break;
            }
            Bitmap sheet = LoadSprite(character, fileName, actionType, rootFolder);
            if (sheet != null)
            {
                _spriteCache[cacheKey] = sheet;
            }

            return sheet;
        }
          
        private static Bitmap LoadSprite(string filefolder, string fileName, string action, string rootFolder)
        {
            string path = System.IO.Path.Combine(AppPaths.BaseDirectory, "SpriteSheet", rootFolder, filefolder, action, fileName);
            if (!File.Exists(path))
                return null;
            try
            {
                using var stream = File.OpenRead(path);
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        }

        public static int PlayEffect(string sheetName, string actionType, int currentFrame, int frameCount, Image targetImage, string character, bool PlayOnce = false, CharacterType characterType = CharacterType.Gremlin)
        {
            Bitmap sheet = GetSpriteSheet(sheetName, actionType, character, characterType);
            if (sheet == null)
            {
                return currentFrame;
            }
            // Same as PlayAnimation above: frameCount <= 0 just means this character doesn't have
            // this animation configured. Must return before the % frameCount below - it used to
            // run that division anyway (after firing an error dialog that would have already
            // closed the app when messages are enabled), a divide-by-zero crash the moment someone
            // disables ALLOW_ERROR_MESSAGES.
            if (frameCount <= 0)
            {
                return currentFrame;
            }
            int x = (currentFrame % Settings.SpriteColumn) * Settings.FrameWidth;
            int y = (currentFrame / Settings.SpriteColumn) * Settings.FrameHeight;

            if (x + Settings.FrameWidth > sheet.PixelSize.Width || y + Settings.FrameHeight > sheet.PixelSize.Height)
            {
                return currentFrame;
            }
            if (!Settings.AllowCache)
            {
                CroppedBitmap oldImage = targetImage.Source as CroppedBitmap;
                if (oldImage != null) oldImage.Dispose();
            }
            targetImage.Source = new CroppedBitmap(sheet, new PixelRect(x, y, Settings.FrameWidth, Settings.FrameHeight));
            return (currentFrame + 1) % frameCount;
        }

    }
}