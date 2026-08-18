#nullable enable
using Avalonia;

namespace DesktopGremlin
{
    /// <summary>
    /// Extra surface only the main gremlin window needs: combat mode, random idle movement, and
    /// pointer-follow. Kept separate from IPetWindow so Companion/Summon/Target don't have to carry
    /// members that make no sense for them.
    /// </summary>
    public interface IMainPetWindow : IPetWindow
    {
        bool IsCombat { get; }
        void TriggerRandomMove();

        Size? FollowCursor_oldWindowSize { get; }
        void FollowCursor_EnlargeMainWindow();
        void FollowCursor_RestoreMainWindow();
        PixelPoint? GetCursorScreen();
    }
}
