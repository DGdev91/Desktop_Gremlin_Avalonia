using Android.Views;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace DesktopGremlin.Droid;

/// <summary>
/// Shared IPetWindow plumbing for every native overlay window (main pet, companion, summon,
/// target): WindowManager add/remove/position, native ScaleTransform mirroring, screen bounds.
/// </summary>
public abstract class AndroidPetWindowBase : IPetWindow
{
    protected readonly IWindowManager WindowManager;
    protected readonly WindowManagerLayoutParams LayoutParams;
    protected readonly SpriteOverlayView View;
    protected readonly double Density;
    private bool _isVisible;
    private ITransform? _renderTransform;

    protected AndroidPetWindowBase(IWindowManager windowManager, WindowManagerLayoutParams layoutParams, SpriteOverlayView view, double density)
    {
        WindowManager = windowManager;
        LayoutParams = layoutParams;
        View = view;
        Density = density;
    }

    public SpriteOverlayView SpriteView => View;

    // Position is exposed in the same DIP space as Width/Height (Core's math, inherited from
    // desktop, adds/subtracts them together - e.g. Position.X + Width/2). LayoutParams.X/Y stay
    // in raw screen pixels internally, since that's what WindowManager needs.
    public PixelPoint Position
    {
        get => new PixelPoint((int)(LayoutParams.X / Density), (int)(LayoutParams.Y / Density));
        set
        {
            LayoutParams.X = (int)(value.X * Density);
            LayoutParams.Y = (int)(value.Y * Density);
            if (_isVisible)
            {
                WindowManager.UpdateViewLayout(View, LayoutParams);
            }
        }
    }

    public double Width => LayoutParams.Width / Density;
    public double Height => LayoutParams.Height / Density;

    public ITransform? RenderTransform
    {
        get => _renderTransform;
        set
        {
            _renderTransform = value;
            if (value is ScaleTransform scale)
            {
                View.ScaleX = (float)scale.ScaleX;
                View.ScaleY = (float)scale.ScaleY;
            }
        }
    }

    public bool IsVisible => _isVisible;

    public virtual void Show()
    {
        if (_isVisible) return;
        WindowManager.AddView(View, LayoutParams);
        _isVisible = true;
    }

    public virtual void Close()
    {
        if (!_isVisible) return;
        WindowManager.RemoveView(View);
        _isVisible = false;
    }

    public void BeginDrag(PointerPressedEventArgs e)
    {
        // Dragging is driven by native Android touch events on the hosting view (see
        // PetOverlayService), not Avalonia's BeginMoveDrag, which needs a desktop window backend.
    }

    // Also converted to DIP space, to match Position/Width/Height above.
    public PixelRect GetCombinedWorkingArea() => ScaleToDip(AndroidScreen.GetWorkingArea(View.Context!));
    public PixelRect? GetCurrentScreenWorkingArea() => ScaleToDip(AndroidScreen.GetWorkingArea(View.Context!));

    private PixelRect ScaleToDip(PixelRect pixels) => new(
        (int)(pixels.X / Density), (int)(pixels.Y / Density),
        (int)(pixels.Width / Density), (int)(pixels.Height / Density));

    public abstract string GetSelectedCharacter();
}
