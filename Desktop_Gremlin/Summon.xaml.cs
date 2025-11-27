using Mambo;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Doto
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

        public Summon(int whichSide)
        {
            InitializeComponent();
            SpriteFlipTransform.ScaleX = whichSide;
            SpriteImage.Source = new CroppedBitmap();
            FrameCounts = ConfigManager.LoadConfigChar("Lemon");
            GremlinState.LockState();   
            ConfigManager.ApplyXamlSettings(this);  
            SecondMediaManager.PlaySound("intro.wav", "Lemon");
            SpriteFlipTransform.CenterX = (Settings.FrameWidth * Settings.SpriteSize) / 2;
            SpriteFlipTransform.CenterY = (Settings.FrameHeight * Settings.SpriteSize) / 2;
            if (Settings.EnableGravity)
            {
                _gravityTimer = new DispatcherTimer();
                _gravityTimer.Interval = TimeSpan.FromMilliseconds(30);
                _gravityTimer.Tick += Gravity_Tick;
                _gravityTimer.Start();
            }
            InitializeAnimations();
        }
        private void Gravity_Tick(object sender, EventArgs e)
        {
            double bottomLimit = SystemParameters.WorkArea.Bottom - SpriteImage.ActualHeight;
            if (IsDragging)
            {
                return;
            }
            if (this.Top < bottomLimit)
            {
                this.Top += Settings.SvGravity;
            }
        }
        private int PlayAnimationIfActive(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {

            currentFrame = SpriteManagerComp.PlayAnimation(stateName, folder, currentFrame, frameCount, SpriteImage);

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
            currentFrame = SpriteManagerComp.PlayEffect(stateName, folder, currentFrame, frameCount, IntroEffect);
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
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsDragging = true;
            DragMove();
            IsDragging = false;
        }
    }
}
