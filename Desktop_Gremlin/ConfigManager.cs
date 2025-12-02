using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Desktop_Gremlin;
using Mambo;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

public static class ConfigManager
{
    //TODO: Refactor this entire config manager to be more modular and easier to maintain.
    //No more giant switch statements.  
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
                case "LANGUAGE_DIFF":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.LanguageDiff = Value;
                        }
                    }
                    break;
                case "START_CHAR":
                    {
                        Settings.StartingChar = value;
                        break;
                    }
                case "COMPANION_CHAR":
                    {
                        Settings.CompanionChar = value;
                        break;
                    }
                case "SPRITE_FRAMERATE":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int intValue))
                        {
                            Settings.FrameRate = intValue;
                        }
                        break;
                    }
                case "FOLLOW_RADIUS":
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double intValue))
                        {
                            Settings.FollowRadius = intValue;
                        }
                        break;
                    }
                case "MAX_INTERVAL":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int intValue))
                        {
                            Settings.RandomMaxInterval = intValue;
                        }
                    }
                    break;
                case "MIN_INTERVAL":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int intValue))
                        {
                            Settings.RandomMinInterval = intValue;
                        }
                    }
                    break;
                case "RANDOM_MOVE_DISTANCE":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int intValue))
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
                case "SLEEP_TIME":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
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
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
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
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double Value))
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
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
                        {
                            Settings.MaxItemAcceleration = Value;
                        }
                    }
                    break;
                case "FOLLOW_ACCELERATION":
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double Value))
                        {
                            Settings.CurrentItemAcceleration = Value;
                        }
                    }
                    break;
                case "CURRENT_ACCELERATION":
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double Value))
                        {
                            Settings.ItemAcceleration = Value;
                        }
                    }
                    break;
                case "MAX_EATING_SIZE":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
                        {
                            Settings.FoodItemGetSize = Value;
                        }
                    }
                    break;
                case "ITEM_WIDTH":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
                        {
                            Settings.ItemWidth = Value;
                        }
                    }
                    break;
                case "ITEM_HEIGHT":
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out int Value))
                        {
                            Settings.ItemHeight = Value;
                        }
                    }
                    break;
                case "COMPANIONS_SCALE":
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double Value))
                        {
                            Settings.CompanionScale = Value;
                        }
                    }
                    break;
                case "ENABLE_MIN_RESIZE":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.EnableMinSize = Value;
                        }
                    }
                    break;
                case "FORCE_CENTER":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.ForceCenter = Value;
                        }
                    }
                    break;
                case "ENABLE_MANUAL_RESIZE":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.ManualReize = Value;
                        }
                    }
                    break;
                case "VOLUME_LEVEL":
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out double Value))
                        {
                            Settings.VolumeLevel = Value;
                        }
                    }
                    break;
                case "DISABLE_HOTSPOTS":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.DisableHotspots = Value;
                        }
                    }
                    break;
                case "START_BOTTOM":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.ForceBottomSpawn = Value;
                        }
                    }
                    break;
                case "ENABLE_GRAVITY":
                    {
                        if (bool.TryParse(value, out bool Value))
                        {
                            Settings.EnableGravity = Value;
                        }
                    }
                    break;
                case "GRAVITY_STRENGTH":
                    {
                        if (TryParseDoubleInvariant(value, out double Value))
                        {
                            Settings.SvGravity = Value;
                        }
                    }
                    break;
                case "SPRITE_SPEED":
                    {
                        if (TryParseDoubleInvariant(value, out double Value))
                        {
                            MouseSettings.Speed = Value;
                        }
                    }
                    break;
            }

        }
    }
    public static bool TryParseDoubleInvariant(string inputString, out double result)
    {
        if (Settings.LanguageDiff)
        {
            return double.TryParse(inputString, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
        else
        {
            return double.TryParse(inputString, out result);
        }
    }

    public static FrameCounts LoadConfigChar(string character)
    {
        var result = new FrameCounts();
        string path = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "SpriteSheet", "Gremlins", character, "config.txt");

        if (!File.Exists(path))
        {
            Gremlin.ErrorClose("Cannot find the SpriteSheet config.txt", "Missing config.txt", true);
            return result;
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

            if (!int.TryParse(value, CultureInfo.InvariantCulture, out int intValue))
            {
                continue;
            }
                
            switch (key.ToUpper())
            {
                case "INTRO": result.Intro = intValue; break;
                case "IDLE": result.Idle = intValue; break;
                case "IDLE2": result.Idle2 = intValue; break;
                case "RUNUP": result.Up = intValue; break;
                case "RUNDOWN": result.Down = intValue; break;
                case "RUNLEFT": result.Left = intValue; break;
                case "RUNRIGHT": result.Right = intValue; break;
                case "UPLEFT": result.UpLeft = intValue; break;
                case "UPRIGHT": result.UpRight = intValue; break;
                case "DOWNLEFT": result.DownLeft = intValue; break;
                case "DOWNRIGHT": result.DownRight = intValue; break;
                case "OUTRO": result.Outro = intValue; break;
                case "GRAB": result.Grab = intValue; break;
                case "RUNIDLE": result.RunIdle = intValue; break;
                case "CLICK": result.Click = intValue; break;
                case "HOVER": result.Hover = intValue; break;
                case "SLEEP": result.Sleep = intValue; break;
                case "FIREL": result.LeftFire = intValue; break;
                case "FIRER": result.RightFire = intValue; break;
                case "RELOAD": result.Reload = intValue; break;
                case "PAT": result.Pat = intValue; break;
                case "WALKLEFT": result.WalkL = intValue; break;
                case "WALKRIGHT": result.WalkR = intValue; break;
                case "WALKUP": result.WalkUp = intValue; break;
                case "WALKDOWN": result.WalkDown = intValue; break;
                case "EMOTE1": result.Emote1 = intValue; break;
                case "EMOTE2": result.Emote2 = intValue; break;
                case "EMOTE3": result.Emote3 = intValue; break;
                case "EMOTE4": result.Emote4 = intValue; break;
                case "JUMPSCARE": result.JumpScare = intValue; break;
                case "POOF": result.Poof = intValue; break;
                case "WIDTH": Settings.FrameWidth = intValue; break;
                case "HEIGHT": Settings.FrameHeight = intValue; break;
                case "COLUMN": Settings.SpriteColumn = intValue; break;
                case "WIDTHJS": Settings.FrameWidthJs = intValue; break;
                case "HEIGHTJS": Settings.FrameHeightJs = intValue; break;
            }
        }
        return result;
    }



    public static void ApplyXamlSettings(Window window)
    {
        if (window == null) return;
        Border LeftHotspot = window.FindControl<Border>("LeftHotspot");
        Border LeftDownHotspot = window.FindControl<Border>("LeftDownHotspot");
        Border RightHotspot = window.FindControl<Border>("RightHotspot");
        Border RightDownHotspot = window.FindControl<Border>("RightDownHotspot");
        Border TopHotspot = window.FindControl<Border>("TopHotspot");
        Image SpriteImage = window.FindControl<Image>("SpriteImage");
        if (LeftHotspot != null)
        {
            if (Settings.AllowColoredHotSpot && !Settings.DisableHotspots)
            {
                LeftHotspot.Background = new SolidColorBrush(Colors.Red);
                LeftDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
                RightHotspot.Background = new SolidColorBrush(Colors.Blue);
                RightDownHotspot.Background = new SolidColorBrush(Colors.Orange);
                TopHotspot.Background = new SolidColorBrush(Colors.Purple);
            }
            else
            {
                var noColor = (ImmutableSolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                LeftDownHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                RightDownHotspot.Background = noColor;
                TopHotspot.Background = noColor;
            }

            if (Settings.DisableHotspots)
            {
                LeftHotspot.IsEnabled = false;
                LeftDownHotspot.IsEnabled = false;
                RightDownHotspot.IsEnabled = false; 
                RightHotspot.IsEnabled = false;
                TopHotspot.IsEnabled = false;   
            }
        }
        window.ShowInTaskbar = Settings.ShowTaskBar;
        if (Settings.FakeTransparent)
        {
            window.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
        }
            
        if (Settings.ManualReize)
        {
            window.SizeToContent = SizeToContent.Manual;
        }
           
        if (Settings.ForceCenter)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }           
        if (SpriteImage == null)
        {
            return;
        }
  
        double originalWidth = SpriteImage.Width;
        double originalHeight = SpriteImage.Height;
        double newWidth = originalWidth * Settings.SpriteSize;
        double newHeight = originalHeight * Settings.SpriteSize;

        window.Width *= Settings.SpriteSize;
        window.Height *= Settings.SpriteSize;

        SpriteImage.Width = newWidth;
        SpriteImage.Height = newHeight;

        if (Settings.EnableMinSize)
        {
            window.MinWidth = window.Width;
            window.MinHeight = window.Height;
        }

        // Center the sprite
        double centerX = (window.Width - newWidth) / 2;
        double centerY = (window.Height - newHeight) / 2;
        SpriteImage.Margin = new Thickness(centerX, centerY, 0, 0);

        // Scale hotspots if they exist
        ScaleHotspotSafe(LeftHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(LeftDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(TopHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        if (Settings.ForceBottomSpawn)
        {
            Screen screen = window.Screens.ScreenFromVisual(window) ?? window.Screens.Primary;
            window.Position = new PixelPoint(
                (int)((screen.WorkingArea.Width - window.Width) / 2),
                (int)(screen.WorkingArea.Height - window.Height)
            );
        }
    }

    private static void ScaleHotspotSafe(Border hotspot, Image sprite, double centerX, double centerY, double scaleX, double scaleY)
    {
        if (hotspot == null || sprite == null) return;

        double offsetX = hotspot.Margin.Left - sprite.Margin.Left;
        double offsetY = hotspot.Margin.Top - sprite.Margin.Top;

        hotspot.Width *= scaleX;
        hotspot.Height *= scaleY;
        hotspot.Margin = new Thickness(centerX + offsetX * scaleX, centerY + offsetY * scaleY, 0, 0);
    }

    public class AppConfig
    {
        private Gremlin _gremlin; 
        private TrayIcon _trayIcon;
        public AnimationStates _states;    
        public AppConfig(Gremlin gremlin, AnimationStates states)
        {

            _gremlin = gremlin;
            _states = states;
            SetupTrayIcon();
        }   
        public void SetupTrayIcon()
        {
            _trayIcon = new TrayIcon();

            if (File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet/Gremlins/" + Settings.StartingChar + "/ico.ico")))
            {
                _trayIcon.Icon = new WindowIcon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet/Gremlins/" + Settings.StartingChar + "/ico.ico"));
            }
            else if (File.Exists("SpriteSheet/System/ico.ico"))
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

            NativeMenuItem restartItem = new NativeMenuItem("Restart");
            restartItem.Click += (_, __) => RestartApp();

            NativeMenuItemSeparator separator = new NativeMenuItemSeparator();

            var disableHotspotsItem = new NativeMenuItem("Disable Hotspots");
            disableHotspotsItem.ToggleType = NativeMenuItemToggleType.CheckBox;
            disableHotspotsItem.Click += (s, e) =>
            {
                disableHotspotsItem.IsChecked = !disableHotspotsItem.IsChecked;
                _gremlin.HotSpot();
            };

            var showHotspotsItem = new NativeMenuItem("Show Hotspots");
            disableHotspotsItem.ToggleType = NativeMenuItemToggleType.CheckBox;
            showHotspotsItem.Click += (s, e) =>
            {
                showHotspotsItem.IsChecked = !showHotspotsItem.IsChecked;
                _gremlin.ShowHotSpot();
            };


            menu.Items.Add(closeItem);
            menu.Items.Add(forceCloseItem);
            menu.Items.Add(restartItem);
            menu.Items.Add(separator);
            menu.Items.Add(disableHotspotsItem);
            menu.Items.Add(showHotspotsItem);

            _trayIcon.Menu = menu;
        }

        public void CloseApp()
        {
           _states.PlayOutro();  
            MediaManager.PlaySound("outro.wav", Settings.StartingChar); 
        }
        private void ForceClose()
        {
            Environment.Exit(1);
        }
        private void RestartApp()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath);
            Environment.Exit(1);
        }

      
    }

}

