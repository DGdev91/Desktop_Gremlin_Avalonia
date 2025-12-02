using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;

namespace Koyuki
{
    public partial class Target : Window
    {
        public Target()
        {
            InitializeComponent();
            ImageInitialize();
            this.PointerPressed += Target_MouseLeftButtonDown;
            this.Height = Settings.ItemHeight;
            this.Width = Settings.ItemWidth;
            this.ShowInTaskbar = Settings.ShowTaskBar;

            if (Settings.FakeTransparent)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            }

        }
        private void ImageInitialize()
        {
            Random rng = new Random();
            int index = rng.Next(1, 3);
            string fileName = $"food{index}.png";
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "SpriteSheet", "Misc", fileName);

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
