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
        public Target()
        {
            InitializeComponent();
            ImageInitialize();
            this.MouseLeftButtonDown += Target_MouseLeftButtonDown;
            this.Height = Settings.FrameHeight / 4;
            this.Width = Settings.FrameWidth / 4;
            SpriteFood.Width = Settings.FrameWidth / 4;
            SpriteFood.Height = Settings.FrameHeight / 4;  
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
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet", "Misc", fileName);
            SpriteFood.Source = new BitmapImage(new Uri(path));
        }
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