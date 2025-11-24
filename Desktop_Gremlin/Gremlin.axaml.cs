using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Koyuki;
using Mambo;
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
        private DateTime _nextRandomActionTime = DateTime.Now.AddSeconds(1);

        private Random _rng = new Random();
        private bool _wasIdleLastFrame = false;
        private DispatcherTimer _followTimer;
        private Target _currentFood;
        private AppConfig _config;
        //private MediaPlayer _walkLoopPlayer;
        private DispatcherTimer _masterTimer;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _activeRandomMoveTimer;

        private Companion _companionInstance;
        private AnimationStates GremlinState = new AnimationStates();
        private FrameCounts FrameCounts = new FrameCounts();
        private CurrentFrames CurrentFrames = new CurrentFrames();
        public struct POINT
        {
            public int X;
            public int Y;
        }
        public Gremlin()
        {
            InitializeComponent();
            ConfigManager.LoadMasterConfig();
            ConfigManager.ApplyXamlSettings(this);
            InitializeAnimations();
            InitializeTimers();
            SpriteImage.Source = new CroppedBitmap();
            FrameCounts = ConfigManager.LoadConfigChar(Settings.StartingChar);
            GremlinState.LockState();
            _config = new AppConfig(this, GremlinState);

            MediaManager.PlaySound("intro.wav",Settings.StartingChar);
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

        private int PlayAnimationIfActive(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {
            if(!GremlinState.GetState(stateName))
            {
                return currentFrame;
            };
            currentFrame = SpriteManager.PlayAnimation(stateName, folder, currentFrame, frameCount, SpriteImage);


            if (resetOnEnd && currentFrame == 0 && stateName == "Outro")
            {
                Environment.Exit(1);
            }

            if (resetOnEnd && currentFrame == 0)
            {
                GremlinState.UnlockState();
                GremlinState.ResetAllExceptIdle();
            }
            return currentFrame;
        }

        private void InitializeAnimations()
        {
            _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _masterTimer.Tick += (s, e) =>
            {
                //Repeatable Animations = false at the end//    
                CurrentFrames.Grab = PlayAnimationIfActive("Grab", "Actions", CurrentFrames.Grab, FrameCounts.Grab, false);
                CurrentFrames.Emote1 = PlayAnimationIfActive("Emote1", "Emotes", CurrentFrames.Emote1, FrameCounts.Emote1, false);
                CurrentFrames.Emote3 = PlayAnimationIfActive("Emote3", "Emotes", CurrentFrames.Emote3, FrameCounts.Emote3, false);
                CurrentFrames.Idle = PlayAnimationIfActive("Idle", "Actions", CurrentFrames.Idle, FrameCounts.Idle, false);
                CurrentFrames.Hover = PlayAnimationIfActive("Hover", "Actions", CurrentFrames.Hover, FrameCounts.Hover, false);
                CurrentFrames.Sleep = PlayAnimationIfActive("Sleeping", "Actions", CurrentFrames.Sleep, FrameCounts.Sleep, false);
                CurrentFrames.Pat = PlayAnimationIfActive("Pat", "Actions", CurrentFrames.Pat, FrameCounts.Pat, false);

                //Single Repeat Animations = true at the end//    
                CurrentFrames.Emote4 = PlayAnimationIfActive("Emote4", "Emotes", CurrentFrames.Emote4, FrameCounts.Emote4, true);
                CurrentFrames.Emote2 = PlayAnimationIfActive("Emote2", "Emotes", CurrentFrames.Emote2, FrameCounts.Emote2, true);
                CurrentFrames.Intro = PlayAnimationIfActive("Intro", "Actions", CurrentFrames.Intro, FrameCounts.Intro,true);
                CurrentFrames.Outro = PlayAnimationIfActive("Outro", "Actions", CurrentFrames.Outro, FrameCounts.Outro, true);
                CurrentFrames.Click = PlayAnimationIfActive("Click", "Actions", CurrentFrames.Click, FrameCounts.Click, true);  
                if (MouseSettings.FollowCursor && GremlinState.GetState("Walking"))
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
                            CurrentFrames.Right = SpriteManager.PlayAnimation("runRight","Run",CurrentFrames.Right,FrameCounts.Right,SpriteImage);
                        }
                        else if (angle >= 22.5 && angle < 67.5)
                        {
                            CurrentFrames.DownRight = SpriteManager.PlayAnimation("downRight","Run",CurrentFrames.DownRight,FrameCounts.DownRight,SpriteImage);
                        }
                        else if (angle >= 67.5 && angle < 112.5)
                        {
                            CurrentFrames.Down = SpriteManager.PlayAnimation("runDown","Run",CurrentFrames.Down,FrameCounts.Down,SpriteImage);
                        }
                        else if (angle >= 112.5 && angle < 157.5)
                        {
                            CurrentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft","Run",CurrentFrames.DownLeft,FrameCounts.DownLeft,SpriteImage);
                        }
                        else if (angle >= 157.5 && angle < 202.5)
                        {
                            CurrentFrames.Left = SpriteManager.PlayAnimation("runLeft","Run",CurrentFrames.Left,FrameCounts.Left,SpriteImage);
                        }
                        else if (angle >= 202.5 && angle < 247.5)
                        {
                            CurrentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft","Run",CurrentFrames.UpLeft,FrameCounts.UpLeft,SpriteImage);
                        }
                        else if (angle >= 247.5 && angle < 292.5)
                        {
                            CurrentFrames.Up = SpriteManager.PlayAnimation("runUp","Run",CurrentFrames.Up,FrameCounts.Up,SpriteImage);
                        }
                        else if (angle >= 292.5 && angle < 337.5)
                        {
                            CurrentFrames.UpRight = SpriteManager.PlayAnimation("upRight","Run",CurrentFrames.UpRight,FrameCounts.UpRight,SpriteImage);
                        }
                    }
                    else
                    {
                        CurrentFrames.WalkIdle = SpriteManager.PlayAnimation("runIdle","Actions",CurrentFrames.WalkIdle,FrameCounts.RunIdle,SpriteImage);
                    }

                }
                bool isIdleNow = GremlinState.IsCompletelyIdle();
                if (Settings.AllowRandomness)
                {
                    if (isIdleNow && !_wasIdleLastFrame)
                    {
                        int interval = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                        _nextRandomActionTime = DateTime.Now.AddSeconds(interval);
                    }
                    if (isIdleNow && DateTime.Now >= _nextRandomActionTime)
                    {
                        GremlinState.SetState("Random");

                        int action = _rng.Next(0, 5);
                        switch (action)
                        {
                            case 0:
                                CurrentFrames.Click = 0;
                                GremlinState.UnlockState();
                                GremlinState.SetState("Click");
                                MediaManager.PlaySound("mambo.wav",Settings.StartingChar);
                                GremlinState.LockState();
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
            GremlinState.SetState("Random");

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
                        CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight","Walk", CurrentFrames.WalkRight,FrameCounts.WalkR, SpriteImage);
                    }
                    else
                    {
                        CurrentFrames.WalkLeft = SpriteManager.PlayAnimation("walkLeft","Walk", CurrentFrames.WalkLeft,FrameCounts.WalkL, SpriteImage);
                    }
                }
                else
                {
                    if (dy > 0)
                    {
                        CurrentFrames.WalkDown = SpriteManager.PlayAnimation("walkDown","Walk", CurrentFrames.WalkDown,FrameCounts.WalkDown, SpriteImage);
                    }
                    else
                    {
                        CurrentFrames.WalkUp = SpriteManager.PlayAnimation("walkUp","Walk", CurrentFrames.WalkUp,FrameCounts.WalkUp,SpriteImage);
                    }
                }

                if (moveCount >= step || !GremlinState.GetState("Random") )
                {
                    moveTimer.Stop();
                    GremlinState.SetState("Idle");
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
                GremlinState.UnlockState();
                GremlinState.SetState("Click");
                MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
                GremlinState.LockState();   
            }
        }

        private void SpriteImage_MouseEnter(object sender, PointerEventArgs e)
        {
            GremlinState.SetState("Hover");
            if (GremlinState.GetState("Hover"))
            {
                MediaManager.PlaySound("hover.wav",Settings.StartingChar, 5);
            }
            
        }
        private void SpriteImage_MouseLeave(object sender, PointerEventArgs e)
        {
            GremlinState.SetState("Idle");
            CurrentFrames.Hover = 0;            
        }
        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ResetIdleTimer();
                GremlinState.UnlockState();
                GremlinState.SetState("Grab");
                MediaManager.PlaySound("grab.wav", Settings.StartingChar);
                BeginMoveDrag(e);
                GremlinState.SetState("Idle");
                MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
                if (MouseSettings.FollowCursor)
                {
                    GremlinState.SetState("Walking");
                    GremlinState.LockState();
                }
                CurrentFrames.Grab = 0;
                if (MouseSettings.FollowCursor)
                {
                    MediaManager.PlaySound("run.wav", Settings.StartingChar);
                }
            }
        }
        private void TopHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (_companionInstance != null && _companionInstance.IsVisible)
                {
                    _companionInstance.Close();
                    return;
                }
                _companionInstance = new Companion();
                _companionInstance.MainGremlin = this;
                _companionInstance.Closed += (s, args) => _companionInstance = null;
                _companionInstance.Show();
            }
        }
      
        private void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }
        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (GremlinState.GetState("Sleeping"))
            {
                return;
            }
            else
            {
                GremlinState.UnlockState();
                MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                GremlinState.SetState("Sleeping");
                GremlinState.LockState();
            }

        }
        public void EmoteHelper(string emote, string mp3)
        {
            ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState(emote);
            MediaManager.PlaySound(mp3,Settings.StartingChar);
            GremlinState.LockState();
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

