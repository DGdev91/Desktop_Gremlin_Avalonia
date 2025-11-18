using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Desktop_Gremlin;
using System;
using System.Diagnostics;
using System.IO;

public static class ConfigManager
{
    public static void LoadMasterConfig()
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        if (!File.Exists(path))
        {
            Gremlin.ErrorClose("Cannot find the Main config.txt", "Missing config.txt", true);
            return;
        }

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
            {
                continue;
            }

            var parts = line.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch (key.ToUpper())
            {
                case "START_CHAR":
                    {
                        Settings.StartingChar = value;
                        break;
                    }
                case "SPRITE_FRAMERATE":
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            Settings.FrameRate = intValue;
                        }
                        break;
                    }
                case "FOLLOW_RADIUS":
                    {
                        if (double.TryParse(value, out double intValue))
                        {
                            Settings.FollowRadius = intValue;
                        }
                        break;
                    }
                case "MAX_INTERVAL":
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            Settings.RandomMaxInterval = intValue;
                        }
                    }
                    break;
                case "MIN_INTERVAL":
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            Settings.RandomMinInterval = intValue;
                        }
                    }
                    break;
                case "RANDOM_MOVE_DISTANCE":
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            Settings.MoveDistance = intValue;
                        }
                    }
                    break;
                case "ALLOW_RANDOM_ACTIONS":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.AllowRandomness = Value;
                        }
                    }
                    break;
                case "ALLOW_GRAVITY":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.AllowGravity = Value;
                        }
                    }
                    break;
                case "SLEEP_TIME":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.SleepTime = Value;
                        }
                    }
                    break;
                case "ALLOW_FOOTSTEP_SOUNDS":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.FootStepSounds = Value;
                        }
                    }
                    break;
                case "AMMO":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.Ammo = Value;
                        }
                    }
                    break;

                case "ALLOW_COLOR_HOTSPOT":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.AllowColoredHotSpot = Value;
                        }
                    }
                    break;
                case "SHOW_TASKBAR":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.ShowTaskBar = Value;
                        }
                    }
                    break;
                case "SPRITE_SCALE":
                    {
                        if (double.TryParse(value, out double Value))
                        {
                            Settings.SpriteSize = Value;
                        }
                    }
                    break;
                case "FORCE_FAKE_TRANSPARENT":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.FakeTransparent = Value;
                        }
                    }
                    break;
                case "ALLOW_ERROR_MESSAGES":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.AllowErrorMessages = Value;
                        }
                    }
                    break;
                case "MAX_ACCELERATION":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.MaxItemAcceleration = Value;
                        }
                    }
                    break;
                case "FOLLOW_ACCELERATION":
                    {
                        if (double.TryParse(value, out double Value))
                        {
                            Settings.ItemAcceleration = Value;
                        }
                    }
                    break;
                case "CURRENT_ACCELERATION":
                    {
                        if (double.TryParse(value, out double Value))
                        {
                            Settings.ItemAcceleration = Value;
                        }
                    }
                    break;
                case "MAX_EATING_SIZE":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.FoodItemGetSize = Value;
                        }
                    }
                    break;
                case "ITEM_WIDTH":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.ItemWidth = Value;
                        }
                    }
                    break;
                case "ITEM_HEIGHT":
                    {
                        if (int.TryParse(value, out int Value))
                        {
                            Settings.ItemHeight = Value;
                        }
                    }
                    break;
            }

        }
    }
    
    public static void LoadConfigChar()
    {
        string path = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "SpriteSheet", "Gremlins", Settings.StartingChar, "config.txt");

        if (!File.Exists(path))
        {
            Gremlin.ErrorClose("Cannot find the SpriteSheet config.txt", "Missing config.txt", true );
            return;
        }

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
            {
                continue;
            }

            var parts = line.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            if (!int.TryParse(value, out int intValue))
            {
                continue;
            }
            switch (key.ToUpper())
            {
                case "INTRO":
                    FrameCounts.Intro = intValue;
                    break;
                case "IDLE":
                    FrameCounts.Idle = intValue;
                    break;
                case "IDLE2":
                    FrameCounts.Idle2 = intValue;
                    break;
                case "RUNUP":
                    FrameCounts.Up = intValue;
                    break;
                case "RUNDOWN":
                    FrameCounts.Down = intValue;
                    break;
                case "RUNLEFT":
                    FrameCounts.Left = intValue;
                    break;
                case "RUNRIGHT":
                    FrameCounts.Right = intValue;
                    break;
                case "UPLEFT":
                    FrameCounts.UpLeft = intValue;
                    break;
                case "UPRIGHT":
                    FrameCounts.UpRight = intValue;
                    break;
                case "DOWNLEFT":
                    FrameCounts.DownLeft = intValue;
                    break;
                case "DOWNRIGHT":
                    FrameCounts.DownRight = intValue;
                    break;
                case "OUTRO":
                    FrameCounts.Outro = intValue;
                    break;
                case "GRAB":
                    FrameCounts.Grab = intValue;
                    break;
                case "RUNIDLE":
                    FrameCounts.RunIdle = intValue;
                    break;
                case "CLICK":
                    FrameCounts.Click = intValue;
                    break;
                case "HOVER":
                    FrameCounts.Hover = intValue;
                    break;
                case "SLEEP":
                    FrameCounts.Sleep = intValue;
                    break;
                case "FIREL":
                    FrameCounts.LeftFire = intValue;
                    break;
                case "FIRER":
                    FrameCounts.RightFire = intValue;
                    break;
                case "RELOAD":
                    FrameCounts.Reload = intValue;
                    break;
                case "PAT":
                    FrameCounts.Pat = intValue;
                    break;             
                case "WALKLEFT":
                    FrameCounts.WalkL = intValue;
                    break;
                case "WALKRIGHT":
                    FrameCounts.WalkR = intValue;
                    break;
                case "WALKUP":
                    FrameCounts.WalkUp = intValue;
                    break;
                case "WALKDOWN":
                    FrameCounts.WalkDown = intValue;
                    break;
                case "EMOTE1":
                    FrameCounts.Emote1 = intValue;
                    break;
                case "EMOTE2":
                    FrameCounts.Emote2 = intValue;
                    break;
                case "EMOTE3":
                    FrameCounts.Emote3 = intValue;
                    break;
                case "EMOTE4":
                    FrameCounts.Emote4 = intValue;
                    break;
                case "JUMPSCARE":
                    FrameCounts.JumpScare = intValue;
                    break;
                case "WIDTH":
                    Settings.FrameWidth = intValue;
                    break;
                case "HEIGHT":
                    Settings.FrameHeight = intValue;
                    break;
                case "COLUMN":
                    Settings.SpriteColumn = intValue;
                    break;
                case "WIDTHJS":
                    Settings.FrameWidthJs = intValue;
                    break;
                case "HEIGHTJS":
                    Settings.FrameHeightJs = intValue;
                    break;
            }
        }
    }

    public static void ApplyXamlSettings(Gremlin window)
    {
        if (window == null)
        {
            return;
        }
        bool useColors = Settings.AllowColoredHotSpot;
        bool showTaskBar = Settings.ShowTaskBar;
        double scale = Settings.SpriteSize;

        ApplySettings(window, Settings.AllowColoredHotSpot, Settings.ShowTaskBar,Settings.SpriteSize,Settings.FakeTransparent );
    }
    private static void ApplySettings(Gremlin window, bool useColors, bool showTaskBar, double scale, bool useFakeTransparent)
    {
        Border LeftHotspot = window.LeftHotspot;
        Border LeftDownHotspot = window.LeftDownHotspot;
        Border RightHotspot = window.RightHotspot;
        Border RightDownHotspot = window.RightDownHotspot;
        Border TopHotspot = window.TopHotspot;
        Image SpriteImage = window.SpriteImage;

        if (useColors)
        {
            LeftHotspot.Background = new SolidColorBrush(Colors.Red);
            LeftDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
            RightHotspot.Background = new SolidColorBrush(Colors.Blue);
            RightDownHotspot.Background = new SolidColorBrush(Colors.Orange);
            TopHotspot.Background = new SolidColorBrush(Colors.Purple);
        }
        else
        {
            var noColor = (ImmutableSolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            LeftHotspot.Background = noColor;
            LeftDownHotspot.Background = noColor;
            RightHotspot.Background = noColor;
            RightDownHotspot.Background = noColor;
            TopHotspot.Background = noColor;
        }

        window.ShowInTaskbar = showTaskBar;

        if (useFakeTransparent)
        {
            window.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
        }

/*
        double baseLeftW = LeftHotspot.Width, baseLeftH = LeftHotspot.Height;
        double baseLeftDownW = LeftDownHotspot.Width, baseLeftDownH = LeftDownHotspot.Height;
        double baseRightW = RightHotspot.Width, baseRightH = RightHotspot.Height;
        double baseRightDownW = RightDownHotspot.Width, baseRightDownH = RightDownHotspot.Height;
        double baseTopW = TopHotspot.Width, baseTopH = TopHotspot.Height;

        double originalWidth = SpriteImage.Width;
        double originalHeight = SpriteImage.Height;

        double newWidth = originalWidth * scale;
        double newHeight = originalHeight * scale;
        window.Width = window.Height * scale;
        window.Height = window.Height * scale;  

        SpriteImage.Width = newWidth;
        SpriteImage.Height = newHeight;
        double leftHotspotOffsetX = LeftHotspot.Margin.Left - SpriteImage.Margin.Left;
        double leftHotspotOffsetY = LeftHotspot.Margin.Top - SpriteImage.Margin.Top;

        double leftDownOffsetX = LeftDownHotspot.Margin.Left - SpriteImage.Margin.Left;
        double leftDownOffsetY = LeftDownHotspot.Margin.Top - SpriteImage.Margin.Top;

        double rightOffsetX = RightHotspot.Margin.Left - SpriteImage.Margin.Left;
        double rightOffsetY = RightHotspot.Margin.Top - SpriteImage.Margin.Top;

        double rightDownOffsetX = RightDownHotspot.Margin.Left - SpriteImage.Margin.Left;
        double rightDownOffsetY = RightDownHotspot.Margin.Top - SpriteImage.Margin.Top;

        double topOffsetX = TopHotspot.Margin.Left - SpriteImage.Margin.Left;
        double topOffsetY = TopHotspot.Margin.Top - SpriteImage.Margin.Top;

        double centerX = (window.Width - newWidth) / 2;
        double centerY = (window.Height - newHeight) / 2;

        SpriteImage.Margin = new Thickness(centerX, centerY, 0, 0);

        double scaleX = newWidth / originalWidth;
        double scaleY = newHeight / originalHeight;
        ScaleHotspot(LeftHotspot, leftHotspotOffsetX, leftHotspotOffsetY, scaleX, scaleY, centerX, centerY, baseLeftW, baseLeftH);
        ScaleHotspot(LeftDownHotspot, leftDownOffsetX, leftDownOffsetY, scaleX, scaleY, centerX, centerY, baseLeftDownW, baseLeftDownH);
        ScaleHotspot(RightHotspot, rightOffsetX, rightOffsetY, scaleX, scaleY, centerX, centerY, baseRightW, baseRightH);
        ScaleHotspot(RightDownHotspot, rightDownOffsetX, rightDownOffsetY, scaleX, scaleY, centerX, centerY, baseRightDownW, baseRightDownH);
        ScaleHotspot(TopHotspot, topOffsetX, topOffsetY, scaleX, scaleY, centerX, centerY, baseTopW, baseTopH);
        */
    }

    private static void ScaleHotspot(Border hotspot, double offsetX, double offsetY, double scaleX,
    double scaleY, double centerX, double centerY, double baseWidth, double baseHeight)
    {
        hotspot.Width = baseWidth * scaleX;
        hotspot.Height = baseHeight * scaleY;
        hotspot.Margin = new Thickness(centerX + offsetX * scaleX, centerY + offsetY * scaleY, 0, 0);
        
    }
    public class AppConfig
    {
        private readonly Window _window;
        private TrayIcon _trayIcon;
        public AppConfig(Window window)
        {
            _window = window;
            SetupTrayIcon();
        }   
        public void SetupTrayIcon()
        {
            _trayIcon = new TrayIcon();

            if (File.Exists("SpriteSheet/System/ico.ico"))
            {
                _trayIcon.Icon = new WindowIcon("SpriteSheet/System/ico.ico");
            }
            else if (File.Exists("ico.ico"))
            {
                _trayIcon.Icon = new WindowIcon("ico.ico");
            }
            else
            {
                Gremlin.ErrorClose("Cannot find the ico.ico in the application folder or SpriteSheet/System folder", "Missing ico.ico", false);
            }

            _trayIcon.IsVisible = true;
            _trayIcon.ToolTipText = "Gremlin";

            NativeMenu menu = new NativeMenu();

            NativeMenuItem closeItem = new NativeMenuItem("Stylish Close");
            closeItem.Click += (_, __) => CloseApp();

            NativeMenuItem forceCloseItem = new NativeMenuItem("Force Close");
            forceCloseItem.Click += (_, __) => ForceClose();

            NativeMenuItem reappearItem = new NativeMenuItem("Reappear");
            reappearItem.Click += (_, __) => RestartApp();

            menu.Items.Add(closeItem);
            menu.Items.Add(forceCloseItem);
            menu.Items.Add(reappearItem);

            _trayIcon.Menu = menu;
        }

        private void CloseApp()
        {
            AnimationStates.PlayOutro();    
        }
        private void ForceClose()
        {
            Environment.Exit(1);
        }
        private void RestartApp()
        {
            AnimationStates.PlayOutro();
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath);
            Environment.Exit(1);
        }
    }

}

