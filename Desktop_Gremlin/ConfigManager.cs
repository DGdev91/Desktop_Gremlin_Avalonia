using Desktop_Gremlin;
using DesktopGremlin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
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
        };
        var settingsMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["LANGUAGE_DIFF"] = val => { if (bool.TryParse(val, out bool v)) Settings.LanguageDiff = v; },
            ["START_CHAR"] = val => Settings.StartingChar = val,
            ["COMPANION_CHAR"] = val => Settings.CompanionChar = val,
            ["SPRITE_FRAMERATE"] = val => { if (int.TryParse(val, out int v)) Settings.FrameRate = v; },
            ["FOLLOW_RADIUS"] = val => { if (TryParseDoubleInvariant(val, out double v)) Settings.FollowRadius = v; },
            ["MAX_INTERVAL"] = val => { if (int.TryParse(val, out int v)) Settings.RandomMaxInterval = v; },
            ["MIN_INTERVAL"] = val => { if (int.TryParse(val, out int v)) Settings.RandomMinInterval = v; },
            ["RANDOM_MOVE_DISTANCE"] = val => { if (int.TryParse(val, out int v)) Settings.MoveDistance = v; },
            ["ALLOW_RANDOM_ACTIONS"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowRandomness = v; },
            ["SLEEP_TIME"] = val => { if (int.TryParse(val, out int v)) Settings.SleepTime = v; },
            ["ALLOW_FOOTSTEP_SOUNDS"] = val => { if (bool.TryParse(val, out bool v)) Settings.FootStepSounds = v; },
            ["AMMO"] = val => { if (int.TryParse(val, out int v)) Settings.Ammo = v; },
            ["ALLOW_COLOR_HOTSPOT"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowColoredHotSpot = v; },
            ["SHOW_TASKBAR"] = val => { if (bool.TryParse(val, out bool v)) Settings.ShowTaskBar = v; },
            ["SPRITE_SCALE"] = val => { if (TryParseDoubleInvariant(val, out double v)) Settings.SpriteSize = v; },
            ["FORCE_FAKE_TRANSPARENT"] = val => { if (bool.TryParse(val, out bool v)) Settings.FakeTransparent = v; },
            ["ALLOW_ERROR_MESSAGES"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowErrorMessages = v; },
            ["MAX_ACCELERATION"] = val => { if (int.TryParse(val, out int v)) Quirks.MaxItemAcceleration = v; },
            ["FOLLOW_ACCELERATION"] = val => { if (TryParseDoubleInvariant(val, out double v)) Quirks.CurrentItemAcceleration = v; },
            ["CURRENT_ACCELERATION"] = val => { if (TryParseDoubleInvariant(val, out double v)) Quirks.ItemAcceleration = v; },
            ["ITEM_WIDTH"] = val => { if (int.TryParse(val, out int v)) Settings.ItemWidth = v; },
            ["ITEM_HEIGHT"] = val => { if (int.TryParse(val, out int v)) Settings.ItemHeight = v; },
            ["COMPANIONS_SCALE"] = val => { if (TryParseDoubleInvariant(val, out double v)) Settings.CompanionScale = v; },
            ["ENABLE_MIN_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableMinSize = v; },
            ["FORCE_CENTER"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceCenter = v; },
            ["ENABLE_MANUAL_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.ManualReize = v; },
            ["VOLUME_LEVEL"] = val => { if (TryParseDoubleInvariant(val, out double v)) Settings.VolumeLevel = v; },
            ["DISABLE_HOTSPOTS"] = val => { if (bool.TryParse(val, out bool v)) Settings.DisableHotspots = v; },
            ["START_BOTTOM"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceBottomSpawn = v; },
            ["ENABLE_GRAVITY"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableGravity = v; },
            ["GRAVITY_STRENGTH"] = val => { if (TryParseDoubleInvariant(val, out double v)) Settings.SvGravity = v; },
            ["ALLOW_CACHE"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowCache = v; },
            ["SPRITE_SPEED"] = val => { if (TryParseDoubleInvariant(val, out double v)) MouseSettings.Speed = v; },
            ["ENABLE_KEYBOARD"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowKeyboard = v; },
            ["WALK_DISTANCE"] = val => { if (int.TryParse(val, out int v)) Settings.WalkDistance = v; },
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
    public static void ApplyXamlSettings(Window window)
    {
        if (window == null)
        {
            return;
        }
        Border LeftHotspot = window.FindName("LeftHotspot") as Border;
        Border LeftDownHotspot = window.FindName("LeftDownHotspot") as Border;
        Border RightHotspot = window.FindName("RightHotspot") as Border;
        Border RightDownHotspot = window.FindName("RightDownHotspot") as Border;
        Border TopHotspot = window.FindName("TopHotspot") as Border;
        Image SpriteImage = window.FindName("SpriteImage") as Image;
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
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
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

        double centerX = (window.Width - newWidth) / 2;
        double centerY = (window.Height - newHeight) / 2;
        SpriteImage.Margin = new Thickness(centerX, centerY, 0, 0);

        ScaleHotspotSafe(LeftHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(LeftDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(TopHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        if (Settings.ForceBottomSpawn)
        {
            window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
            window.Top = SystemParameters.WorkArea.Bottom - window.Height;
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
        private NotifyIcon _trayIcon;
        public AnimationStates _states;
        public AppConfig(MainWindow gremlin, AnimationStates states)
        {

            _gremlin = gremlin;
            _states = states;
            SetupTrayIcon();
        }
        public void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon();

            if (File.Exists("SpriteSheet/System/ico.ico"))
            {
                _trayIcon.Icon = new Icon("SpriteSheet/System/ico.ico");
            }
            else
            {
                _trayIcon.Icon = SystemIcons.Application;
            }

            _trayIcon.Visible = true;
            _trayIcon.Text = "Gremlin";

            var menu = new ContextMenuStrip();
            menu.Items.Add("Stylish Close", null, (s, e) => CloseApp());
            menu.Items.Add("Force Close", null, (s, e) => ForceClose());
            menu.Items.Add("Restart", null, (s, e) => RestartApp());
            menu.Items.Add(new ToolStripSeparator());

            var disableHotspotsItem = menu.Items.Add("Disable Hotspots");
            var showHotspotsItem = menu.Items.Add("Show Hotspots");
            var enableGravity = menu.Items.Add("Toggle Gravity");
            var enableClickThrough = menu.Items.Add("Toggle Click Through");
            var disableItem = disableHotspotsItem as ToolStripMenuItem;
            var showItem = showHotspotsItem as ToolStripMenuItem;
            var gravity = enableGravity as ToolStripMenuItem; 
            disableItem.Click += (s, e) =>
            {
                disableItem.Checked = !disableItem.Checked;
                _gremlin.HotSpot();
            };

            showItem.Click += (s, e) =>
            {
                showItem.Checked = !showItem.Checked;
                _gremlin.ShowHotSpot();
            };
            enableClickThrough.Click += (s, e) =>
            {
                _gremlin.ToggleClickThrough();
            };
            gravity.Click += (s, e) =>
            {
                _gremlin.ToggleGravity();  
            };
            _trayIcon.ContextMenuStrip = menu;
        }

        public void CloseApp()
        {
            _states.PlayOutro();
            MediaManager.PlaySound("outro.wav", Settings.StartingChar);
        }
        private void ForceClose()
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void RestartApp()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath);
            System.Windows.Application.Current.Shutdown();
        }


    }

}



