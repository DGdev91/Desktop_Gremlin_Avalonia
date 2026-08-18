using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Controls;
using DesktopGremlin;
using Java.Interop;
using System;
using AndroidSettings = Android.Provider.Settings;

namespace DesktopGremlin.Droid;

[Service(ForegroundServiceType = ForegroundService.TypeSpecialUse, Exported = false)]
public class PetOverlayService : Service, IPetSpawner
{
    private const string ChannelId = "pet_overlay";
    private const int NotificationId = 1;

    // Notification quick actions and MainActivity's settings screen both just re-enter this
    // service with one of these actions - same mechanism, same handling, one place to look.
    public const string ActionToggleGravity = "com.desktopgremlin.app.TOGGLE_GRAVITY";
    public const string ActionToggleHotspots = "com.desktopgremlin.app.TOGGLE_HOTSPOTS";
    public const string ActionShowHotspots = "com.desktopgremlin.app.SHOW_HOTSPOTS";
    public const string ActionToggleCompanion = "com.desktopgremlin.app.TOGGLE_COMPANION";
    public const string ActionSwitchCharacter = "com.desktopgremlin.app.SWITCH_CHARACTER";
    public const string ActionStop = "com.desktopgremlin.app.STOP";
    public const string ActionStylishStop = "com.desktopgremlin.app.STYLISH_STOP";
    public const string ExtraCharacter = "character";

    // Hotspot rectangles in the fixed 300x300 sprite canvas, matching MainWindow.axaml exactly.
    private static readonly RectF LeftHotspot = new(50, 25, 90, 165);
    private static readonly RectF LeftDownHotspot = new(49, 165, 90, 295);
    private static readonly RectF RightHotspot = new(225, 25, 264, 170);
    private static readonly RectF RightDownHotspot = new(225, 170, 264, 295);
    private static readonly RectF TopHotspot = new(115, 30, 205, 80);

    private static bool _avaloniaInitialized;

    // Lets MainActivity show real state instead of always displaying both Start and Stop as if
    // the pet were already showing - true only once the overlay window has actually been added.
    public static bool IsRunning { get; private set; }

    private IWindowManager? _windowManager;
    private SpriteOverlayView? _view;
    private PetFrameHolder? _frameHolder;
    private AndroidPetWindow? _petWindow;
    private WindowManagerLayoutParams? _layoutParams;
    private double _density;

    private CompanionInstance? _companion;
    private SummonInstance? _summon;
    private bool _hotspotsEnabled = true;
    private bool _hotspotsVisible;

    private float _touchStartRawX, _touchStartRawY;
    private float _touchDownLocalX, _touchDownLocalY;
    private int _touchStartX, _touchStartY;
    private bool _dragging;
    private const float DragThresholdPx = 12f;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        StartAsForeground();

        // WindowManager.AddView(..., TYPE_APPLICATION_OVERLAY) throws BadTokenException if this
        // isn't granted - it's a special permission that resets on reinstall/signature change, so
        // it can't be assumed just because the manifest declares it.
        if (!AndroidSettings.CanDrawOverlays(this))
        {
            Android.Util.Log.Warn("DesktopGremlin", "Overlay permission not granted, stopping service.");
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        ShowPet(); // idempotent - only actually creates the pet the first time

        switch (intent?.Action)
        {
            case ActionToggleGravity:
                _frameHolder?.ToggleGravity();
                _companion?.ToggleGravity();
                break;
            case ActionToggleHotspots:
                _hotspotsEnabled = !_hotspotsEnabled;
                break;
            case ActionShowHotspots:
                _hotspotsVisible = !_hotspotsVisible;
                if (_view != null) _view.ShowHotspots = _hotspotsVisible;
                break;
            case ActionToggleCompanion:
                ToggleCompanion();
                break;
            case ActionSwitchCharacter:
                var character = intent.GetStringExtra(ExtraCharacter);
                if (!string.IsNullOrEmpty(character)) _frameHolder?.SwitchCharacter(character);
                break;
            case ActionStop:
                StopSelf();
                return StartCommandResult.NotSticky;
            case ActionStylishStop:
                _frameHolder?.TriggerStylishStop();
                break;
        }

        UpdateNotification();
        return StartCommandResult.Sticky;
    }

    private void StartAsForeground()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var manager = (NotificationManager)GetSystemService(NotificationService)!;
            manager.CreateNotificationChannel(new NotificationChannel(ChannelId, "Desktop Gremlin", NotificationImportance.Low));
        }

        StartForeground(NotificationId, BuildNotification());
    }

    private void UpdateNotification()
    {
        var manager = (NotificationManager)GetSystemService(NotificationService)!;
        manager.Notify(NotificationId, BuildNotification());
    }

    private Notification BuildNotification()
    {
        var contentIntent = PendingIntent.GetActivity(
            this, 0, new Intent(this, typeof(MainActivity)),
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        string gravityLabel = "Gravity: " + (Settings.EnableGravity ? "ON" : "OFF");
        string hotspotsLabel = "Hotspots: " + (_hotspotsEnabled ? "ON" : "OFF");

        return new Notification.Builder(this, ChannelId)
            .SetContentTitle("Desktop Gremlin")
            .SetContentText("Running - tap to open settings")
            .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon)
            .SetOngoing(true)
            .SetContentIntent(contentIntent)
            .AddAction(BuildNotificationAction(gravityLabel, ActionToggleGravity, 1))
            .AddAction(BuildNotificationAction(hotspotsLabel, ActionToggleHotspots, 2))
            .AddAction(BuildNotificationAction("Stop", ActionStop, 3))
            .Build();
    }

    private Notification.Action BuildNotificationAction(string label, string action, int requestCode)
    {
        var intent = new Intent(this, typeof(PetOverlayService)).SetAction(action);
        var pendingIntent = PendingIntent.GetForegroundService(
            this, requestCode, intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        return new Notification.Action.Builder(Android.Resource.Drawable.SymDefAppIcon, label, pendingIntent).Build();
    }

    private void EnsureAvaloniaInitialized()
    {
        if (_avaloniaInitialized) return;

        // Only used headlessly here (to decode/crop sprite bitmaps via Core's SpriteManager) -
        // the actual on-screen rendering is native (SpriteOverlayView), since Avalonia's own
        // Android rendering surface can't do real per-pixel transparency outside an Activity.
        AppBuilder.Configure<App>()
            .UseAndroid()
            .SetupWithoutStarting();

        _avaloniaInitialized = true;
    }

    private void ShowPet()
    {
        if (_view != null) return;

        string baseDir = AndroidAssetExtractor.EnsureExtracted(this);
        AppPaths.BaseDirectory = baseDir + "/";
        AppErrors.Reporter = (message, title, close) => Android.Util.Log.Error("DesktopGremlin", $"{title}: {message}");

        ConfigManager.LoadMasterConfig();
        new FrameCounts().LoadConfigChar(Settings.StartingChar); // populates Settings.FrameWidth/Height/SpriteColumn used elsewhere
        _hotspotsEnabled = !Settings.DisableHotspots;

        EnsureAvaloniaInitialized();

        _windowManager = GetSystemService(WindowService)!.JavaCast<IWindowManager>();

        // Desktop's 300 DIP canvas assumes a desktop-density monitor; at real phone density that's
        // most of the screen. Scale down the effective density instead of the canvas size, so every
        // DIP-space calculation that already depends on _density (window size, Position, hotspot
        // hit-testing) shrinks together and stays internally consistent. User-configurable from
        // MainActivity, persisted so it survives service restarts.
        _density = Resources!.DisplayMetrics!.Density * AndroidPrefs.GetDisplayScale(this);

        _view = new SpriteOverlayView(this);
        _view.Touch += View_Touch;

        // Fixed 300x300 canvas, same as MainWindow.axaml's SpriteImage - not the character's own
        // sprite-sheet frame size, which is only used for cropping. Hotspot rects assume this.
        int sizePx = (int)(300 * Settings.SpriteSize * _density);

        _layoutParams = new WindowManagerLayoutParams(
            sizePx,
            sizePx,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotTouchModal | WindowManagerFlags.LayoutNoLimits,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Left,
            X = 100,
            Y = 300,
        };

        _petWindow = new AndroidPetWindow(_windowManager, _layoutParams, _view, _density);
        _petWindow.Show();

        _frameHolder = new PetFrameHolder();
        _frameHolder.SpriteImage.PropertyChanged += (s, e) =>
        {
            if (e.Property == Image.SourceProperty)
            {
                _view.SetFrame(_frameHolder.SpriteImage.Source as global::Avalonia.Media.Imaging.CroppedBitmap);
            }
        };
        _frameHolder.Attach(_petWindow, this, onOutroComplete: StopSelf);

        IsRunning = true;
    }

    private void View_Touch(object? sender, View.TouchEventArgs e)
    {
        if (_petWindow == null || e.Event == null) return;

        switch (e.Event.Action)
        {
            case MotionEventActions.Down:
                _touchStartRawX = e.Event.RawX;
                _touchStartRawY = e.Event.RawY;
                _touchDownLocalX = e.Event.GetX();
                _touchDownLocalY = e.Event.GetY();
                var pos = _petWindow.Position;
                _touchStartX = pos.X;
                _touchStartY = pos.Y;
                _dragging = false;
                e.Handled = true;
                break;

            case MotionEventActions.Move:
                float dx = e.Event.RawX - _touchStartRawX;
                float dy = e.Event.RawY - _touchStartRawY;
                if (!_dragging && (Math.Abs(dx) > DragThresholdPx || Math.Abs(dy) > DragThresholdPx))
                {
                    _dragging = true;
                }
                if (_dragging)
                {
                    // _touchStartX/Y are DIP (Position's unit); dx/dy are raw screen-pixel deltas.
                    _petWindow.Position = new PixelPoint(_touchStartX + (int)(dx / _density), _touchStartY + (int)(dy / _density));
                }
                e.Handled = true;
                break;

            case MotionEventActions.Up:
                if (!_dragging)
                {
                    HandleTap();
                }
                e.Handled = true;
                break;
        }
    }

    private void HandleTap()
    {
        if (_frameHolder == null) return;

        if (_hotspotsEnabled)
        {
            double scale = Settings.SpriteSize * _density;
            float x = _touchDownLocalX / (float)scale;
            float y = _touchDownLocalY / (float)scale;

            if (LeftHotspot.Contains(x, y)) { _frameHolder.TriggerLeftEmote(); return; }
            if (LeftDownHotspot.Contains(x, y)) { _frameHolder.TriggerLeftDownEmote(); return; }
            if (RightHotspot.Contains(x, y)) { _frameHolder.TriggerRightEmote(); return; }
            if (RightDownHotspot.Contains(x, y)) { _frameHolder.TriggerRightDownEmote(); return; }
            if (TopHotspot.Contains(x, y)) { _frameHolder.TopHotspotTap(); return; }
        }

        _frameHolder.TriggerTap();
    }

    // --- IPetSpawner ---

    public void ToggleCompanion()
    {
        if (_windowManager == null || _petWindow == null) return;

        if (_companion != null)
        {
            _companion.Close();
            _companion = null;
            return;
        }

        _companion = new CompanionInstance(_windowManager, this, _density);
        _companion.Window.Position = _petWindow.Position;
        _companion.Show(_petWindow);
    }

    public void SpawnSummon(double direction, double offsetX)
    {
        if (_windowManager == null || _petWindow == null) return;

        _summon?.Close();
        var summon = new SummonInstance(_windowManager, this, _density, direction);
        var pos = _petWindow.Position;
        summon.Window.Position = new PixelPoint(pos.X + (int)Math.Round(offsetX), pos.Y);
        summon.OnClosed = () => _summon = null;
        _summon = summon;
        summon.Show();
    }

    public IPetWindow SpawnFood()
    {
        var rng = new Random();
        double density = _density;
        int widthPx = (int)(Settings.FrameWidth / 4.0 * density);
        int heightPx = (int)(Settings.FrameHeight / 4.0 * density);

        var layoutParams = new WindowManagerLayoutParams(
            widthPx,
            heightPx,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotTouchModal | WindowManagerFlags.LayoutNoLimits,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Left,
        };

        var view = new SpriteOverlayView(this);
        string fileName = Settings.FoodMode.ToUpperInvariant() switch
        {
            "OGURI" => $"food{rng.Next(1, 3)}.png",
            "CALSTONE" => "cat.png",
            "SPEAKI" => "speaki.png",
            _ => "coffee1.png",
        };
        string path = System.IO.Path.Combine(AppPaths.BaseDirectory, "SpriteSheet", "Misc", fileName);
        view.SetStaticBitmap(BitmapFactory.DecodeFile(path));

        var window = new AndroidOverlayWindow(_windowManager!, layoutParams, view, density, () => Settings.StartingChar);

        var workingArea = window.GetCombinedWorkingArea(); // DIP, like window.Width/Height below
        double randomLeft = rng.NextDouble() * (workingArea.Width - window.Width) + workingArea.X;
        double randomTop = rng.NextDouble() * (workingArea.Height - window.Height) + workingArea.Y;
        window.Position = new PixelPoint((int)Math.Round(randomLeft), (int)Math.Round(randomTop));
        window.Show();
        return window;
    }

    public override void OnDestroy()
    {
        _companion?.Close();
        _summon?.Close();
        _petWindow?.Close();
        IsRunning = false;
        base.OnDestroy();
    }
}
