using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using System;

namespace DesktopGremlin
{
    public partial class Target : Window
    {
        private DispatcherTimer _masterTimer;
        private DispatcherTimer _effectTimer;
        private CurrentFrames _currentFrames = new CurrentFrames(); 
        private FrameCounts _frameCounts = new FrameCounts();
        public Target()
        {
            InitializeComponent();
            ImageInitialize();
            this.PointerPressed += Target_MouseLeftButtonDown;
            this.Height = Settings.FrameHeight /4;
            this.Width = Settings.FrameWidth /4;
            this.ShowInTaskbar = Settings.ShowTaskBar;

            if (Settings.FakeTransparent)
            {
                this.Background = (ImmutableSolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            }

        }
        private void ImageInitialize()
        {
            string fileName = "coffee1.png"; //defaults to Cafe
            if (!string.IsNullOrEmpty(Settings.FoodMode) && Settings.FoodMode.ToUpper().CompareTo("OGURI") == 0)
            {
                Random rng = new Random();
                int index = rng.Next(1, 3);
                fileName = $"food{index}.png";
            }
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet", "Misc", fileName);
            SpriteFood.Source = new Bitmap(path);
        }
        private void Target_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e); 
            }
        }    
        public Point GetCenter()
        {
            return new Point(this.Position.X + this.Width / 2, this.Position.Y + this.Height / 2);
        }

    }
}
