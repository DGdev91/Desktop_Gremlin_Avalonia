using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DesktopGremlin
{
    /// <summary>
    /// Applies AppConfig settings to an actual OS Window (sizing, position, hotspot colors, tray
    /// icon). Desktop-only: it depends on Avalonia.Controls.Window and TrayIcon, neither of which
    /// exist on Android. The platform-agnostic parsing of config.txt itself lives in
    /// Desktop_Gremlin.Core's ConfigManager.LoadMasterConfig.
    /// </summary>
    public static class DesktopWindowConfig
    {
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
            if (Settings.RandomizeSpawn)
            {
                SpawnNearCenter(window);
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
        private static void SpawnNearCenter(Window window)
        {
            if (window == null)
            {
                return;
            }

            Screen screen = window.Screens.ScreenFromVisual(window) ?? window.Screens.Primary;
            PixelRect workArea = screen.WorkingArea;

            double centerX = workArea.X + (workArea.Width - window.Width) / 2;
            double centerY = workArea.Y + (workArea.Height - window.Height) / 2;

            Random rng = new();

            double offsetX = rng.Next(-Settings.SpawnDistance, Settings.SpawnDistance + 1);
            double offsetY = rng.Next(-Settings.SpawnDistance, Settings.SpawnDistance + 1);

            double left = centerX + offsetX;
            double top = centerY + offsetY;

            left = Math.Max(workArea.X, Math.Min(left, workArea.Right - window.Width));
            top = Math.Max(workArea.Y, Math.Min(top, workArea.Bottom - window.Height));

            window.Position = new PixelPoint((int)left, (int)top);
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
                    string spriteSheetFolder = System.IO.Path.Combine(AppPaths.BaseDirectory, "SpriteSheet/Gremlins/");

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
                        AppErrors.Report("Cannot find the SpriteSheet/Gremlins directory", "Missing SpriteSheet/Gremlins directory", false);
                    }
                }
                catch (Exception ex)
                {
                    AppErrors.Report($"Error while loading characters directories: {ex.Message}", "Error loading characters directories", false);
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
                    menuItem.ToggleType = MenuItemToggleType.Radio;
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

                NativeMenuItem disableHotspotsItem = new NativeMenuItem("Disable Hotspots");
                disableHotspotsItem.ToggleType = MenuItemToggleType.CheckBox;
                disableHotspotsItem.Click += (s, e) =>
                {
                    disableHotspotsItem.IsChecked = !disableHotspotsItem.IsChecked;
                    _gremlin.HotSpot();
                };

                NativeMenuItem showHotspotsItem = new NativeMenuItem("Show Hotspots");
                showHotspotsItem.ToggleType = MenuItemToggleType.CheckBox;
                showHotspotsItem.Click += (s, e) =>
                {
                    showHotspotsItem.IsChecked = !showHotspotsItem.IsChecked;
                    _gremlin.ShowHotSpot();
                };

                NativeMenuItem enableGravity = new NativeMenuItem("Toggle Gravity");
                enableGravity.ToggleType = MenuItemToggleType.CheckBox;
                enableGravity.IsChecked = Settings.EnableGravity;
                enableGravity.Click += (s, e) =>
                {
                    enableGravity.IsChecked = !enableGravity.IsChecked;
                    _gremlin.ToggleGravity();
                    Quirks.Companion.Companion companionInstance = _gremlin.GetCompanionInstance();
                    if (companionInstance != null)
                    {
                        companionInstance.ToggleGravity();
                    }
                };

                //For now, there click trough doesn't fully work on Avalonia in some configurations (ex. Linux Wayland).
                // leaving thhis commented out until I can find a better solution or until Avalonia implements a proper click trough solution that works across all platforms.
                /*
                NativeMenuItem enableClickThrough = new NativeMenuItem("Toggle Click Through");
                enableClickThrough.ToggleType = MenuItemToggleType.CheckBox;
                enableClickThrough.Click += (s, e) =>
                {
                    enableClickThrough.IsChecked = !enableClickThrough.IsChecked;
                    _gremlin.ToggleClickThrough();
                };
                */

                menu.Items.Add(closeItem);
                menu.Items.Add(forceCloseItem);
                menu.Items.Add(restartItem);
                menu.Items.Add(separator1);
                menu.Items.Add(selectCharacterItem);
                menu.Items.Add(separator2);
                menu.Items.Add(disableHotspotsItem);
                menu.Items.Add(showHotspotsItem);
                menu.Items.Add(enableGravity);
                //menu.Items.Add(enableClickThrough);

                _trayIcon.Menu = menu;
            }

            public void SetIcon()
            {
                if (File.Exists(System.IO.Path.Combine(AppPaths.BaseDirectory, "SpriteSheet/Gremlins/" + _selectedCharacter + "/ico.ico")))
                {
                    _trayIcon.Icon = new WindowIcon(System.IO.Path.Combine(AppPaths.BaseDirectory, "SpriteSheet/Gremlins/" + _selectedCharacter + "/ico.ico"));
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
                    AppErrors.Report("Cannot find the ico.ico in the application folder or SpriteSheet/System folder", "Missing ico.ico", false);
                }
            }

            public void CloseApp()
            {
                _states.PlayOutro();
                Quirks.MediaManager.PlaySound("outro.wav", _selectedCharacter);
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
