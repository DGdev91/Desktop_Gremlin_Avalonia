using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using System;

namespace DesktopGremlin.Quirks.Companion
{
    public partial class Companion : Window
    {
        private static extern bool GetCursorPos(out POINT lpPoint);
        public static string characterChoice = "";
        public Window MainGremlin { get; set; }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        private AnimationStates GremlinState = new AnimationStates();
        private CurrentFrames CurrentFrames = new CurrentFrames();
        private FrameCounts FrameCounts = new FrameCounts();

        private CompanionAnimationController _animationController;
        private CompanionFollowController _followController;
        public Companion()
        {
            InitializeComponent();
            InitializeCompanion();
        }

        private void InitializeCompanion()
        {
            SpriteImage.Source = new CroppedBitmap();
            FrameCounts.LoadConfigChar(QuirkSettings.CompanionChar, SpriteManager.CharacterType.Companion);
            GremlinState.LockState();

            ApplyScaling();
            ApplyWindowSettings();

            _animationController = new CompanionAnimationController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage);
            _followController = new CompanionFollowController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage);
           

            _animationController.SetFollowController(_followController);
            _animationController.Start();
            MediaManager.PlaySound("intro.wav", QuirkSettings.CompanionChar);
        }

        private void ApplyScaling()
        {
            this.Width = this.Width * QuirkSettings.CompanionScale;
            this.Height = this.Height * QuirkSettings.CompanionScale;
            SpriteImage.Width = SpriteImage.Width * QuirkSettings.CompanionScale;
            SpriteImage.Height = SpriteImage.Height * QuirkSettings.CompanionScale;
        }

        private void ApplyWindowSettings()
        {
            if (Settings.FakeTransparent)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            }
            if (Settings.ManualReize)
            {
                this.SizeToContent = SizeToContent.Manual;
            }
            if (Settings.ForceCenter)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            if (Settings.EnableMinSize)
            {
                this.MinWidth = this.Width;
                this.MinHeight = this.Height;
            }
            if (Settings.ShowTaskBar)
            {
                this.ShowInTaskbar = true;
            }
        }

        public void SetMainGremlin(Window mainGremlin)
        {
            MainGremlin = mainGremlin;
            if (_followController != null)
            {
                _followController.SetMainGremlin(mainGremlin);
            }
        }

        public void ToggleGravity()
        {
            _followController.ToggleGravity();
        }

        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            GremlinState.UnlockState();
            GremlinState.SetState("Grab");
            MediaManager.PlaySound("grab.wav",QuirkSettings.CompanionChar);
            BeginMoveDrag(e);
            GremlinState.SetState("Idle");
        }
    }
}
