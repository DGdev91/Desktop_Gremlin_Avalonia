using Doto;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static ConfigManager;
namespace Desktop_Gremlin
{
    public partial class Gremlin : Window
    {
       
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        private DateTime _nextRandomActionTime = DateTime.Now.AddSeconds(1);
        private Random _rng = new Random();
        private bool _wasIdleLastFrame = false;

        private AppConfig _config;

        private DispatcherTimer _masterTimer;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _activeRandomMoveTimer;

        private bool _isCombat = false;
        private bool _hotspotVisible = false;
        private bool _hotspotDisable = false; 
        public bool IsCombat => _isCombat;

        private DispatcherTimer _gravityTimer;

        private Summon _SummonInstance;
        private AnimationStates GremlinState = new AnimationStates();
        private FrameCounts FrameCounts = new FrameCounts();
        private CurrentFrames CurrentFrames = new CurrentFrames();

        private KeyboardController _keyboardController;

        public struct POINT
        {
            public int X;
            public int Y;
        }
        //Do note this is a scuff and schizophrenic version that only applies to Exu/Exu2   
        //It contains some changes that i will hopefully push to the uma and ba versions
        //Don't Quote me on that
        public Gremlin()
        {
            InitializeComponent();
            InitializeConfig();
            MediaManager.PlaySound("intro.wav", Settings.StartingChar);
            GremlinState.LockState();
        }

        public void InitializeConfig()
        {
            ConfigManager.LoadMasterConfig();
            ConfigManager.ApplyXamlSettings(this);
            InitializeAnimations();
            InitializeTimers();

            SpriteImage.Source = new CroppedBitmap();
            FrameCounts = ConfigManager.LoadConfigChar(Settings.StartingChar);

            SpriteFlipTransform.CenterX = (Settings.FrameWidth * Settings.SpriteSize) / 2;
            SpriteFlipTransform.CenterY = (Settings.FrameHeight * Settings.SpriteSize) / 2;

            _config = new AppConfig(this, GremlinState);

            _keyboardController = new KeyboardController(this, GremlinState, CurrentFrames, FrameCounts, _rng);
        }

        public void InitializeTimers()
        {
            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(Settings.SleepTime);
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();

            if (Settings.EnableGravity)
            {
                _gravityTimer = new DispatcherTimer();
                _gravityTimer.Interval = TimeSpan.FromMilliseconds(30);
                _gravityTimer.Tick += Gravity_Tick;
                _gravityTimer.Start();
            }
        }

        private void Gravity_Tick(object sender, EventArgs e)
        {
            double bottomLimit = SystemParameters.WorkArea.Bottom - SpriteImage.ActualHeight;
            if (GremlinState.GetState("Grab"))
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
            if (!GremlinState.GetState(stateName))
            {
                return currentFrame;
            }
            currentFrame = SpriteManager.PlayAnimation(stateName, folder, currentFrame, frameCount, SpriteImage);

            if (resetOnEnd && currentFrame == 0 && stateName == "Outro")
            {
                System.Windows.Application.Current.Shutdown();
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
                CurrentFrames.Idle = PlayAnimationIfActive("Idle", "Actions", CurrentFrames.Idle, FrameCounts.Idle, false);
                CurrentFrames.Sleep = PlayAnimationIfActive("Sleeping", "Actions", CurrentFrames.Sleep, FrameCounts.Sleep, false);

                CurrentFrames.Intro = PlayAnimationIfActive("Intro", "Actions", CurrentFrames.Intro, FrameCounts.Intro, true);
                CurrentFrames.Click = PlayAnimationIfActive("Click", "Actions", CurrentFrames.Click, FrameCounts.Click, true);
                CurrentFrames.Outro = PlayAnimationIfActive("Outro", "Actions", CurrentFrames.Outro, FrameCounts.Outro, true);
                CurrentFrames.Emote3 = PlayAnimationIfActive("Emote3", "Emotes", CurrentFrames.Emote3, FrameCounts.Emote3, true);
                CurrentFrames.Emote1 = PlayAnimationIfActive("Emote1", "Emotes", CurrentFrames.Emote1, FrameCounts.Emote1, true);

                if (MouseSettings.FollowCursor && GremlinState.GetState("Walking") && _isCombat == false)
                {
                    POINT cursorPos;
                    GetCursorPos(out cursorPos);
                    var cursorScreen = new System.Windows.Point(cursorPos.X, cursorPos.Y);

                    double halfW = SpriteImage.ActualWidth > 0 ? SpriteImage.ActualWidth / 2.0 : Settings.FrameWidth / 2.0;
                    double halfH = SpriteImage.ActualHeight > 0 ? SpriteImage.ActualHeight / 2.0 : Settings.FrameHeight / 2.0;
                    var spriteCenterScreen = SpriteImage.PointToScreen(new System.Windows.Point(halfW, halfH));

                    var source = PresentationSource.FromVisual(this);
                    System.Windows.Media.Matrix transformFromDevice = System.Windows.Media.Matrix.Identity;

                    if (source?.CompositionTarget != null)
                    {
                        transformFromDevice = source.CompositionTarget.TransformFromDevice;
                    }

                    var spriteCenterWpf = transformFromDevice.Transform(spriteCenterScreen);
                    var cursorWpf = transformFromDevice.Transform(cursorScreen);

                    double dx = cursorWpf.X - spriteCenterWpf.X;
                    double dy = cursorWpf.Y - spriteCenterWpf.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (Settings.EnableGravity)
                    {
                        dy = 0;
                    }
                    if (distance > Settings.FollowRadius)
                    {
                        double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);
                        double nx = dx / distance;
                        double ny = dy / distance;
                        double moveX = nx * step;
                        double moveY = ny * step;

                        this.Left += moveX;
                        this.Top += moveY;
                        double angle = Math.Atan2(moveY, moveX) * (180.0 / Math.PI);

                        if (angle < 0) angle += 360;
                        if (angle >= 337.5 || angle < 22.5)
                        {
                            SpriteFlipTransform.ScaleX = 1;
                            CurrentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", CurrentFrames.Right, FrameCounts.Right, SpriteImage);
                        }
                        else if (angle >= 157.5 && angle < 202.5)
                        {
                            SpriteFlipTransform.ScaleX = -1;
                            CurrentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", CurrentFrames.Right, FrameCounts.Right, SpriteImage);
                        }
                    }
                    else
                    {
                        CurrentFrames.WalkIdle = SpriteManager.PlayAnimation("Idle", "Actions", CurrentFrames.WalkIdle, FrameCounts.RunIdle, SpriteImage);
                    }
                }
                bool isIdleNow = GremlinState.IsCompletelyIdle();
                if (Settings.AllowRandomness && _isCombat == false && !_keyboardController.IsKeyboardMoving)
                {
                    if (isIdleNow && !_wasIdleLastFrame)
                    {
                        int interval = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                        _nextRandomActionTime = DateTime.Now.AddSeconds(interval);
                    }
                    if (isIdleNow && DateTime.Now >= _nextRandomActionTime)
                    {
                        GremlinState.SetState("Random");

                        int action = _rng.Next(0, 4);
                        switch (action)
                        {
                            case 0:
                                CurrentFrames.Click = 0;
                                GremlinState.UnlockState();
                                GremlinState.SetState("Click");
                                MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
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

            double targetLeft = Math.Max(SystemParameters.WorkArea.Left, Math.Min(this.Left + moveX, SystemParameters.WorkArea.Right - SpriteImage.ActualWidth));
            double targetTop = Math.Max(SystemParameters.WorkArea.Top, Math.Min(this.Top + moveY, SystemParameters.WorkArea.Bottom - SpriteImage.ActualHeight));

            const double step = 120;
            double dx = (targetLeft - this.Left) / step;
            double dy = (targetTop - this.Top) / step;

            DispatcherTimer moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _activeRandomMoveTimer = moveTimer;
            int moveCount = 0;
            moveTimer.Tick += (s, e) =>
            {
                this.Left += dx;
                this.Top += dy;
                moveCount++;
                if (Settings.EnableGravity)
                {
                    dy = 0;
                }
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    if (dx > 0)
                    {
                        SpriteFlipTransform.ScaleX = 1;
                        CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk", CurrentFrames.WalkRight, FrameCounts.WalkR, SpriteImage);
                    }
                    else
                    {
                        SpriteFlipTransform.ScaleX = -1;
                        CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk", CurrentFrames.WalkRight, FrameCounts.WalkR, SpriteImage);
                    }
                }
                if (moveCount >= step || !GremlinState.GetState("Random"))
                {
                    moveTimer.Stop();
                    GremlinState.SetState("Idle");
                    _activeRandomMoveTimer = null;
                }
            };
            moveTimer.Start();
        }

        #region All the shit for keyboard

        public void TriggerClickAnimation()
        {
            ResetIdleTimer();
            CurrentFrames.Click = 0;
            CurrentFrames.Idle = 0;
            GremlinState.UnlockState();
            GremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
            GremlinState.LockState();
        }

        public void ToggleCursorFollow()
        {

            if (_isCombat)
            {
                return;
            }
            _keyboardController.StopAllMovement();
            _activeRandomMoveTimer?.Stop();

            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;

            if (MouseSettings.FollowCursor && !_isCombat)
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Walking");
                GremlinState.LockState();
                MediaManager.PlaySound("run.wav", Settings.StartingChar);
            }
            else
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Idle");
            }
        }

        public void ToggleCombatMode()
        {
            if (!_isCombat)
            {
                _keyboardController.StopAllMovement();
                _activeRandomMoveTimer?.Stop();
                MouseSettings.FollowCursor = false;
            }
            _isCombat = !_isCombat;

            if (Settings.StartingChar == "Exu")
            {
                Settings.StartingChar = "Exu2";
            }
            else
            {
                Settings.StartingChar = "Exu";
            }
            MediaManager.PlaySound("intro.wav", Settings.StartingChar);
            FrameCounts = ConfigManager.LoadConfigChar(Settings.StartingChar);
            GremlinState.UnlockState();
            GremlinState.SetState("Intro");
            CurrentFrames.Idle = 0;
            CurrentFrames.Intro = 0;
        }

        public void TriggerLeftEmote()
        {
            if (!_isCombat)
            {
                return;
            }
            SpriteFlipTransform.ScaleX = -1;
            EmoteHelper("Emote3", "emote1.wav");
            CurrentFrames.Emote3 = 0;
            CurrentFrames.Idle = 0;

            if (_SummonInstance != null && _SummonInstance.IsVisible)
            {
                _SummonInstance.Close();
            }
            _SummonInstance = new Summon(-1);
            double offsetX = this.Width * -0.7;
            _SummonInstance.Left = this.Left + offsetX;
            _SummonInstance.Top = this.Top;
            _SummonInstance.Show();
        }
        public void ForceShutDown()
        {
            System.Windows.Application.Current.Shutdown();
        }
        public void TriggerRightEmote()
        {
            if (!_isCombat)
            {
                return;
            }
            SpriteFlipTransform.ScaleX = 1;
            EmoteHelper("Emote3", "emote3.wav");
            CurrentFrames.Emote3 = 0;
            CurrentFrames.Idle = 0;

            if (_SummonInstance != null && _SummonInstance.IsVisible)
            {
                _SummonInstance.Close();
            }
            _SummonInstance = new Summon(1);
            double offsetX = this.Width * 0.7;
            _SummonInstance.Left = this.Left + offsetX;
            _SummonInstance.Top = this.Top;
            _SummonInstance.Show();
        }

        public void ToggleSleep()
        {
            if (_isCombat)
            {
                return;
            }

            if (GremlinState.GetState("Sleeping"))
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Idle");
                ResetIdleTimer();
            }
            else
            {
                GremlinState.UnlockState();
                MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                GremlinState.SetState("Sleeping");
                GremlinState.LockState();
            }
        }

        public void TriggerRandomMove()
        {
            RandomMove();
        }

        public void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        #endregion
        private void SpriteImage_RightClick(object sender, MouseButtonEventArgs e)
        {
            TriggerClickAnimation();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState("Grab");
            MediaManager.PlaySound("grab.wav", Settings.StartingChar);
            DragMove();
            GremlinState.SetState("Idle");
            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor && _isCombat == false)
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

        private void TopHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            ToggleCombatMode();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (_isCombat)
            {
                ResetIdleTimer();
                return;
            }
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

        private void LeftHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerLeftEmote();
        }

        private void RightHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerRightEmote();
        }

        public void EmoteHelper(string emote, string mp3)
        {
            ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState(emote);
            MediaManager.PlaySound(mp3, Settings.StartingChar);
            GremlinState.LockState();
        }

        public static void ErrorClose(string errorMessage, string errorTitle, bool close)
        {
            if (Settings.AllowErrorMessages)
            {
                System.Windows.MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (close)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        public void HotSpot()
        {
            _hotspotDisable = !_hotspotDisable;
            LeftHotspot.IsEnabled = !LeftHotspot.IsEnabled;
            RightHotspot.IsEnabled = !RightHotspot.IsEnabled;
            TopHotspot.IsEnabled = !TopHotspot.IsEnabled;

            if (_hotspotDisable)
            {
                LeftHotspot.Background = new SolidColorBrush(Colors.Transparent);
                RightHotspot.Background = new SolidColorBrush(Colors.Transparent);
                TopHotspot.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                TopHotspot.Background = noColor;    
            }
        }
        public void ShowHotSpot()
        {
            _hotspotVisible = !_hotspotVisible;

            if (_hotspotVisible)
            {
                LeftHotspot.Background = new SolidColorBrush(Colors.Red);
                RightHotspot.Background = new SolidColorBrush(Colors.Blue);
                TopHotspot.Background = new SolidColorBrush(Colors.Purple);
            }
            else
            {
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                TopHotspot.Background = noColor;
            }
        }
    }
}

