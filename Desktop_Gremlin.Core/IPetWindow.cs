#nullable enable
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace DesktopGremlin
{
    /// <summary>
    /// Geometry/lifecycle surface every pet-hosting window (MainWindow, Companion, Summon, Target)
    /// exposes to shared movement/follow logic. Lets that logic run unchanged against an Android
    /// overlay-window implementation, which has no Avalonia.Controls.Window to work with.
    /// </summary>
    public interface IPetWindow
    {
        PixelPoint Position { get; set; }
        double Width { get; }
        double Height { get; }
        ITransform? RenderTransform { get; set; }
        bool IsVisible { get; }

        void Show();
        void Close();
        void BeginDrag(PointerPressedEventArgs e);

        PixelRect GetCombinedWorkingArea();
        PixelRect? GetCurrentScreenWorkingArea();

        string GetSelectedCharacter();
    }
}
