using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using static DesktopGremlin.ConfigManager;
namespace DesktopGremlin
{
    public partial class MainWindow : Window
    {
        public string _SelectedCharacter;
        private Size? _followCursor_oldWindowSize;
        private PixelPoint? _cursorScreen;
        private DateTime _nextRandomActionTime = DateTime.Now.AddSeconds(1);

        private Random _rng = new Random();
        private AppConfig _config;

        private DispatcherTimer _masterTimer;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _activeRandomMoveTimer;
        private DispatcherTimer _gravityTimer;
        private DispatcherTimer _followTimer;

        private bool _isCombat = false;
        private bool _hotspotVisible = false;
        private bool _hotspotDisable = false;
        private bool _wasIdleLastFrame = false; 
        public bool IsCombat => _isCombat;

        private Companion _companionInstance;
        private Summon _SummonInstance;
        private AnimationStates GremlinState = new AnimationStates();
        private FrameCounts FrameCounts = new FrameCounts();
        private CurrentFrames CurrentFrames = new CurrentFrames();
        private Target _currentFood;

        private KeyboardController _keyboardController;

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeConfig();         
            MediaManager.PlaySound("intro.wav", _SelectedCharacter);
        }

        public void InitializeConfig()
        {
            ConfigManager.LoadMasterConfig();
            ConfigManager.ApplyXamlSettings(this);
            InitializeAnimations();
            InitializeTimers();

            _SelectedCharacter = Settings.StartingChar;

            SpriteImage.Source = new CroppedBitmap();
            FrameCounts.LoadConfigChar(_SelectedCharacter);

            if (Settings.AllowKeyboard)
            {
                _keyboardController = new KeyboardController(this, GremlinState, CurrentFrames, FrameCounts, _rng);
            }
            _config = new AppConfig(this, GremlinState);
            GremlinState.LockState();
        }

        public void InitializeTimers()
        {
            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(Settings.SleepTime);
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();
            _gravityTimer = new DispatcherTimer();
            _gravityTimer.Interval = TimeSpan.FromMilliseconds(30);
            _gravityTimer.Tick += Gravity_Tick;
            if (Settings.EnableGravity)
            {
                _gravityTimer.Start();
            }
        }

        private void Gravity_Tick(object sender, EventArgs e)
        {
            Screen screen = TopLevel.GetTopLevel(this)?.Screens.ScreenFromVisual(this);
            if (screen != null)
            {
                double bottomLimit;
                bottomLimit = screen?.WorkingArea.Bottom ?? 0;
                bottomLimit -= SpriteImage.Bounds.Height;
                if (GremlinState.GetState("Grab"))
                {
                    return;
                }
                if (this.Position.Y < bottomLimit && this.Position.Y > 0)
                {
                    Position = new PixelPoint(Position.X, (int)(Position.Y + Math.Round(Settings.SvGravity)));
                }
            }
        }
        
        public static void ErrorClose(string errorMessage, string errorTitle, bool close, int width = 400, int height = 150)
        {
            if (Settings.AllowErrorMessages)
            {
                var msg = new Window
                {
                    Title = errorTitle,
                    Content = new TextBlock { Text = errorMessage },
                    Width = width,
                    Height = height,
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
            if (!GremlinState.GetState(stateName))
            {
                return currentFrame;
            }
            currentFrame = SpriteManager.PlayAnimation(stateName, folder, currentFrame, frameCount, SpriteImage, _SelectedCharacter);

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
                CurrentFrames.Intro = PlayAnimationIfActive("Intro", "Actions", CurrentFrames.Intro, FrameCounts.Intro, true);
                CurrentFrames.Outro = PlayAnimationIfActive("Outro", "Actions", CurrentFrames.Outro, FrameCounts.Outro, true);
                CurrentFrames.Click = PlayAnimationIfActive("Click", "Actions", CurrentFrames.Click, FrameCounts.Click, true);

                if (MouseSettings.FollowCursor && GremlinState.GetState("Walking") && _isCombat == false)
                {
                    //In some scenarios (ex. Linux Wayland) it's not possible to get global cursor position, so i'm enlarging the main window when the follow mouse feature is active
                    if (_followCursor_oldWindowSize is null)
                    {
                        FollowCursor_EnlargeMainWindow();
                    }
                    var spriteBounds = SpriteImage.Bounds;
                    var spriteCenterScreen = new Point(
                        this.Position.X + spriteBounds.X + spriteBounds.Width / 2.0,
                        this.Position.Y + spriteBounds.Y + spriteBounds.Height / 2.0
                    );

                    if (_cursorScreen is null) _cursorScreen = new PixelPoint((int)this.Position.X, (int)this.Position.Y);
                    double dx = _cursorScreen.Value.X - spriteCenterScreen.X;
                    double dy = _cursorScreen.Value.Y - spriteCenterScreen.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (Settings.EnableGravity)
                    {
                        dy = 0;
                        distance = Math.Abs(dx);
                    }
                    else
                    {
                        distance = Math.Sqrt(dx * dx + dy * dy);
                    }

                    if (distance > Settings.FollowRadius)
                    {
                        double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);
                        double nx = dx / distance;
                        double ny = dy / distance;
                        double moveX = nx * step;
                        double moveY = ny * step;
                        if (Settings.EnableGravity)
                        {
                            dy = 0;
                        }

                        Position = new PixelPoint((int)(Position.X + Math.Round(moveX)), (int)(Position.Y + Math.Round(moveY)));
                        double angle = Math.Atan2(moveY, moveX) * (180.0 / Math.PI);

                        if (angle < 0) angle += 360;
                        if (angle >= 337.5 || angle < 22.5)
                        {
                            if (Settings.MirrorXSprite)
                            {
                                this.RenderTransform = new ScaleTransform(1.0, 1.0);
                            }
                            CurrentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", CurrentFrames.Right, FrameCounts.Right, SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 22.5 && angle < 67.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.DownRight = SpriteManager.PlayAnimation("downRight","Run",CurrentFrames.DownRight,FrameCounts.DownRight,SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 67.5 && angle < 112.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.Down = SpriteManager.PlayAnimation("runDown","Run",CurrentFrames.Down,FrameCounts.Down,SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 112.5 && angle < 157.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft","Run",CurrentFrames.DownLeft,FrameCounts.DownLeft,SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 157.5 && angle < 202.5)
                        {
                            if (Settings.MirrorXSprite)
                            {
                                this.RenderTransform = new ScaleTransform(-1.0, 1.0);
                                CurrentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", CurrentFrames.Right, FrameCounts.Right, SpriteImage, _SelectedCharacter);
                            }
                            else
                            {
                                CurrentFrames.Left = SpriteManager.PlayAnimation("runLeft","Run",CurrentFrames.Left,FrameCounts.Left,SpriteImage, _SelectedCharacter);
                            }
                        }
                        else if (angle >= 202.5 && angle < 247.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft","Run",CurrentFrames.UpLeft,FrameCounts.UpLeft,SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 247.5 && angle < 292.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.Up = SpriteManager.PlayAnimation("runUp","Run",CurrentFrames.Up,FrameCounts.Up,SpriteImage, _SelectedCharacter);
                        }
                        else if (angle >= 292.5 && angle < 337.5 && Settings.EnableGravity == false)
                        {
                            CurrentFrames.UpRight = SpriteManager.PlayAnimation("upRight","Run",CurrentFrames.UpRight,FrameCounts.UpRight,SpriteImage, _SelectedCharacter);
                        }
                    }
                    else
                    {
                        CurrentFrames.WalkIdle = SpriteManager.PlayAnimation("runIdle", "Actions", CurrentFrames.WalkIdle, FrameCounts.WalkIdle, SpriteImage, _SelectedCharacter);
                    }
                }
                else if (_followCursor_oldWindowSize is not null)
                {
                    FollowCursor_RestoreMainWindow();
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
                                MediaManager.PlaySound("mambo.wav", _SelectedCharacter);
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

        public PixelRect GetCombinedScreens()
        {
            PixelRect combined = Screens.All[0].Bounds;
            for (int i = 1; i < Screens.All.Count; i++)
                combined = combined.Union(Screens.All[i].Bounds);
            return combined;
        }

        private void FollowCursor_EnlargeMainWindow()
        {
            _followCursor_oldWindowSize = this.ClientSize;
            PixelRect combined = GetCombinedScreens();

            Width = combined.Width * 2;
            Height = combined.Height * 2;
        }

        private void FollowCursor_RestoreMainWindow()
        {
            this.Width = _followCursor_oldWindowSize.Value.Width;
            this.Height = _followCursor_oldWindowSize.Value.Height;
            _followCursor_oldWindowSize = null;
        }

        private void RandomMove()
        {
            _activeRandomMoveTimer?.Stop();
            GremlinState.SetState("Random");

            double moveX = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;
            double moveY = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;

            PixelRect workingArea = GetCombinedScreens();
            double targetLeft = Math.Max(workingArea.X, Math.Min(Position.X + moveX, workingArea.Right - SpriteImage.Bounds.Width));
            double targetTop = Math.Max(workingArea.Y, Math.Min(Position.Y + moveY, workingArea.Bottom - SpriteImage.Bounds.Height));

            int step = Settings.WalkDistance;
            double dx = (targetLeft - this.Position.X) / step;
            double dy = (targetTop - this.Position.Y) / step;

            DispatcherTimer moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _activeRandomMoveTimer = moveTimer;

            int moveCount = 0;

            moveTimer.Tick += (s, e) =>
            {
                Position = new PixelPoint((int)(Position.X + Math.Round(dx)), (int)(Position.Y + Math.Round(dy)));
                moveCount++;
                if (Settings.EnableGravity)
                {
                    dy = 0;
                }
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    if (dx > 0)
                    {
                        this.RenderTransform = new ScaleTransform(1.0, 1.0);
                        CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk", CurrentFrames.WalkRight, FrameCounts.WalkRight, SpriteImage, _SelectedCharacter);
                    }
                    else
                    {
                        if (Settings.MirrorXSprite)
                        {
                            this.RenderTransform = new ScaleTransform(-1.0, 1.0);
                            CurrentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk", CurrentFrames.WalkRight, FrameCounts.WalkRight, SpriteImage, _SelectedCharacter);
                        }
                        else
                        {
                            CurrentFrames.WalkLeft = SpriteManager.PlayAnimation("walkLeft", "Walk", CurrentFrames.WalkLeft, FrameCounts.WalkLeft, SpriteImage, _SelectedCharacter);
                        }
                    }
                }
                else
                {
                    if (dy > 0)
                    {
                        CurrentFrames.WalkDown = SpriteManager.PlayAnimation("walkDown", "Walk", CurrentFrames.WalkDown, FrameCounts.WalkDown, SpriteImage, _SelectedCharacter);
                    }
                    else
                    {
                        CurrentFrames.WalkUp = SpriteManager.PlayAnimation("walkUp", "Walk", CurrentFrames.WalkUp, FrameCounts.WalkUp,SpriteImage, _SelectedCharacter);
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
        private void StartFollowingFood()
        {
            MediaManager.PlaySound("food.wav", Settings.StartingChar);
            double _startingSpeed = Quirks.CurrentItemAcceleration;
            _followTimer?.Stop();
            _followTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _followTimer.Tick += (s, e) =>
            {
                if (_currentFood == null || !_currentFood.IsVisible)
                {
                    _followTimer.Stop();
                    GremlinState.UnlockState();
                    GremlinState.SetState("Sleeping");
                    GremlinState.LockState();
                    return;
                }

                Point foodCenter = _currentFood.GetCenter();
                double gremlinCenterX = this.Position.X + this.Width / 2;
                double gremlinCenterY = this.Position.Y + this.Height / 2;
                double dx = foodCenter.X - gremlinCenterX;
                double dy = foodCenter.Y - gremlinCenterY;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance - 25 < Settings.SpriteSize / 2 || !GremlinState.GetState("FollowItem"))
                {
                    MediaManager.PlaySound("eat.wav", Settings.StartingChar);
                    _currentFood.Close();
                    return;
                }

                _startingSpeed = Math.Min(_startingSpeed + Quirks.ItemAcceleration, Quirks.MaxItemAcceleration);
                double step = Math.Min(_startingSpeed, distance);

                Position = new PixelPoint(Position.X +(int)Math.Round(dx / distance * step), Position.Y + (int)Math.Round(dy / distance * step));

                double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
                if (angle < 0) angle += 360;

                if (angle >= 337.5 || angle < 22.5)
                {
                    CurrentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", CurrentFrames.Right, FrameCounts.Right, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 22.5 && angle < 67.5)
                {
                    CurrentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run", CurrentFrames.DownRight, FrameCounts.DownRight, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 67.5 && angle < 112.5)
                {
                    CurrentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run", CurrentFrames.Down, FrameCounts.Down, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 112.5 && angle < 157.5)
                {
                    CurrentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run", CurrentFrames.DownLeft, FrameCounts.DownLeft, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 157.5 && angle < 202.5)
                {
                    CurrentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run", CurrentFrames.Left, FrameCounts.Left, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 202.5 && angle < 247.5)
                {
                    CurrentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run", CurrentFrames.UpLeft, FrameCounts.UpLeft, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 247.5 && angle < 292.5)
                {
                    CurrentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run", CurrentFrames.Up, FrameCounts.Up, SpriteImage, _SelectedCharacter);
                }
                else if (angle >= 292.5 && angle < 337.5)
                {
                    CurrentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run", CurrentFrames.UpRight, FrameCounts.UpRight, SpriteImage, _SelectedCharacter);
                }

                GremlinState.SetState("Walking");
            };
            _followTimer.Start();
        }
        private void SpriteImage_RightClick(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                TriggerClickAnimation();
            }
        }

        private void SpriteImage_MouseEnter(object sender, PointerEventArgs e)
        {
            _keyboardController?.DisableKeyboardMovement();
            GremlinState.SetState("Hover");
            if (GremlinState.GetState("Hover"))
            {
                MediaManager.PlaySound("hover.wav",_SelectedCharacter, 5);
            }
            
        }
        private void SpriteImage_MouseLeave(object sender, PointerEventArgs e)
        {
            _keyboardController?.EnableKeyboardMovement();
            GremlinState.SetState("Idle");
            CurrentFrames.Hover = 0;            
        }
        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (_followCursor_oldWindowSize is not null)
                {
                    ResetIdleTimer();
                    GremlinState.UnlockState();
                    GremlinState.SetState("Idle");
                    FollowCursor_RestoreMainWindow();
                }
                else
                {
                    ResetIdleTimer();
                    GremlinState.UnlockState();
                    GremlinState.SetState("Grab");
                    MediaManager.PlaySound("grab.wav", _SelectedCharacter);
                    BeginMoveDrag(e);
                }
            }

        }
        protected void Canvas_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && GremlinState.GetState("Grab"))
            {
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
                    MediaManager.PlaySound("run.wav", _SelectedCharacter);
                }
            }
        }
        private void TopHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (!string.IsNullOrEmpty(Settings.CombatModeChar)) ToggleCombatMode();

                else if (!string.IsNullOrEmpty(Settings.CompanionChar))
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

                else if (GremlinState.GetState("FollowItem"))
                {
                    return;
                }

                else
                {
                    GremlinState.UnlockState();
                    GremlinState.SetState("FollowItem");
                    GremlinState.LockState();
                    PixelRect workingArea = GetCombinedScreens();
                    double screenWidth = workingArea.Width;
                    double screenHeight = workingArea.Height;

                    Random rng = new Random();
                    double randomLeft = rng.NextDouble() * (screenWidth - Settings.FrameWidth) + workingArea.X;
                    double randomTop = rng.NextDouble() * (screenHeight - Settings.FrameHeight) + workingArea.Y;
                    _currentFood = new Target();
                    _currentFood.Position = new PixelPoint((int)Math.Round(randomLeft), (int)Math.Round(randomTop));
                    _currentFood.Show();
                    StartFollowingFood();
                }
            }
        }

        public void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
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
                MediaManager.PlaySound("sleep.wav", _SelectedCharacter);
                GremlinState.SetState("Sleeping");
                GremlinState.LockState();
            }
        }
        public void EmoteHelper(string emote, string mp3)
        {
            ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState(emote);
            MediaManager.PlaySound(mp3, _SelectedCharacter);
            GremlinState.LockState();
        }

        private void LeftHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TriggerLeftEmote();
            }
        }
        private void LeftDownHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TriggerLeftDownEmote();
            }
        }
        private void RightHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TriggerRightEmote();
            }
        }
        private void RightDownHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TriggerRightDownEmote();  
            }
        }

        public void HotSpot()
        {
            _hotspotDisable = !_hotspotDisable;
            LeftHotspot.IsEnabled = !LeftHotspot.IsEnabled;
            LeftDownHotspot.IsEnabled = !LeftDownHotspot.IsEnabled;
            RightHotspot.IsEnabled = !RightHotspot.IsEnabled;
            RightDownHotspot.IsEnabled = !RightDownHotspot.IsEnabled;
            TopHotspot.IsEnabled = !TopHotspot.IsEnabled;

            if (_hotspotDisable)
            {
                LeftHotspot.Background = new SolidColorBrush(Colors.Transparent);
                RightHotspot.Background = new SolidColorBrush(Colors.Transparent);
                TopHotspot.Background = new SolidColorBrush(Colors.Transparent);
                LeftDownHotspot.Background = new SolidColorBrush(Colors.Transparent);
                RightDownHotspot.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                var noColor = (ImmutableSolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                TopHotspot.Background = noColor;
                LeftDownHotspot.Background = noColor;
                RightDownHotspot.Background = noColor;    
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
                LeftDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
                RightDownHotspot.Background = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                var noColor = (ImmutableSolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                TopHotspot.Background = noColor;
                LeftDownHotspot.Background = noColor;
                RightDownHotspot.Background = noColor;
            }
        }
        public void ToggleGravity()
        {
            Settings.EnableGravity = !Settings.EnableGravity;
            if (Settings.EnableGravity)
            {
                _gravityTimer.Start();
            }
            else
            {
                _gravityTimer.Stop();
            }
        }
        #region All the shit for keyboard
        public void TriggerClickAnimation()
        {
            ResetIdleTimer();
            CurrentFrames.Click = 0;
            CurrentFrames.Idle = 0;
            GremlinState.UnlockState();
            GremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", _SelectedCharacter);
            GremlinState.LockState();
        }

        public void ToggleCursorFollow()
        {

            if (_isCombat)
            {
                return;
            }
            _keyboardController?.StopAllMovement();
            _activeRandomMoveTimer?.Stop();

            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor && !_isCombat)
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Walking");
                GremlinState.LockState();
                MediaManager.PlaySound("run.wav", _SelectedCharacter);
            }
            else
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Idle");
            }
        }

        public void TriggerFood()
        {
            if (GremlinState.GetState("FollowItem"))
            {
                return;
            }
            GremlinState.UnlockState();
            GremlinState.SetState("FollowItem");
            GremlinState.LockState();
            PixelRect workingArea = GetCombinedScreens();
            double screenWidth = workingArea.Width;
            double screenHeight = workingArea.Height;

            Random rng = new Random();
            double randomLeft = rng.NextDouble() * (screenWidth - Settings.FrameWidth) + workingArea.X;
            double randomTop = rng.NextDouble() * (screenHeight - Settings.FrameHeight) + workingArea.Y;
            _currentFood.Position = new PixelPoint((int)Math.Round(randomLeft), (int)Math.Round(randomTop));
            _currentFood.Show();
            StartFollowingFood();
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

            if (_SelectedCharacter == Settings.StartingChar)
            {
                _SelectedCharacter = Settings.CombatModeChar;
            }
            else
            {
                _SelectedCharacter = Settings.StartingChar;
            }
            MediaManager.PlaySound("intro.wav", _SelectedCharacter);
            FrameCounts.LoadConfigChar(_SelectedCharacter);
            GremlinState.UnlockState();
            GremlinState.SetState("Intro");
            CurrentFrames.Idle = 0;
            CurrentFrames.Intro = 0;
        }

        public void TriggerLeftEmote()
        {
            if (!string.IsNullOrEmpty(Settings.CombatModeChar) && !_isCombat)
            {
                return;
            }
            if (Settings.MirrorXSprite)
            {
                this.RenderTransform = new ScaleTransform(-1.0, 1.0);
                EmoteHelper("Emote3", "emote1.wav");
                CurrentFrames.Emote3 = 0;
                CurrentFrames.Idle = 0;
            }
            else
            {
                EmoteHelper("Emote1", "emote1.wav");
                CurrentFrames.Emote1 = 0;
                CurrentFrames.Idle = 0;
            }

            if (!string.IsNullOrEmpty(Settings.SummonChar))
            {
                if (_SummonInstance != null && _SummonInstance.IsVisible)
                {
                    _SummonInstance.Close();
                }
                _SummonInstance = new Summon(-1.0);
                double offsetX = this.Width * -0.7;
                _SummonInstance.Position = new PixelPoint(Position.X + (int)Math.Round(offsetX), Position.Y);
                _SummonInstance.Show();   
            }
        }
        public void ForceShutDown()
        {
            Environment.Exit(1);
        }
        public void TriggerRightEmote()
        {
            if (!string.IsNullOrEmpty(Settings.CombatModeChar) && !_isCombat)
            {
                return;
            }
            if (Settings.MirrorXSprite)
            {
                this.RenderTransform = new ScaleTransform(1.0, 1.0);
            }
            EmoteHelper("Emote3", "emote3.wav");
            CurrentFrames.Emote3 = 0;
            CurrentFrames.Idle = 0;

            if (!string.IsNullOrEmpty(Settings.SummonChar))
            {
                if (_SummonInstance != null && _SummonInstance.IsVisible)
                {
                    _SummonInstance.Close();
                }
                _SummonInstance = new Summon(1);
                double offsetX = this.Width * 0.7;
                _SummonInstance.Position = new PixelPoint(Position.X + (int)Math.Round(offsetX), Position.Y);
                _SummonInstance.Show();
            }
        }
        public void TriggerLeftDownEmote()
        {
            CurrentFrames.Emote2 = 0;
            CurrentFrames.Idle = 0;
            EmoteHelper("Emote2", "emote2.wav");
        }
        public void TriggerRightDownEmote()
        {
            CurrentFrames.Emote4 = 0;
            CurrentFrames.Idle = 0;
            EmoteHelper("Emote4", "emote4.wav");
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
                MediaManager.PlaySound("sleep.wav", _SelectedCharacter);
                GremlinState.SetState("Sleeping");
                GremlinState.LockState();
            }
        }

        public void TriggerRandomMove()
        {
            RandomMove();
        }


        #endregion
   

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Automatically center to pointer position when resizing windows, useful for mouse follow feature
            if (_cursorScreen.HasValue)
            {
                var newPos = new PixelPoint(
                    (int)Math.Round(_cursorScreen.Value.X - this.Bounds.Width / 2.0 ),
                    (int)Math.Round(_cursorScreen.Value.Y - this.Bounds.Height / 2.0)
                );

                Position = newPos;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            _cursorScreen = this.PointToScreen(e.GetPosition(this));
        }
    }
}

