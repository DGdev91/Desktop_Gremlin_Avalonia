using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Koyuki;
using System;
using static ConfigManager;
namespace Desktop_Gremlin
{
    public partial class Gremlin : Window
    {
        //To those reading this, I'm sorry for this messy code, or not//
        //In the future I'm planning to seperate major code snippets into diffrent class files//
        //Instead of barfing evrything in 1 file//
        //Thanks and have a Mamboful day//
        private Point _pointerPosition;
        private DateTime _nextRandomActionTime = DateTime.Now.AddSeconds(20);

        private Random _rng = new Random();

        //private MediaPlayer _walkLoopPlayer;

        private bool _wasIdleLastFrame = false;

        private DispatcherTimer _followTimer;
        private Target _currentFood;
        private AppConfig _config;
        private DispatcherTimer _masterTimer;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _activeRandomMoveTimer;
        public struct POINT
        {
            public int X;
            public int Y;
        }
        public Gremlin()
        {
            InitializeComponent();
            SpriteImage.Source = new CroppedBitmap();
            ConfigManager.LoadMasterConfig();
            ConfigManager.LoadConfigChar();
            ConfigManager.ApplyXamlSettings(this);
            InitializeAnimations();
            InitializeTimers();
            AnimationStates.LockState();
            MediaManager.PlaySound("intro.wav");
            _config = new AppConfig(this);
        }
        public void InitializeTimers()
        {
            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(Settings.SleepTime);
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();
            //_walkLoopPlayer = new MediaPlayer();
            //_walkLoopPlayer.Open(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", Settings.StartingChar, "steps.wav")));
            //_walkLoopPlayer.Volume = 1; 
            //_walkLoopPlayer.MediaEnded += (s, e) =>
            //{
            //    _walkLoopPlayer.Position = TimeSpan.Zero;
            //    _walkLoopPlayer.Play(); 
            //};
        }
        
        public static void ErrorClose(string errorMessage, string errorTitle, bool close)
        {
            if (!Settings.AllowErrorMessages)
            {
                var msg = new Window
                {
                    Title = errorTitle,
                    Content = new TextBlock { Text = errorMessage },
                    Width = 400,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Colors.White)
                };
                var stackPanel = new StackPanel
                {
                    Spacing = 10,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                };

                var textBlock = new TextBlock
                {
                    Text = errorMessage,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.Black),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var button = new Button
                {
                    Content = "OK",
                    Width = 80,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                button.Click += (s, e) =>
                {
                    if (close)
                    {
                        Environment.Exit(1);
                    }
                    else
                    {
                        msg.Close();
                    }
                };
                
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(button);
                msg.Content = stackPanel;

                msg.Show();
                msg.Focus();
            }
            else if (close)
            {
                Environment.Exit(1);
            }
        }
        private void InitializeAnimations()
        {
            _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _masterTimer.Tick += (s, e) =>
            {
                if (AnimationStates.GetState("Intro"))
                {
                    CurrentFrames.Intro = SpriteManager.PlayAnimation("intro","Actions",CurrentFrames.Intro,FrameCounts.Intro,SpriteImage);

                    if (CurrentFrames.Intro == 0)
                    {
                        AnimationStates.UnlockState();
                        AnimationStates.ResetAllExceptIdle();   
                    }
                }
                if (AnimationStates.GetState("Dragging"))
                {
                    CurrentFrames.Grab = SpriteManager.PlayAnimation("grab", "Actions", CurrentFrames.Grab,
                        FrameCounts.Grab,SpriteImage);
                }
                if (AnimationStates.GetState("Emote1"))
                {
                    CurrentFrames.Emote1 = SpriteManager.PlayAnimation("emote1","Emotes", CurrentFrames.Emote1,
                        FrameCounts.Emote1, SpriteImage);
                    
                }
                if (AnimationStates.GetState("Emote2"))
                {
                    CurrentFrames.Emote2 = SpriteManager.PlayAnimation("emote2","Emotes", CurrentFrames.Emote2,
                        FrameCounts.Emote2, SpriteImage);
                    if (CurrentFrames.Emote2 == 0)
                    {
                        AnimationStates.UnlockState();
                        AnimationStates.ResetAllExceptIdle();
                    }
                }
                if (AnimationStates.GetState("Emote3"))
                {
                    CurrentFrames.Emote3 = SpriteManager.PlayAnimation("emote3","Emotes", CurrentFrames.Emote3,
                     FrameCounts.Emote3, SpriteImage);
                }
                if (AnimationStates.GetState("Emote4"))
                {
                   CurrentFrames.Emote4 = SpriteManager.PlayAnimation("emote4","Emotes", CurrentFrames.Emote4,
                        FrameCounts.Emote4, SpriteImage);
                    if (CurrentFrames.Emote4 == 0)
                    {
                        AnimationStates.UnlockState();
                        AnimationStates.ResetAllExceptIdle();
                    }
                }
                if (AnimationStates.GetState("Idle"))
                {
                    CurrentFrames.Idle = SpriteManager.PlayAnimation("idle","Actions", CurrentFrames.Idle,
                        FrameCounts.Idle, SpriteImage);
                }
                if (AnimationStates.GetState("Outro"))
                { 
                    CurrentFrames.Outro = SpriteManager.PlayAnimation("outro","Actions", CurrentFrames.Outro,
                        FrameCounts.Outro, SpriteImage);
                    if (CurrentFrames.Outro == 0)
                    {
                        Close();                   
                    }
                }

                if (AnimationStates.GetState("Click"))
                {
                    CurrentFrames.Click = SpriteManager.PlayAnimation("click","Actions",
                    CurrentFrames.Click,
                    FrameCounts.Click,SpriteImage);

                    if (CurrentFrames.Click == 0)
                    {
                        AnimationStates.UnlockState();
                        AnimationStates.ResetAllExceptIdle();
                    }
                }
                if (AnimationStates.GetState("Hover"))
                {
                    CurrentFrames.Hover = SpriteManager.PlayAnimation(
                        "hover",
                        "Actions",
                        CurrentFrames.Hover,
                        FrameCounts.Hover,
                        SpriteImage);

                }
                if (AnimationStates.GetState("Sleeping"))
                {
                    CurrentFrames.Sleep = SpriteManager.PlayAnimation(
                       "sleep",
                       "Actions",   
                       CurrentFrames.Sleep,
                       FrameCounts.Sleep,
                       SpriteImage);
                }
                if (AnimationStates.GetState("Pat"))
                {
                    AnimationStates.LockState();
                    CurrentFrames.Pat = SpriteManager.PlayAnimation(
                      "pat",
                      "Actions",
                      CurrentFrames.Pat,
                      FrameCounts.Pat,
                      SpriteImage);
                    if (CurrentFrames.Pat == 0) {
                        AnimationStates.UnlockState();
                        AnimationStates.ResetAllExceptIdle();
                    }
                }
                if (MouseSettings.FollowCursor && AnimationStates.GetState("Walking"))
                {
                    var cursorScreen = new Point(_pointerPosition.X, _pointerPosition.Y);

                    double halfW = SpriteImage.Bounds.Width > 0 ? SpriteImage.Bounds.Width / 2.0 : Settings.FrameWidth / 2.0;
                    double halfH = SpriteImage.Bounds.Height > 0 ? SpriteImage.Bounds.Height / 2.0 : Settings.FrameHeight / 2.0;
                    //var spriteCenterScreen = SpriteImage.PointToScreen(new Point(halfW, halfH));
                    var spriteCenterScreen = new Point(this.Position.X + halfW, this.Position.Y + halfH);

                    /*
                    var source = PresentationSource.FromVisual(this);
                    System.Windows.Media.Matrix transformFromDevice = System.Windows.Media.Matrix.Identity;

                    if (source?.CompositionTarget != null)
                    {
                        transformFromDevice = source.CompositionTarget.TransformFromDevice;
                    }

                    var spriteCenterWpf = transformFromDevice.Transform(spriteCenterScreen);
                    var cursorWpf = transformFromDevice.Transform(cursorScreen);
                    */

                    double dx = cursorScreen.X - spriteCenterScreen.X;
                    double dy = cursorScreen.Y - spriteCenterScreen.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance > Settings.FollowRadius)
                    {
                        double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);
                        double nx = dx / distance;
                        double ny = dy / distance;
                        double moveX = nx * step;
                        double moveY = ny * step;

                        Position = new PixelPoint((int)(Position.X + Math.Round(moveX)), (int)(Position.Y + Math.Round(moveY)));
                        double angle = Math.Atan2(moveY, moveX) * (180.0 / Math.PI);

                        if (angle < 0) angle += 360;


                        if (angle >= 337.5 || angle < 22.5)
                        {
                            CurrentFrames.Right = SpriteManager.PlayAnimation(
                                "runRight",
                                "Run",
                                CurrentFrames.Right,
                                FrameCounts.Right,
                                SpriteImage);
                        }
                        else if (angle >= 22.5 && angle < 67.5)
                        {
                            CurrentFrames.DownRight = SpriteManager.PlayAnimation(
                                "downRight",
                                "Run",
                                CurrentFrames.DownRight,
                                FrameCounts.DownRight,
                                SpriteImage);
                        }
                        else if (angle >= 67.5 && angle < 112.5)
                        {
                            CurrentFrames.Down = SpriteManager.PlayAnimation(
                                "runDown",
                                "Run",
                                CurrentFrames.Down,
                                FrameCounts.Down,
                                SpriteImage);
                        }
                        else if (angle >= 112.5 && angle < 157.5)
                        {
                            CurrentFrames.DownLeft = SpriteManager.PlayAnimation(
                                "downLeft",
                                "Run",
                                CurrentFrames.DownLeft,
                                FrameCounts.DownLeft,
                                SpriteImage);
                        }
                        else if (angle >= 157.5 && angle < 202.5)
                        {
                            CurrentFrames.Left = SpriteManager.PlayAnimation(
                                "runLeft",
                                "Run",
                                CurrentFrames.Left,
                                FrameCounts.Left,
                                SpriteImage);
                        }
                        else if (angle >= 202.5 && angle < 247.5)
                        {
                            CurrentFrames.UpLeft = SpriteManager.PlayAnimation(
                                "upLeft",
                                "Run",
                                CurrentFrames.UpLeft,
                                FrameCounts.UpLeft,
                                SpriteImage);
                        }
                        else if (angle >= 247.5 && angle < 292.5)
                        {
                            CurrentFrames.Up = SpriteManager.PlayAnimation(
                                "runUp",
                                "Run",
                                CurrentFrames.Up,
                                FrameCounts.Up,
                                SpriteImage);
                        }
                        else if (angle >= 292.5 && angle < 337.5)
                        {
                            CurrentFrames.UpRight = SpriteManager.PlayAnimation(
                                "upRight",
                                "Run",
                                CurrentFrames.UpRight,
                                FrameCounts.UpRight,
                                SpriteImage);
                        }
                    }
                    else
                    {
                        CurrentFrames.WalkIdle = SpriteManager.PlayAnimation(
                            "runIdle",
                            "Actions",
                            CurrentFrames.WalkIdle,
                            FrameCounts.RunIdle,
                            SpriteImage);
                    }

                }
                bool isIdleNow = AnimationStates.IsCompletelyIdle();
                if (Settings.AllowRandomness)
                {
                    if (isIdleNow && !_wasIdleLastFrame)
                    {
                        int interval = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                        _nextRandomActionTime = DateTime.Now.AddSeconds(interval);
                    }
                    if (isIdleNow && DateTime.Now >= _nextRandomActionTime)
                    {
                        AnimationStates.SetState("Random");

                        int action = _rng.Next(0, 5);
                        switch (action)
                        {
                            case 0:
                                CurrentFrames.Click = 0;
                                AnimationStates.UnlockState();
                                AnimationStates.SetState("Click");
                                MediaManager.PlaySound("mambo.wav");
                                AnimationStates.LockState();
                                break;
                            case 1:
                                RandomMove();
                                break;
                            case 2:
                                RandomMove();
                                break;
                            case 3:
                                RandomMove();
                                break;
                            case 4:
                                RandomMove();
                                break;                            
                        }

                        int intervalAfterAction = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                        _nextRandomActionTime = DateTime.Now.AddSeconds(intervalAfterAction);
                    }
                }
                _wasIdleLastFrame = isIdleNow;
            };      
            _masterTimer.Start();     
        }
        private void RandomMove()
        {
            _activeRandomMoveTimer?.Stop();
            AnimationStates.SetState("Random");

            double moveX = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;
            double moveY = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;

            double targetLeft = Math.Max(Screens.Primary.WorkingArea.X,Math.Min(Position.X + moveX, Screens.Primary.WorkingArea.Right - SpriteImage.Bounds.Width));
            double targetTop = Math.Max(Screens.Primary.WorkingArea.Y,Math.Min(Position.Y + moveY, Screens.Primary.WorkingArea.Bottom - SpriteImage.Bounds.Height));

            const double step = 120;
            double dx = (targetLeft - this.Position.X) / step;
            double dy = (targetTop - this.Position.Y) / step;

            DispatcherTimer moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _activeRandomMoveTimer = moveTimer;

            int moveCount = 0;

            moveTimer.Tick += (s, e) =>
            {
                Position = new PixelPoint((int)(Position.X + Math.Round(dx)), (int)(Position.Y + Math.Round(dy)));
                moveCount++;
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    if (dx > 0)
                    {
                        CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight","Walk", CurrentFrames.WalkRight,
                            FrameCounts.WalkR, SpriteImage);
                    }
                    else
                    {
                        CurrentFrames.WalkLeft = SpriteManager.PlayAnimation("walkLeft","Walk", CurrentFrames.WalkLeft,
                            FrameCounts.WalkL, SpriteImage);
                    }
                }
                else
                {
                    if (dy > 0)
                    {
                        CurrentFrames.WalkDown = SpriteManager.PlayAnimation("walkDown","Walk", CurrentFrames.WalkDown,
                            FrameCounts.WalkDown, SpriteImage);
                    }
                    else
                    {
                        CurrentFrames.WalkUp = SpriteManager.PlayAnimation("walkUp","Walk", CurrentFrames.WalkUp,
                            FrameCounts.WalkUp,SpriteImage);
                    }
                }

                if (moveCount >= step || !AnimationStates.GetState("Random") )
                {
                    moveTimer.Stop();
                    AnimationStates.SetState("Idle");
                    _activeRandomMoveTimer = null;
                }
            };
            moveTimer.Start();
        }
   
        private void SpriteImage_RightClick(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                ResetIdleTimer();
                CurrentFrames.Click = 0;
                AnimationStates.UnlockState();
                AnimationStates.SetState("Click");
                MediaManager.PlaySound("mambo.wav");
                AnimationStates.LockState();   
            }
        }

        private void SpriteImage_MouseEnter(object sender, PointerEventArgs e)
        {
            AnimationStates.SetState("Hover");
            if (AnimationStates.GetState("Hover"))
            {
                MediaManager.PlaySound("hover.wav", 5);
            }
            
        }
        private void SpriteImage_MouseLeave(object sender, PointerEventArgs e)
        {
            AnimationStates.SetState("Idle");
            CurrentFrames.Hover = 0;            
        }
        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ResetIdleTimer();
                AnimationStates.UnlockState();
                AnimationStates.SetState("Dragging");
                MediaManager.PlaySound("grab.wav");
                BeginMoveDrag(e);
                AnimationStates.SetState("Idle");
                MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
                if (MouseSettings.FollowCursor)
                {
                    AnimationStates.SetState("Walking");
                    AnimationStates.LockState();
                }
                CurrentFrames.Grab = 0;
                if (MouseSettings.FollowCursor)
                {
                    MediaManager.PlaySound("run.wav");
                }
            }
        }
        private void TopHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CurrentFrames.Click = 0;    
                AnimationStates.UnlockState();
                AnimationStates.SetState("Click");
                MediaManager.PlaySound("mambo.wav");
            }
        }
      
        private void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }
        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (AnimationStates.GetState("Sleeping"))
            {
                return;
            }
            else
            {
                AnimationStates.UnlockState();
                MediaManager.PlaySound("sleep.wav");
                AnimationStates.SetState("Sleeping");
                AnimationStates.LockState();
            }

        }
        public void EmoteHelper(string emote, string mp3)
        {
            ResetIdleTimer();
            AnimationStates.UnlockState();
            AnimationStates.SetState(emote);
            MediaManager.PlaySound(mp3);
            AnimationStates.LockState();
        }
        private void LeftHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CurrentFrames.Emote1 = 0;
                EmoteHelper("Emote1", "emote1.wav");
            }
        }
        private void LeftDownHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CurrentFrames.Emote2 = 0;
                EmoteHelper("Emote2", "emote2.wav");   
            }
        }
        private void RightHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CurrentFrames.Emote3 = 0;
                EmoteHelper("Emote3", "emote3.wav");
            }
        }
        private void RightDownHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CurrentFrames.Emote4 = 0;
                EmoteHelper("Emote4", "emote4.wav");   
            }
        }

        private void Grid_PointerMoved(object sender, PointerEventArgs e)
        {
            _pointerPosition = e.GetPosition(this);
        }     
    }
}

