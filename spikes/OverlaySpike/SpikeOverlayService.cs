using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Interop;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AndroidApp = Android.App.Application;

namespace OverlaySpike;

[Service(ForegroundServiceType = ForegroundService.TypeSpecialUse, Exported = false)]
public class SpikeOverlayService : Service
{
    private const string ChannelId = "overlay_spike";
    private const int NotificationId = 1;

    private static bool _avaloniaInitialized;

    private IWindowManager? _windowManager;
    private AvaloniaView? _avaloniaView;
    private WindowManagerLayoutParams? _layoutParams;
    private Border? _box;
    private DispatcherTimer? _blinkTimer;

    private float _touchStartRawX, _touchStartRawY;
    private int _touchStartX, _touchStartY;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        StartAsForeground();
        ShowOverlay();
        return StartCommandResult.Sticky;
    }

    private void StartAsForeground()
    {
        var manager = (NotificationManager)GetSystemService(NotificationService)!;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, "Overlay spike", NotificationImportance.Low);
            manager.CreateNotificationChannel(channel);
        }

        var notification = new Notification.Builder(this, ChannelId)
            .SetContentTitle("Overlay spike running")
            .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon)
            .SetOngoing(true)
            .Build();

        StartForeground(NotificationId, notification);
    }

    private void EnsureAvaloniaInitialized()
    {
        if (_avaloniaInitialized)
        {
            return;
        }

        AppBuilder.Configure<App>()
            .UseAndroid()
            .SetupWithoutStarting();

        _avaloniaInitialized = true;
    }

    private void ShowOverlay()
    {
        if (_avaloniaView != null)
        {
            return;
        }

        EnsureAvaloniaInitialized();

        _windowManager = GetSystemService(WindowService)!.JavaCast<IWindowManager>();

        _box = new Border
        {
            Width = 160,
            Height = 160,
            Background = new SolidColorBrush(Colors.MediumPurple),
            CornerRadius = new CornerRadius(24),
            Child = new TextBlock
            {
                Text = "drag me",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };

        _avaloniaView = new AvaloniaView(this)
        {
            Content = _box,
        };
        _avaloniaView.Touch += AvaloniaView_Touch;

        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        bool toggle = false;
        _blinkTimer.Tick += (_, _) =>
        {
            toggle = !toggle;
            _box.Background = new SolidColorBrush(toggle ? Colors.MediumPurple : Colors.Orange);
        };
        _blinkTimer.Start();

        var density = Resources!.DisplayMetrics!.Density;
        int sizePx = (int)(160 * density);

        _layoutParams = new WindowManagerLayoutParams(
            sizePx,
            sizePx,
            Build.VERSION.SdkInt >= BuildVersionCodes.O
                ? WindowManagerTypes.ApplicationOverlay
                : WindowManagerTypes.Phone,
            WindowManagerFlags.NotTouchModal | WindowManagerFlags.LayoutNoLimits,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Left,
            X = 100,
            Y = 300,
        };

        _windowManager.AddView(_avaloniaView, _layoutParams);
    }

    private void AvaloniaView_Touch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if (_layoutParams == null || _windowManager == null || e.Event == null)
        {
            return;
        }

        switch (e.Event.Action)
        {
            case MotionEventActions.Down:
                _touchStartRawX = e.Event.RawX;
                _touchStartRawY = e.Event.RawY;
                _touchStartX = _layoutParams.X;
                _touchStartY = _layoutParams.Y;
                e.Handled = true;
                break;

            case MotionEventActions.Move:
                _layoutParams.X = _touchStartX + (int)(e.Event.RawX - _touchStartRawX);
                _layoutParams.Y = _touchStartY + (int)(e.Event.RawY - _touchStartRawY);
                _windowManager.UpdateViewLayout(_avaloniaView, _layoutParams);
                e.Handled = true;
                break;
        }
    }

    public override void OnDestroy()
    {
        _blinkTimer?.Stop();
        if (_avaloniaView != null && _windowManager != null)
        {
            _windowManager.RemoveView(_avaloniaView);
        }
        _avaloniaView = null;
        base.OnDestroy();
    }
}
