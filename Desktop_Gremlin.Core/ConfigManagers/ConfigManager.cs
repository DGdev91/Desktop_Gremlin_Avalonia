using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DesktopGremlin
{
    public static class ConfigManager
    {
        //TODO: Refactor this entire config manager to be more modular and easier to maintain.
        //No more giant switch statements.
        public static void LoadMasterConfig()
        {
            string path = System.IO.Path.Combine(AppPaths.BaseDirectory, "config.txt");
            if (!File.Exists(path))
            {
                AppErrors.Report("Cannot find the Main config.txt", "Missing config.txt", true);
                return;
            }

            var settingsMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["START_CHAR"] = val => Settings.StartingChar = val,
                //["FOOD_SPAWN"] = val => Settings.FoodSpawn = val, //Replaced by FOOD_MODE in avalonia port
                ["COMPANION_CHAR"] = val => QuirkSettings.CompanionChar = val,
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
                ["MAX_ACCELERATION"] = val => { if (int.TryParse(val, out int v)) QuirkSettings.MaxItemAcceleration = v; },
                ["FOLLOW_ACCELERATION"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) QuirkSettings.CurrentItemAcceleration = v; },
                ["CURRENT_ACCELERATION"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) QuirkSettings.ItemAcceleration = v; },
                ["MAX_EATING_SIZE"] = val => { if (int.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out int v)) Settings.FoodItemGetSize = v; },
                ["ITEM_WIDTH"] = val => { if (int.TryParse(val, out int v)) Settings.ItemWidth = v; },
                ["ITEM_HEIGHT"] = val => { if (int.TryParse(val, out int v)) Settings.ItemHeight = v; },
                ["COMPANIONS_SCALE"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) QuirkSettings.CompanionScale = v; },
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
                ["RANDOMIZE_SPAWN"] = val => { if (bool.TryParse(val, out bool v)) Settings.RandomizeSpawn = v; },
                ["STRAIGHT_MOVE"] = val => { if (bool.TryParse(val, out bool v)) Settings.StraightLine = v; },
                ["CLICK_THROUGH"] = val => { if (bool.TryParse(val, out bool v)) Settings.ClickThrough = v; },
                ["SPAWN_DISTANCE"] = val => { if (int.TryParse(val, out int v)) Settings.SpawnDistance = v; },
                ["COMPANION_CHAR"] = val => QuirkSettings.CompanionChar = val,
                ["COMPANION_SCALE"] = val => { if (double.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) QuirkSettings.CompanionScale = v; },
                ["COMPANION_FOLLOW"] = val => { if (int.TryParse(val, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out int v)) QuirkSettings.CompanionFollow = v; },
            };

            foreach (var rawLine in File.ReadAllLines(path))
            {
                // Strip trailing "// comment" before parsing, otherwise a line like
                // "ALLOW_CACHE = true //explanation" fails bool.TryParse and silently keeps
                // the setting at its default instead of applying the configured value.
                int commentIndex = rawLine.IndexOf("//", StringComparison.Ordinal);
                string line = commentIndex >= 0 ? rawLine.Substring(0, commentIndex) : rawLine;

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
    }
}
