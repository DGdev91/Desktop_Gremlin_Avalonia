using Android.Content;
using Android.Graphics;
using Android.Views;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DesktopGremlin;
using System;

namespace DesktopGremlin.Droid;

/// <summary>Android analogue of Summon.axaml.cs: plays its intro animation once, then closes itself.</summary>
public class SummonInstance
{
    private readonly CurrentFrames _currentFrames = new();
    private readonly FrameCounts _frameCounts = new();
    private readonly Avalonia.Controls.Image _spriteImage = new();
    private readonly DispatcherTimer _masterTimer;

    private readonly AndroidOverlayWindow _window;
    private bool _closed;

    public Action? OnClosed { get; set; }

    public SummonInstance(IWindowManager windowManager, Context context, double density, double whichSide)
    {
        int sizePx = (int)(300 * Settings.SpriteSize * density);
        var layoutParams = new WindowManagerLayoutParams(
            sizePx,
            sizePx,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotTouchModal | WindowManagerFlags.LayoutNoLimits,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Left,
        };

        var view = new SpriteOverlayView(context);
        _window = new AndroidOverlayWindow(windowManager, layoutParams, view, density, () => Settings.SummonChar)
        {
            RenderTransform = new ScaleTransform(whichSide, 1.0),
        };

        _spriteImage.Width = 300 * Settings.SpriteSize;
        _spriteImage.Height = 300 * Settings.SpriteSize;
        _spriteImage.Source = new CroppedBitmap();
        _spriteImage.PropertyChanged += (s, e) =>
        {
            if (e.Property == Avalonia.Controls.Image.SourceProperty)
            {
                view.SetFrame(_spriteImage.Source as CroppedBitmap);
            }
        };

        _frameCounts.LoadConfigChar(Settings.SummonChar, SpriteManager.CharacterType.Summon);

        _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
        _masterTimer.Tick += (s, e) => Tick();
    }

    public IPetWindow Window => _window;

    public void Show()
    {
        _window.Show();
        Quirks.MediaManager.PlaySound("intro.wav", Settings.SummonChar);
        _masterTimer.Start();
    }

    private void Tick()
    {
        _currentFrames.Intro = SpriteManager.PlayAnimation("Intro", "Actions", _currentFrames.Intro, _frameCounts.Intro, _spriteImage, Settings.SummonChar, false, SpriteManager.CharacterType.Summon);
        if (_currentFrames.Intro == 0)
        {
            Close();
        }
    }

    public void Close()
    {
        if (_closed) return;
        _closed = true;
        _masterTimer.Stop();
        _window.Close();
        OnClosed?.Invoke();
    }
}
