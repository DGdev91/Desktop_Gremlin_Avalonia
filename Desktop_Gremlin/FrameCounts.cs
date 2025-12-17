using DesktopGremlin;
using System;
using System.Globalization;
using System.IO;
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


    public void LoadConfigChar(string character)
    {
        string path = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "SpriteSheet", "Gremlins", character, "config.txt");

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

            if(key == "SCALE")
            {
                if (TryParseDoubleInvariant(value, out double spriteScale))
                {
                    Settings.SpriteSize = spriteScale;
                }
            }

            if (!int.TryParse(value, out int intValue))
            {
                continue;
            }

            switch (key.ToUpper())
            {
                case "INTRO": this.Intro = intValue; break;
                case "IDLE": this.Idle = intValue; break;
                case "IDLE2": this.Idle2 = intValue; break;
                case "RUNUP": this.Up = intValue; break;
                case "RUNDOWN": this.Down = intValue; break;
                case "RUNLEFT": this.Left = intValue; break;
                case "RUNRIGHT": this.Right = intValue; break;
                case "UPLEFT": this.UpLeft = intValue; break;
                case "UPRIGHT": this.UpRight = intValue; break;
                case "DOWNLEFT": this.DownLeft = intValue; break;
                case "DOWNRIGHT": this.DownRight = intValue; break;
                case "OUTRO": this.Outro = intValue; break;
                case "GRAB": this.Grab = intValue; break;
                case "RUNIDLE": this.WalkIdle = intValue; break;
                case "CLICK": this.Click = intValue; break;
                case "HOVER": this.Hover = intValue; break;
                case "SLEEP": this.Sleep = intValue; break;
                case "FIREL": this.LeftFire = intValue; break;
                case "FIRER": this.RightFire = intValue; break;
                case "RELOAD": this.Reload = intValue; break;
                case "PAT": this.Pat = intValue; break;
                case "WALKLEFT": this.WalkLeft = intValue; break;
                case "WALKRIGHT": this.WalkRight = intValue; break;
                case "WALKUP": this.WalkUp = intValue; break;
                case "WALKDOWN": this.WalkDown = intValue; break;
                case "EMOTE1": this.Emote1 = intValue; break;
                case "EMOTE2": this.Emote2 = intValue; break;
                case "EMOTE3": this.Emote3 = intValue; break;
                case "EMOTE4": this.Emote4 = intValue; break;
                case "JUMPSCARE": this.JumpScare = intValue; break;
                case "POOF": this.Poof = intValue; break;
                case "WIDTH": Settings.FrameWidth = intValue; break;
                case "HEIGHT": Settings.FrameHeight = intValue; break;
                case "COLUMN": Settings.SpriteColumn = intValue; break;
                case "WIDTHJS": Settings.FrameWidthJs = intValue; break;
                case "HEIGHTJS": Settings.FrameHeightJs = intValue; break;                       
            }
        }
    }
    public static bool TryParseDoubleInvariant(string input, out double result)
    {
        if (Settings.LanguageDiff)
        {
            return double.TryParse(input, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
        else
        {
            return double.TryParse(input, out result);
        }
    }
}
