using DesktopGremlin;

namespace DesktopGremlin.Droid;

/// <summary>
/// The one piece of MainWindow.axaml.cs's behavior that can't be shared Core code: creating
/// another overlay window needs a WindowManager, which only PetOverlayService has. PetFrameHolder
/// (the Android analogue of MainWindow's animation/behavior logic) calls into this instead of
/// `new Companion()`/`new Summon()`/`new Target()` directly.
/// </summary>
public interface IPetSpawner
{
    void ToggleCompanion();
    void SpawnSummon(double direction, double offsetX);
    IPetWindow SpawnFood();
}
