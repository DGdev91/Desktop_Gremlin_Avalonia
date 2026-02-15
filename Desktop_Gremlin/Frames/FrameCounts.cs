using DesktopGremlin;
using System;
using System.Globalization;
using System.IO;
namespace DesktopGremlin
{
    public class FrameCounts
    {
        public int LeftFire { get; set; } = 0;
        public int RightFire { get; set; } = 0;
        public int Intro { get; set; } = 0;
        public int Idle { get; set; } = 0;
        public int Idle2 { get; set; } = 0;
        public int Outro { get; set; } = 0;
        public int Down { get; set; } = 0;
        public int Up { get; set; } = 0;
        public int Right { get; set; } = 0;
        public int Left { get; set; } = 0;
        public int UpLeft { get; set; } = 0;
        public int UpRight { get; set; } = 0;
        public int DownLeft { get; set; } = 0;
        public int DownRight { get; set; } = 0;
        public int Grab { get; set; } = 0;
        public int WalkIdle { get; set; } = 0;
        public int Click { get; set; } = 0;
        public int Dance { get; set; } = 0;
        public int Hover { get; set; } = 0;
        public int Sleep { get; set; } = 0;
        public int Reload { get; set; } = 0;
        public int Pat { get; set; } = 0;
        public int WalkLeft { get; set; } = 0;
        public int WalkRight { get; set; } = 0;
        public int WalkUp { get; set; } = 0;
        public int WalkDown { get; set; } = 0;
        public int Emote1 { get; set; } = 0;
        public int Emote2 { get; set; } = 0;
        public int Emote3 { get; set; } = 0;
        public int Emote4 { get; set; } = 0;
        public int JumpScare { get; set; } = 0;
        public int Poof { get; set; } = 0;


        public void LoadConfigChar(string character, SpriteManager.CharacterType characterType = SpriteManager.CharacterType.Gremlin)
        {
            string rootFolder = "Gremlins";
            switch (characterType)
            {
                case SpriteManager.CharacterType.Gremlin:
                    //Resetting the values in the "settings" class to default, in case the config file does not have those
                    Settings.FrameWidth = 0;
                    Settings.FrameHeight = 0;
                    Settings.SpriteColumn = 0;
                    Settings.FrameWidthJs = 0;
                    Settings.FrameHeightJs = 0;
                    Settings.MirrorXSprite = false;
                    break;
                case SpriteManager.CharacterType.Companion:
                    rootFolder = "Companions";
                    break;
                case SpriteManager.CharacterType.Summon:
                    rootFolder = "Summons";
                    break;
            }
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet", rootFolder, character, "config.txt");

            if (!File.Exists(path))
            {
                MainWindow.ErrorClose("Cannot find the SpriteSheet config.txt", "Missing config.txt", true);
                return;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                    continue;

                var parts = line.Split('=');

                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if(key == "SCALE" && double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double spriteScale))
                {
                    Settings.SpriteSize = spriteScale;
                }

                if (!int.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue))
                {
                    continue;
                }

                switch (key.ToUpper())
                {
                    case "INTRO": Intro = intValue; break;
                    case "IDLE": Idle = intValue; break;
                    case "IDLE2": Idle2 = intValue; break;
                    case "RUNUP": Up = intValue; break;
                    case "RUNDOWN": Down = intValue; break;
                    case "RUNLEFT": Left = intValue; break;
                    case "RUNRIGHT": Right = intValue; break;
                    case "UPLEFT": UpLeft = intValue; break;
                    case "UPRIGHT": UpRight = intValue; break;
                    case "DOWNLEFT": DownLeft = intValue; break;
                    case "DOWNRIGHT": DownRight = intValue; break;
                    case "OUTRO": Outro = intValue; break;
                    case "GRAB": Grab = intValue; break;
                    case "RUNIDLE": WalkIdle = intValue; break;
                    case "CLICK": Click = intValue; break;
                    case "HOVER": Hover = intValue; break;
                    case "SLEEP": Sleep = intValue; break;
                    case "FIREL": LeftFire = intValue; break;
                    case "FIRER": RightFire = intValue; break;
                    case "RELOAD": Reload = intValue; break;
                    case "PAT": Pat = intValue; break;
                    case "WALKLEFT": WalkLeft = intValue; break;
                    case "WALKRIGHT": WalkRight = intValue; break;
                    case "WALKUP": WalkUp = intValue; break;
                    case "WALKDOWN": WalkDown = intValue; break;
                    case "EMOTE1": Emote1 = intValue; break;
                    case "EMOTE2": Emote2 = intValue; break;
                    case "EMOTE3": Emote3 = intValue; break;
                    case "EMOTE4": Emote4 = intValue; break;
                    case "JUMPSCARE": JumpScare = intValue; break;
                    case "POOF": Poof = intValue; break;
                    case "WIDTH": 
                        if (characterType == SpriteManager.CharacterType.Gremlin) Settings.FrameWidth = intValue; 
                        if (characterType == SpriteManager.CharacterType.Companion) QuirkSettings.CompanionWidth = intValue; 
                        break;
                    case "HEIGHT": 
                        if (characterType == SpriteManager.CharacterType.Gremlin) Settings.FrameHeight = intValue; 
                        if (characterType == SpriteManager.CharacterType.Companion) QuirkSettings.CompanionHeight = intValue; 
                        break;
                    case "COLUMN": if (characterType == SpriteManager.CharacterType.Gremlin) Settings.SpriteColumn = intValue; break;
                    case "WIDTHJS": if (characterType == SpriteManager.CharacterType.Gremlin) Settings.FrameWidthJs = intValue; break;
                    case "HEIGHTJS": if (characterType == SpriteManager.CharacterType.Gremlin) Settings.FrameHeightJs = intValue; break;
                    case "MIRRORXSPRITE": if (characterType == SpriteManager.CharacterType.Gremlin) Settings.MirrorXSprite = intValue == 1; break;
                }
            }
        }
    }
}