using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Desktop_Gremlin
{
    public partial class FullScreen : Window
    {
        private DispatcherTimer _jumpTimer;
        public FullScreen()
        {
            InitializeComponent();
            InitializeAnimation();  

        }
        private int PlayAnimation(string sheetName,string action, int currentFrame, int frameCount, Image targetImage, bool PlayOnce = false)
        {
            Bitmap sheet = SpriteManager.Get(sheetName,action);

            if (sheet == null)
            {
                return currentFrame;
            }
            int x = (currentFrame % Settings.SpriteColumn) * Settings.FrameWidthJs;
            int y = (currentFrame / Settings.SpriteColumn) * Settings.FrameHeightJs;

            if (x + Settings.FrameWidthJs > sheet.PixelSize.Width || y + Settings.FrameHeightJs > sheet.PixelSize.Height)
            {
                return currentFrame;
            }

            targetImage.Source = new CroppedBitmap(sheet, new PixelRect(x, y, Settings.FrameWidthJs, Settings.FrameHeightJs));

            return (currentFrame + 1) % frameCount;
        }
        public void InitializeAnimation()
        {
            _jumpTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _jumpTimer.Tick += (s, e) =>
            {
               CurrentFrames.JumpScare = PlayAnimation("jumpscare","Action", CurrentFrames.JumpScare, 
                   FrameCounts.JumpScare, 
                   SpriteImage);
                if (CurrentFrames.JumpScare <= 0)
                {
                    _jumpTimer.Stop();
                    this.Close();
                }
            };  
            _jumpTimer.Start();
        }





    }
}
