using Android.Views;
using Avalonia;
using DesktopGremlin;

namespace DesktopGremlin.Droid;

/// <summary>
/// IMainPetWindow backed by a native Android overlay window. Core's controllers
/// (AnimationController, MovementController, TimerController, ...) don't know the difference
/// between this and a desktop Avalonia.Controls.Window.
/// </summary>
public class AndroidPetWindow : AndroidPetWindowBase, IMainPetWindow
{
    public AndroidPetWindow(IWindowManager windowManager, WindowManagerLayoutParams layoutParams, SpriteOverlayView view, double density)
        : base(windowManager, layoutParams, view, density)
    {
    }

    /// <summary>Mirrors MainWindow's _config._selectedCharacter - mutable so combat mode can swap it.</summary>
    public string SelectedCharacter { get; set; } = Settings.StartingChar;
    public override string GetSelectedCharacter() => SelectedCharacter;

    public bool IsCombat { get; set; }
    public Size? FollowCursor_oldWindowSize => null;
    public void FollowCursor_EnlargeMainWindow() { }
    public void FollowCursor_RestoreMainWindow() { }
    public PixelPoint? GetCursorScreen() => null;

    private MovementController? _movementController;
    public void AttachMovementController(MovementController movementController) => _movementController = movementController;
    public void TriggerRandomMove() => _movementController?.RandomMove();
}
