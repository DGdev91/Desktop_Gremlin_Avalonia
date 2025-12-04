using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Desktop_Gremlin
{

    public partial class Summon : Window
    {

        public Window MainGremlin { get; set; }
        public struct POINT
        {
            public int X;
            public int Y;
        }
        private DispatcherTimer _masterTimer;
        private DispatcherTimer _effectTimer;
        private DispatcherTimer _gravityTimer;
        private AnimationStates GremlinState = new AnimationStates();
        private CurrentFrames CurrentFrames = new CurrentFrames();
        private FrameCounts FrameCounts = new FrameCounts();
        private bool IsDragging = false;    
        public int WhichSide = -1;

        public Summon(double whichSide)
        {
            InitializeComponent();
            this.RenderTransform = new ScaleTransform(whichSide, 1.0);
            SpriteImage.Source = new CroppedBitmap();
            FrameCounts = ConfigManager.LoadConfigChar(Settings.SummonChar);
            GremlinState.LockState();   
            ConfigManager.ApplyXamlSettings(this);  
            MediaManager.PlaySound("intro.wav", Settings.SummonChar);
            if (Settings.EnableGravity)
            {
                _gravityTimer = new DispatcherTimer();
                _gravityTimer.Interval = TimeSpan.FromMilliseconds(30);
                _gravityTimer.Tick += Gravity_Tick;
                _gravityTimer.Start();
            }
            InitializeAnimations();
        }
        public new void Close()
        {
            _masterTimer.Stop();
            _gravityTimer.Stop();
            base.Close();
        }
        private void Gravity_Tick(object sender, EventArgs e)
        {
            Screen screen = TopLevel.GetTopLevel(this)?.Screens?.ScreenFromVisual(this);
            if (screen != null)
            {
                double bottomLimit;
                bottomLimit = screen?.WorkingArea.Bottom ?? 0;
                bottomLimit -= SpriteImage.Bounds.Height;
                if (IsDragging)
                {
                    return;
                }
                if (this.Position.Y < bottomLimit && this.Position.Y > 0)
                {
                    this.Position = new PixelPoint(
                        this.Position.X,
                        (int)(this.Position.Y + Math.Round(Settings.SvGravity))
                    );
                }
            }
        }
        private int PlayAnimationIfActive(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {

            currentFrame = SpriteManager.PlayAnimation(stateName, folder, currentFrame, frameCount, SpriteImage, Settings.SummonChar);

            if (resetOnEnd && currentFrame == 0)
            {
                this.Close();
                GremlinState.UnlockState();
                GremlinState.ResetAllExceptIdle();
            }
            return currentFrame;

        }
        private int OverlayEffect(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {
            currentFrame = SpriteManager.PlayEffect(stateName, folder, currentFrame, frameCount, IntroEffect, Settings.SummonChar);
            if (resetOnEnd && currentFrame == 0)
            {
                IntroEffect.Source = null;
                _effectTimer.Stop();
            }
            return currentFrame;
        }
        private void InitializeAnimations()
        {
            _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _masterTimer.Tick += (s, e) =>
            {
                CurrentFrames.Intro = PlayAnimationIfActive("Intro","Actions" , CurrentFrames.Intro, FrameCounts.Intro, true);
            };
            _masterTimer.Start();

            //_effectTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            //_effectTimer.Tick += (s, e) =>
            //{
            //    CurrentFrames.Poof = OverlayEffect("Poof", "Effects", CurrentFrames.Poof, FrameCounts.Poof, true);
            //};
            //_effectTimer.Start();
        }
        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                IsDragging = true;
                BeginMoveDrag(e);
                IsDragging = false;   
            }
        }
    }
}
