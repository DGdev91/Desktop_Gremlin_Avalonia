using Android.Views;
using System;

namespace DesktopGremlin.Droid;

/// <summary>
/// IPetWindow for the non-main overlay windows (Companion, Summon, Target) - same geometry
/// plumbing as AndroidPetWindow, just parametrized by which character/asset it represents
/// instead of carrying the main-pet-only combat/random-move/cursor-follow members.
/// </summary>
public class AndroidOverlayWindow : AndroidPetWindowBase
{
    private readonly Func<string> _characterGetter;

    public AndroidOverlayWindow(IWindowManager windowManager, WindowManagerLayoutParams layoutParams, SpriteOverlayView view, double density, Func<string> characterGetter)
        : base(windowManager, layoutParams, view, density)
    {
        _characterGetter = characterGetter;
    }

    public override string GetSelectedCharacter() => _characterGetter();
}
