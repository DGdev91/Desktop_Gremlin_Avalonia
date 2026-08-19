using Android.Content;
using Android.Graphics;
using Android.Views;
using Avalonia.Media.Imaging;
using DesktopGremlin;
using DesktopGremlin.Quirks;

namespace DesktopGremlin.Droid;

/// <summary>Android analogue of Quirks/Companion/Companion.axaml.cs.</summary>
public class CompanionInstance
{
    private readonly AnimationStates _gremlinState = new();
    private readonly CurrentFrames _currentFrames = new();
    private readonly FrameCounts _frameCounts = new();
    private readonly Avalonia.Controls.Image _spriteImage = new();

    private readonly AndroidOverlayWindow _window;
    private readonly CompanionAnimationController _animationController;
    private readonly CompanionFollowController _followController;

    public CompanionInstance(IWindowManager windowManager, Context context, double density)
    {
        int sizePx = (int)(300 * QuirkSettings.CompanionScale * density);
        var layoutParams = new WindowManagerLayoutParams(
            sizePx,
            sizePx,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotTouchModal | WindowManagerFlags.LayoutNoLimits | WindowManagerFlags.NotFocusable,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Left,
        };

        var view = new SpriteOverlayView(context);
        _window = new AndroidOverlayWindow(windowManager, layoutParams, view, density, () => QuirkSettings.CompanionChar);

        _spriteImage.Width = 300 * QuirkSettings.CompanionScale;
        _spriteImage.Height = 300 * QuirkSettings.CompanionScale;
        _spriteImage.Source = new CroppedBitmap();
        _spriteImage.PropertyChanged += (s, e) =>
        {
            if (e.Property == Avalonia.Controls.Image.SourceProperty)
            {
                view.SetFrame(_spriteImage.Source as CroppedBitmap);
            }
        };

        _frameCounts.LoadConfigChar(QuirkSettings.CompanionChar, SpriteManager.CharacterType.Companion);
        _gremlinState.LockState();

        _animationController = new CompanionAnimationController(_window, _gremlinState, _currentFrames, _frameCounts, _spriteImage);
        _followController = new CompanionFollowController(_window, _gremlinState, _currentFrames, _frameCounts, _spriteImage);
        _animationController.SetFollowController(_followController);
    }

    public IPetWindow Window => _window;

    public void ToggleGravity() => _followController.ToggleGravity();

    public void Show(IPetWindow mainGremlin)
    {
        _followController.SetMainGremlin(mainGremlin);
        _window.Show();
        _animationController.Start();
        MediaManager.PlaySound("intro.wav", QuirkSettings.CompanionChar);
    }

    public void Close()
    {
        _animationController.Stop();
        _window.Close();
    }
}
