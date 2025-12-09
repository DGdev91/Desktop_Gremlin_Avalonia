using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DesktopGremlin
{
    public static class ConfigManager
    {
        //TODO: Refactor this entire config manager to be more modular and easier to maintain.
        //No more giant switch statements.  
        public static void LoadMasterConfig()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
            if (!File.Exists(path))
            {
                MainWindow.ErrorClose("Cannot find the Main config.txt", "Missing config.txt", true);
                return;
            }

            var settingsMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["START_CHAR"] = val => Settings.StartingChar = val,
                ["COMPANION_CHAR"] = val => Settings.CompanionChar = val,
                ["SUMMON_CHAR"] = val => Settings.SummonChar = val,
                ["COMBAT_MODE_CHAR"] = val => Settings.CombatModeChar = val,
                ["SPRITE_FRAMERATE"] = val => { if (int.TryParse(val, out int v)) Settings.FrameRate = v; },
                ["FOLLOW_RADIUS"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Settings.FollowRadius = v; },
                ["MAX_INTERVAL"] = val => { if (int.TryParse(val, out int v)) Settings.RandomMaxInterval = v; },
                ["MIN_INTERVAL"] = val => { if (int.TryParse(val, out int v)) Settings.RandomMinInterval = v; },
                ["RANDOM_MOVE_DISTANCE"] = val => { if (int.TryParse(val, out int v)) Settings.MoveDistance = v; },
                ["ALLOW_RANDOM_ACTIONS"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowRandomness = v; },
                ["SLEEP_TIME"] = val => { if (int.TryParse(val, out int v)) Settings.SleepTime = v; },
                ["ALLOW_FOOTSTEP_SOUNDS"] = val => { if (bool.TryParse(val, out bool v)) Settings.FootStepSounds = v; },
                ["AMMO"] = val => { if (int.TryParse(val, out int v)) Settings.Ammo = v; },
                ["ALLOW_COLOR_HOTSPOT"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowColoredHotSpot = v; },
                ["SHOW_TASKBAR"] = val => { if (bool.TryParse(val, out bool v)) Settings.ShowTaskBar = v; },
                ["SPRITE_SCALE"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Settings.SpriteSize = v; },
                ["FORCE_FAKE_TRANSPARENT"] = val => { if (bool.TryParse(val, out bool v)) Settings.FakeTransparent = v; },
                ["ALLOW_ERROR_MESSAGES"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowErrorMessages = v; },
                ["MAX_ACCELERATION"] = val => { if (int.TryParse(val, out int v)) Quirks.MaxItemAcceleration = v; },
                ["FOLLOW_ACCELERATION"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Quirks.CurrentItemAcceleration = v; },
                ["CURRENT_ACCELERATION"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Quirks.ItemAcceleration = v; },
                ["MAX_EATING_SIZE"] = val => { if (int.TryParse(val, out int v)) Settings.FoodItemGetSize = v; },
                ["ITEM_WIDTH"] = val => { if (int.TryParse(val, out int v)) Settings.ItemWidth = v; },
                ["ITEM_HEIGHT"] = val => { if (int.TryParse(val, out int v)) Settings.ItemHeight = v; },
                ["COMPANIONS_SCALE"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Settings.CompanionScale = v; },
                ["ENABLE_MIN_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableMinSize = v; },
                ["FORCE_CENTER"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceCenter = v; },
                ["ENABLE_MANUAL_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.ManualReize = v; },
                ["VOLUME_LEVEL"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Settings.VolumeLevel = v; },
                ["DISABLE_HOTSPOTS"] = val => { if (bool.TryParse(val, out bool v)) Settings.DisableHotspots = v; },
                ["START_BOTTOM"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceBottomSpawn = v; },
                ["ENABLE_GRAVITY"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableGravity = v; },
                ["GRAVITY_STRENGTH"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) Settings.SvGravity = v; },
                ["ALLOW_CACHE"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowCache = v; },
                ["SPRITE_SPEED"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) MouseSettings.Speed = v; },
                ["ENABLE_KEYBOARD"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowKeyboard = v; },
                ["WALK_DISTANCE"] = val => { if (int.TryParse(val, out int v)) Settings.WalkDistance = v; },
                ["FOOD_MODE"] = val => Settings.FoodMode = val,
            };

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
                if (settingsMap.TryGetValue(key, out var setter))
                {
                    setter(value);
                }
            }
            settingsMap.Clear();
            settingsMap = null;
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
                window.Background = (ImmutableSolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
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
            private MainWindow _gremlin; 
            private TrayIcon _trayIcon;
            public string _selectedCharacter;
            public AnimationStates _states;    
            private List<string> _characterList;
            public AppConfig(MainWindow gremlin, AnimationStates states, string selectedCharacter)
            {

                _gremlin = gremlin;
                _states = states;
                _selectedCharacter = selectedCharacter;
                _characterList = LoadCharacterList();
                SetupTrayIcon();
            }

            private List<string> LoadCharacterList()
            {
                List<string> characterDirs = new List<string>();

                try
                {
                    string spriteSheetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet/Gremlins/");

                    if (Directory.Exists(spriteSheetFolder))
                    {
                        string[] subDirs = Directory.GetDirectories(spriteSheetFolder);
                        foreach (string subDir in subDirs)
                        {
                            characterDirs.Add(Path.GetFileName(subDir));
                        }
                    }
                    else
                    {
                        MainWindow.ErrorClose("Cannot find the SpriteSheet/Gremlins directory", "Missing SpriteSheet/Gremlins directory", false);
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorClose($"Error while loading characters directories: {ex.Message}", "Error loading characters directories", false);
                }

                return characterDirs;
            }

            public void SetupTrayIcon()
            {
                _trayIcon = new TrayIcon();
                SetIcon();

                _trayIcon.IsVisible = true;
                _trayIcon.ToolTipText = "Gremlin";

                NativeMenu menu = new NativeMenu();

                NativeMenuItem closeItem = new NativeMenuItem("Stylish Close");
                closeItem.Click += (_, __) => CloseApp();

                NativeMenuItem forceCloseItem = new NativeMenuItem("Force Close");
                forceCloseItem.Click += (_, __) => ForceClose();

                NativeMenuItem restartItem = new NativeMenuItem("Restart");
                restartItem.Click += (_, __) => RestartApp();

                NativeMenuItemSeparator separator1 = new NativeMenuItemSeparator();

                NativeMenuItem selectCharacterItem = new NativeMenuItem("Select Character");
                NativeMenu charactersMenu = new NativeMenu();
                foreach (string character in _characterList)
                {
                    NativeMenuItem menuItem = new NativeMenuItem(character);
                    menuItem.ToggleType = NativeMenuItemToggleType.Radio;
                    if (character.CompareTo(_selectedCharacter) == 0) menuItem.IsChecked = true;
                    else menuItem.IsChecked = false;

                    menuItem.Click += (sender, args) =>
                    {
                        NativeMenuItem oldItem = charactersMenu.Items.OfType<NativeMenuItem>().FirstOrDefault(p => p.Header.CompareTo(_selectedCharacter) == 0);
                        if (oldItem != null) oldItem.IsChecked = false;
                        _gremlin.SetSelectedCharacter(character);
                        _gremlin.PlayIntro();
                        SetIcon();
                        menuItem.IsChecked = true;
                    };

                    charactersMenu.Items.Add(menuItem);
                }
                selectCharacterItem.Menu = charactersMenu;

                NativeMenuItemSeparator separator2 = new NativeMenuItemSeparator();

                var disableHotspotsItem = new NativeMenuItem("Disable Hotspots");
                disableHotspotsItem.ToggleType = NativeMenuItemToggleType.CheckBox;
                disableHotspotsItem.Click += (s, e) =>
                {
                    disableHotspotsItem.IsChecked = !disableHotspotsItem.IsChecked;
                    _gremlin.HotSpot();
                };

                var showHotspotsItem = new NativeMenuItem("Show Hotspots");
                showHotspotsItem.ToggleType = NativeMenuItemToggleType.CheckBox;
                showHotspotsItem.Click += (s, e) =>
                {
                    showHotspotsItem.IsChecked = !showHotspotsItem.IsChecked;
                    _gremlin.ShowHotSpot();
                };


                menu.Items.Add(closeItem);
                menu.Items.Add(forceCloseItem);
                menu.Items.Add(restartItem);
                menu.Items.Add(separator1);
                menu.Items.Add(selectCharacterItem);
                menu.Items.Add(separator2);
                menu.Items.Add(disableHotspotsItem);
                menu.Items.Add(showHotspotsItem);

                _trayIcon.Menu = menu;
            }

            public void SetIcon()
            {
                if (File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet/Gremlins/" + _selectedCharacter + "/ico.ico")))
                {
                    _trayIcon.Icon = new WindowIcon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet/Gremlins/" + _selectedCharacter + "/ico.ico"));
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
                    MainWindow.ErrorClose("Cannot find the ico.ico in the application folder or SpriteSheet/System folder", "Missing ico.ico", false);
                }
            }

            public void CloseApp()
            {
                _states.PlayOutro();  
                MediaManager.PlaySound("outro.wav", _selectedCharacter); 
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

}