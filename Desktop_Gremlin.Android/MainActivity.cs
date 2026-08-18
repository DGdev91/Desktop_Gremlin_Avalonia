using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using DesktopGremlin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AndroidSettings = Android.Provider.Settings;

namespace DesktopGremlin.Droid;

[Activity(Label = "Desktop Gremlin", MainLauncher = true, Exported = true)]
public class MainActivity : Activity
{
    private TextView _status = null!;
    private Button _startButton = null!;
    private Button _stopButton = null!;
    private Button _stylishStopButton = null!;
    private EditText _sizeInput = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // So the character list below has something to read even if the service was never
        // started yet - idempotent (checks a marker file) and safe to call from here too.
        string baseDir = AndroidAssetExtractor.EnsureExtracted(this);
        AppPaths.BaseDirectory = baseDir + "/";
        ConfigManager.LoadMasterConfig();

        RequestNotificationPermission();
        if (!AndroidSettings.CanDrawOverlays(this)) RequestOverlayPermission();

        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(40, 40, 40, 40);

        // Android 15+ (API 35+) enforces edge-to-edge for apps targeting SDK 35+ (this one targets
        // 36) - content draws behind the status bar unless padded for it explicitly, which on some
        // devices hides the top row of controls under the title bar. Add the real inset on top of
        // the fixed padding above instead of guessing a fixed value.
        layout.SetOnApplyWindowInsetsListener(new InsetsPaddingListener());

        _status = new TextView(this);
        layout.AddView(_status);

        _startButton = new Button(this) { Text = "Start" };
        _startButton.Click += (_, _) => SendServiceAction(null);
        layout.AddView(_startButton);

        _stopButton = new Button(this) { Text = "Stop" };
        _stopButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionStop);
        layout.AddView(_stopButton);

        _stylishStopButton = new Button(this) { Text = "Stylish Stop" };
        _stylishStopButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionStylishStop);
        layout.AddView(_stylishStopButton);

        var gravityButton = new Button(this) { Text = "Toggle Gravity" };
        gravityButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionToggleGravity);
        layout.AddView(gravityButton);

        var hotspotsButton = new Button(this) { Text = "Toggle Hotspots" };
        hotspotsButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionToggleHotspots);
        layout.AddView(hotspotsButton);

        var showHotspotsButton = new Button(this) { Text = "Show/Hide Hotspots" };
        showHotspotsButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionShowHotspots);
        layout.AddView(showHotspotsButton);

        var companionButton = new Button(this) { Text = "Toggle Companion" };
        companionButton.Click += (_, _) => SendServiceAction(PetOverlayService.ActionToggleCompanion);
        layout.AddView(companionButton);

        var sizeLabel = new TextView(this) { Text = "Size multiplier:" };
        layout.AddView(sizeLabel);

        var sizeRow = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        _sizeInput = new EditText(this)
        {
            Text = AndroidPrefs.GetDisplayScale(this).ToString(System.Globalization.CultureInfo.InvariantCulture),
            InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal,
        };
        sizeRow.AddView(_sizeInput);
        var applySizeButton = new Button(this) { Text = "Apply" };
        applySizeButton.Click += (_, _) => ApplySizeMultiplier();
        sizeRow.AddView(applySizeButton);
        layout.AddView(sizeRow);

        var characterLabel = new TextView(this) { Text = "Character:" };
        layout.AddView(characterLabel);

        var characters = LoadCharacterList();
        var spinner = new Spinner(this);
        var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, characters);
        spinner.Adapter = adapter;
        int startIndex = characters.IndexOf(Settings.StartingChar);
        if (startIndex >= 0) spinner.SetSelection(startIndex);
        bool spinnerReady = false;
        spinner.ItemSelected += (_, e) =>
        {
            // Spinner fires this once for the initial SetSelection above, with no user interaction
            // involved - ignore that first callback so opening the screen doesn't itself trigger a
            // character switch (and, before permission is granted, the error dialog below).
            if (!spinnerReady) { spinnerReady = true; return; }
            if (e.Position >= 0 && e.Position < characters.Count)
            {
                SendServiceAction(PetOverlayService.ActionSwitchCharacter, characters[e.Position]);
            }
        };
        layout.AddView(spinner);

        SetContentView(layout);
    }

    private bool _pollingStatus;

    protected override void OnResume()
    {
        base.OnResume();
        _pollingStatus = true;
        PollStatus();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _pollingStatus = false;
    }

    // Stop/Stylish Stop can flip back to Start on their own, asynchronously, once an outro
    // animation finishes playing out - not just in direct response to a button tap here. Poll
    // while the screen is actually visible instead of only refreshing right after our own actions.
    private void PollStatus()
    {
        RefreshStatus();
        if (_pollingStatus) _status.PostDelayed(PollStatus, 1000);
    }

    private void RefreshStatus()
    {
        _status.Text = $"Overlay permission granted: {AndroidSettings.CanDrawOverlays(this)}";

        bool running = PetOverlayService.IsRunning;
        _startButton.Visibility = running ? ViewStates.Gone : ViewStates.Visible;
        _stopButton.Visibility = running ? ViewStates.Visible : ViewStates.Gone;
        _stylishStopButton.Visibility = running ? ViewStates.Visible : ViewStates.Gone;
    }

    private void ApplySizeMultiplier()
    {
        if (!double.TryParse(_sizeInput.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value) || value <= 0)
        {
            Toast.MakeText(this, "Enter a positive number", ToastLength.Short)?.Show();
            return;
        }

        AndroidPrefs.SetDisplayScale(this, value);

        if (PetOverlayService.IsRunning)
        {
            // The window's size is only computed once, when the service first shows the pet - a
            // new value only takes effect on the next Start, so restart automatically to apply it
            // right away instead of leaving the user to notice and do it themselves.
            SendServiceAction(PetOverlayService.ActionStop);
            _status.PostDelayed(() => SendServiceAction(null), 500);
        }
    }

    private List<string> LoadCharacterList()
    {
        string dir = Path.Combine(AppPaths.BaseDirectory, "SpriteSheet", "Gremlins");
        if (!Directory.Exists(dir)) return new List<string>();
        return Directory.GetDirectories(dir)
            .Select(Path.GetFileName)
            .Where(name => name != null)
            .Select(name => name!)
            .OrderBy(name => name)
            .ToList();
    }

    private void RequestNotificationPermission()
    {
        // Required from Android 13+ (API 33) - declaring POST_NOTIFICATIONS in the manifest alone
        // is not enough, without this the foreground service's notification (and its quick
        // actions) is silently suppressed even though the service itself keeps running.
        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu) return;
        if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) == Permission.Granted) return;
        RequestPermissions(new[] { Android.Manifest.Permission.PostNotifications }, 1);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == 1 && grantResults.Length > 0 && grantResults[0] == Permission.Granted && AndroidSettings.CanDrawOverlays(this))
        {
            // Refresh so an already-running service's suppressed notification reappears. Only if
            // the overlay permission is also granted, otherwise there's no service running to
            // refresh and this would just pop the overlay-permission dialog as a side effect.
            SendServiceAction(null);
        }
    }

    // "Display over other apps" has no system permission dialog like POST_NOTIFICATIONS - the only
    // way to grant it is the per-app toggle in Settings, so this opens that screen directly instead.
    private void RequestOverlayPermission()
    {
        StartActivity(new Intent(AndroidSettings.ActionManageOverlayPermission, Android.Net.Uri.Parse("package:" + PackageName)));
    }

    private void ShowOverlayPermissionRequiredError()
    {
        new AlertDialog.Builder(this)
            .SetTitle("Permission required")
            .SetMessage("Desktop Gremlin needs the \"Display over other apps\" permission to show the character on screen. Grant it to continue.")
            .SetPositiveButton("Open settings", (_, _) => RequestOverlayPermission())
            .SetNegativeButton("Cancel", (System.EventHandler<DialogClickEventArgs>?)null)
            .Show();
    }

    private void SendServiceAction(string? action, string? character = null)
    {
        // Stopping (or a no-op refresh while already running) never needs the overlay permission -
        // only guard the actions that would try to show something on screen.
        if (action != PetOverlayService.ActionStop && action != PetOverlayService.ActionStylishStop && !AndroidSettings.CanDrawOverlays(this))
        {
            ShowOverlayPermissionRequiredError();
            return;
        }

        var intent = new Intent(this, typeof(PetOverlayService));
        if (action != null) intent.SetAction(action);
        if (character != null) intent.PutExtra(PetOverlayService.ExtraCharacter, character);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForegroundService(intent);
        }
        else
        {
            StartService(intent);
        }

        // IsRunning flips asynchronously as the service starts/stops - catch up shortly after
        // instead of only on the next OnResume (e.g. Stop wouldn't otherwise re-enable "Start"
        // without leaving and reopening the screen).
        _status.PostDelayed(RefreshStatus, 300);
    }

    private const int BasePadding = 40;

    private class InsetsPaddingListener : Java.Lang.Object, View.IOnApplyWindowInsetsListener
    {
        public WindowInsets OnApplyWindowInsets(View v, WindowInsets insets)
        {
            Android.Graphics.Insets bars = OperatingSystem.IsAndroidVersionAtLeast(30)
                ? insets.GetInsets(WindowInsets.Type.SystemBars())
                : Android.Graphics.Insets.Of(insets.SystemWindowInsetLeft, insets.SystemWindowInsetTop, insets.SystemWindowInsetRight, insets.SystemWindowInsetBottom);
            v.SetPadding(BasePadding + bars.Left, BasePadding + bars.Top, BasePadding + bars.Right, BasePadding + bars.Bottom);
            return insets;
        }
    }
}
