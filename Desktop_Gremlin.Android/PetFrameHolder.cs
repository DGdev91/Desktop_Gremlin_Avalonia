using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DesktopGremlin;
using System;

namespace DesktopGremlin.Droid;

/// <summary>
/// Android analogue of MainWindow.axaml.cs: owns the main gremlin's animation state + the
/// hotspot/emote/combat/sleep behavior that reacts to it. Owns an off-screen Avalonia Image that
/// Core's AnimationController/MovementController/TimerController write frames into exactly as
/// they do for the desktop Window content - never attached to any visual tree or window, Avalonia
/// doesn't need that to decode/crop bitmaps. SpriteOverlayView reads SpriteImage.Source each time
/// it changes and draws it natively, since Avalonia's own Android rendering can't do transparent
/// content outside an Activity. Spawning other windows (Companion/Summon/Target) is delegated to
/// IPetSpawner, since that needs a WindowManager which only PetOverlayService has.
/// </summary>
public class PetFrameHolder
{
    private readonly Random _rng = new();
    private readonly AnimationStates _gremlinState = new();
    private readonly FrameCounts _frameCounts = new();
    private readonly CurrentFrames _currentFrames = new();

    public Image SpriteImage { get; } = new Image();

    private AnimationController? _animationController;
    private MovementController? _movementController;
    private TimerController? _timerController;
    private FoodFollower? _foodFollower;

    private AndroidPetWindow? _host;
    private IPetSpawner? _spawner;
    private string _combatModeOriginalCharacter = string.Empty;

    public void Attach(AndroidPetWindow host, IPetSpawner spawner, Action? onOutroComplete = null)
    {
        _host = host;
        _spawner = spawner;

        SpriteImage.Width = Settings.FrameWidth;
        SpriteImage.Height = Settings.FrameHeight;
        SpriteImage.Source = new CroppedBitmap();

        _timerController = new TimerController(host, _gremlinState, SpriteImage);
        _animationController = new AnimationController(host, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng, onOutroComplete);
        _movementController = new MovementController(host, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng);
        _foodFollower = new FoodFollower(host, _gremlinState, _currentFrames, _frameCounts, SpriteImage);
        host.AttachMovementController(_movementController);

        _gremlinState.LockState();
        _animationController.Start();
        _timerController.Start();

        PlayIntro();
    }

    private void PlayIntro()
    {
        if (_host == null) return;
        Quirks.MediaManager.PlaySound("intro.wav", _host.GetSelectedCharacter());
        _frameCounts.LoadConfigChar(_host.GetSelectedCharacter());
        _gremlinState.UnlockState();
        _gremlinState.SetState("Intro");
        _currentFrames.Idle = 0;
        _currentFrames.Intro = 0;
    }

    public void ToggleGravity() => _timerController?.ToggleGravity();

    /// <summary>Mirrors desktop's tray "Stylish Close": plays the Outro animation, which calls
    /// back (see Attach's onOutroComplete) once it finishes instead of desktop's Environment.Exit.</summary>
    public void TriggerStylishStop()
    {
        if (_host == null) return;
        _gremlinState.PlayOutro();
        Quirks.MediaManager.PlaySound("outro.wav", _host.GetSelectedCharacter());
    }

    /// <summary>Mirrors MainWindow's tray "Select Character" item: live character swap, no combat-mode bookkeeping.</summary>
    public void SwitchCharacter(string character)
    {
        if (_host == null || string.IsNullOrEmpty(character)) return;
        _host.SelectedCharacter = character;
        PlayIntro();
    }

    public void TriggerTap()
    {
        if (_timerController == null || _host == null) return;

        _timerController.ResetIdleTimer();
        _currentFrames.Click = 0;
        _currentFrames.Idle = 0;
        _gremlinState.UnlockState();
        _gremlinState.SetState("Click");
        Quirks.MediaManager.PlaySound("mambo.wav", _host.GetSelectedCharacter());
        _gremlinState.LockState();
    }

    private void EmoteHelper(string emote, string mp3)
    {
        if (_timerController == null || _host == null) return;
        _timerController.ResetIdleTimer();
        _gremlinState.UnlockState();
        _gremlinState.SetState(emote);
        Quirks.MediaManager.PlaySound(mp3, _host.GetSelectedCharacter());
        _gremlinState.LockState();
    }

    public void TriggerLeftEmote()
    {
        if (_host == null || _spawner == null) return;
        if (!string.IsNullOrEmpty(Settings.CombatModeChar) && !_host.IsCombat) return;

        if (_host.RenderTransform is ScaleTransform current && current.ScaleX < 0)
        {
            // the buttons are mirrored in this case, so the left button is actually the right emote
            TriggerRightEmote();
            return;
        }

        if (Settings.MirrorXSprite)
        {
            _host.RenderTransform = new ScaleTransform(-1.0, 1.0);
            EmoteHelper("Emote3", "emote1.wav");
            _currentFrames.Emote3 = 0;
            _currentFrames.Idle = 0;
        }
        else
        {
            EmoteHelper("Emote1", "emote1.wav");
            _currentFrames.Emote1 = 0;
            _currentFrames.Idle = 0;
        }

        if (!string.IsNullOrEmpty(Settings.SummonChar))
        {
            double offsetX = _host.Width * -0.7;
            _spawner.SpawnSummon(-1.0, offsetX);
        }
    }

    public void TriggerRightEmote()
    {
        if (_host == null || _spawner == null) return;
        if (!string.IsNullOrEmpty(Settings.CombatModeChar) && !_host.IsCombat) return;

        if (Settings.MirrorXSprite)
        {
            _host.RenderTransform = new ScaleTransform(1.0, 1.0);
        }
        EmoteHelper("Emote3", "emote3.wav");
        _currentFrames.Emote3 = 0;
        _currentFrames.Idle = 0;

        if (!string.IsNullOrEmpty(Settings.SummonChar))
        {
            double offsetX = _host.Width * 0.7;
            _spawner.SpawnSummon(1.0, offsetX);
        }
    }

    public void TriggerLeftDownEmote()
    {
        if (_host?.RenderTransform is ScaleTransform current && current.ScaleX < 0)
        {
            TriggerRightDownEmote();
            return;
        }
        _currentFrames.Emote2 = 0;
        _currentFrames.Idle = 0;
        EmoteHelper("Emote2", "emote2.wav");
    }

    public void TriggerRightDownEmote()
    {
        // No reciprocal ScaleX<0 check here (unlike TriggerLeftDownEmote) - same reason
        // TriggerRightEmote above doesn't redirect back to TriggerLeftEmote: with both sides
        // redirecting to each other on the same condition, and nothing ever changing that
        // condition, a mirrored sprite sends the two into infinite mutual recursion (stack
        // overflow) the moment either hotspot is tapped.
        _currentFrames.Emote4 = 0;
        _currentFrames.Idle = 0;
        EmoteHelper("Emote4", "emote4.wav");
    }

    public void ToggleSleep()
    {
        if (_host == null || _host.IsCombat) return;

        if (_gremlinState.GetState("Sleeping"))
        {
            _gremlinState.UnlockState();
            _gremlinState.SetState("Idle");
            _timerController?.ResetIdleTimer();
        }
        else
        {
            _gremlinState.UnlockState();
            Quirks.MediaManager.PlaySound("sleep.wav", _host.GetSelectedCharacter());
            _gremlinState.SetState("Sleeping");
            _gremlinState.LockState();
        }
    }

    public void ToggleCombatMode()
    {
        if (_host == null) return;

        if (!_host.IsCombat)
        {
            _movementController?.StopRandomMove();
        }
        _host.IsCombat = !_host.IsCombat;

        if (string.IsNullOrEmpty(_combatModeOriginalCharacter))
        {
            _combatModeOriginalCharacter = _host.GetSelectedCharacter();
            _host.SelectedCharacter = Settings.CombatModeChar;
        }
        else
        {
            _host.SelectedCharacter = _combatModeOriginalCharacter;
            _combatModeOriginalCharacter = string.Empty;
        }
        PlayIntro();
    }

    /// <summary>Mirrors MainWindow.axaml.cs's TopHotspot_Click: combat toggle, companion
    /// toggle, and food trigger all fire together, whichever are configured.</summary>
    public void TopHotspotTap()
    {
        if (_spawner == null) return;

        if (!string.IsNullOrEmpty(Settings.CombatModeChar)) ToggleCombatMode();
        if (!string.IsNullOrEmpty(QuirkSettings.CompanionChar)) _spawner.ToggleCompanion();
        if (!string.IsNullOrEmpty(Settings.FoodMode) && Settings.FoodMode != "None") TriggerFood();
    }

    public void TriggerFood()
    {
        if (_host == null || _spawner == null || _foodFollower == null) return;
        if (_gremlinState.GetState("FollowItem")) return;

        _gremlinState.UnlockState();
        _gremlinState.SetState("FollowItem");
        _gremlinState.LockState();

        var target = _spawner.SpawnFood();
        Quirks.MediaManager.PlaySound("food.wav", _host.GetSelectedCharacter());
        Quirks.MediaManager.PlaySound("foodSpawn.wav", _host.GetSelectedCharacter());
        _foodFollower.StartFollowing(target, QuirkSettings.CurrentItemAcceleration);
    }
}
