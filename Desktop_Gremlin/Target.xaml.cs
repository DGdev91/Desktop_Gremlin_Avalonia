using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace DesktopGremlin
{
    public partial class Target : System.Windows.Window
    {
        private DispatcherTimer _masterTimer;
        private DispatcherTimer _effectTimer;
        private CurrentFrames _currentFrames = new CurrentFrames(); 
        private FrameCounts _frameCounts = new FrameCounts();    
        public Target()
        {
            InitializeComponent();
            //InitializeAnimations();
            ImageInitialize();
            this.MouseLeftButtonDown += Target_MouseLeftButtonDown;
            this.Height = Settings.FrameHeight /4;
            this.Width = Settings.FrameWidth /4;
            SpriteFood.Width = Settings.FrameWidth /4;
            SpriteFood.Height = Settings.FrameHeight /4;
            //IntroEffect.Width = Settings.FrameWidth /4;
            //IntroEffect.Height = Settings.FrameHeight /4;   
            this.ShowInTaskbar = Settings.ShowTaskBar;

            if (Settings.FakeTransparent)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            }

        }
        private void ImageInitialize()
        {
            Random rng = new Random();
            string fileName = "food1.png";
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"SpriteSheet", "Misc", fileName);
            SpriteFood.Source = new BitmapImage(new Uri(path));
        }
        //private void InitializeAnimations()
        //{
        //    _effectTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
        //    _effectTimer.Tick += (s, e) =>
        //    {
        //        _currentFrames.Poof = OverlayEffect("poof", "Misc", _currentFrames.Poof,107, true);
        //    };
        //    _effectTimer.Start();
        //}
        //private int OverlayEffect(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        //{
        //    currentFrame = SpriteManager.PlayEffect(stateName, folder, currentFrame, frameCount, IntroEffect);
        //    if (resetOnEnd && currentFrame == 0)
        //    {
        //        IntroEffect.Source = null;
        //        _effectTimer.Stop();
        //    }
        //    return currentFrame;
        //}
        private void Target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); 
        }    
        public Point GetCenter()
        {
            return new Point(this.Left + this.Width / 2, this.Top + this.Height / 2);
        }

    }
}
