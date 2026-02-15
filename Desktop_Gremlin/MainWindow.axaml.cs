using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DesktopGremlin.Quirks.Companion;
using System;
using static DesktopGremlin.ConfigManager;
namespace DesktopGremlin
{
    public partial class MainWindow : Window
    {
        private PixelPoint? _cursorScreen;
        protected Size? _followCursor_oldWindowSize;
        public Size? FollowCursor_oldWindowSize => _followCursor_oldWindowSize;

        private Random _rng = new Random();

        private AnimationStates _gremlinState = new AnimationStates();
        private FrameCounts _frameCounts = new FrameCounts();
        private CurrentFrames _currentFrames = new CurrentFrames();

        private AnimationController _animationController;
        private MovementController _movementController;
        private TimerController _timerController;
        private HotspotController _hotspotController;
        private KeyboardController _keyboardController;
        private FoodFollower _foodFollower;
        private Companion _companionInstance;
        private Summon _SummonInstance;
        private AppConfig _config;

        private string combatMode_originalSelectedChar = string.Empty;
        private bool _isCombat = false;
        public bool IsCombat => _isCombat;

        private Target _currentFood;
        private bool _clickThrough = false;

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeConfig();         
            PlayIntro();
        }

        public string GetSelectedCharacter()
        {
            return _config._selectedCharacter;
        }

        public void SetSelectedCharacter(string character)
        {
            _config._selectedCharacter = character;
            _frameCounts.LoadConfigChar(character);
        }

        public Companion GetCompanionInstance()
        {
            return _companionInstance;
        }

        public void ToggleClickThrough()
        {
            _clickThrough = !_clickThrough;

            // Original implementation, not working on avalonia
            /*
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, _exlLayaer);
            style |= _wsxLayer;

            if (_clickThrough)
            {
                style = style | _windowTrasnparent;
            }
            else
            {
                style = style & (~_windowTrasnparent);
            }
            SetWindowLong(hwnd, _exlLayaer, style);
            */
        }

        public void InitializeConfig()
        {
            ConfigManager.LoadMasterConfig();
            _frameCounts.LoadConfigChar(Settings.StartingChar);
            ConfigManager.ApplyXamlSettings(this);
            _timerController = new TimerController(this, _gremlinState, SpriteImage);
            _animationController = new AnimationController(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng);
            _movementController = new MovementController(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng);
            _hotspotController = new HotspotController(this, LeftHotspot, RightHotspot, TopHotspot, LeftDownHotspot, RightDownHotspot);
            _foodFollower = new FoodFollower(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage);
            _config = new AppConfig(this, _gremlinState, Settings.StartingChar);

            if (Settings.AllowKeyboard)
            {
                _keyboardController = new KeyboardController(this, _gremlinState, _currentFrames, _frameCounts, _rng);
            }
            SpriteImage.Source = new CroppedBitmap();
            _gremlinState.LockState();
            _animationController.Start();
            _timerController.Start();
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

        public PixelRect GetCombinedScreens()
        {
            PixelRect combined = Screens.All[0].Bounds;
            for (int i = 1; i < Screens.All.Count; i++)
                combined = combined.Union(Screens.All[i].Bounds);
            return combined;
        }

        public void FollowCursor_EnlargeMainWindow()
        {
            _followCursor_oldWindowSize = this.ClientSize;
            PixelRect combined = GetCombinedScreens();

            Width = combined.Width * 2;
            Height = combined.Height * 2;
        }

        public void FollowCursor_RestoreMainWindow()
        {
            this.Width = _followCursor_oldWindowSize.Value.Width;
            this.Height = _followCursor_oldWindowSize.Value.Height;
            _followCursor_oldWindowSize = null;
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
            _gremlinState.SetState("Hover");
            if (_gremlinState.GetState("Hover"))
            {
                Quirks.MediaManager.PlaySound("hover.wav",GetSelectedCharacter(), 5);
            }
            
        }
        private void SpriteImage_MouseLeave(object sender, PointerEventArgs e)
        {
            _keyboardController?.EnableKeyboardMovement();
            _gremlinState.SetState("Idle");
            _currentFrames.Hover = 0;            
        }
        private void Canvas_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (_followCursor_oldWindowSize is not null)
                {
                    _timerController.ResetIdleTimer();
                    _gremlinState.UnlockState();
                    _gremlinState.SetState("Idle");
                    FollowCursor_RestoreMainWindow();
                }
                else
                {
                    _timerController.ResetIdleTimer();
                    _gremlinState.UnlockState();
                    _gremlinState.SetState("Grab");
                    Quirks.MediaManager.PlaySound("grab.wav", GetSelectedCharacter());
                    BeginMoveDrag(e);
                }
            }

        }
        protected void Canvas_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _gremlinState.GetState("Grab"))
            {
                _gremlinState.SetState("Idle");
                MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
                if (MouseSettings.FollowCursor)
                {
                    _gremlinState.SetState("Walking");
                    _gremlinState.LockState();
                }
                _currentFrames.Grab = 0;
                if (MouseSettings.FollowCursor)
                {
                    Quirks.MediaManager.PlaySound("run.wav", GetSelectedCharacter());
                }
            }
        }
        private void TopHotspot_Click(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (!string.IsNullOrEmpty(Settings.CombatModeChar)) ToggleCombatMode();

                if (!string.IsNullOrEmpty(QuirkSettings.CompanionChar))
                {
                    if (_companionInstance != null && _companionInstance.IsVisible)
                    {
                        _companionInstance.Close();
                        return;
                    }
                    _companionInstance = new Companion();
                    _companionInstance.SetMainGremlin(this);
                    _companionInstance.Closed += (s, args) => _companionInstance = null;
                    _companionInstance.Show();   
                }

                if (!string.IsNullOrEmpty(Settings.FoodMode) && Settings.FoodMode.CompareTo("None") != 0)
                {
                    TriggerFood();
                }
            }
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

        private void StartFollowingFood()
        {
            Quirks.MediaManager.PlaySound("food.wav", GetSelectedCharacter());
            _foodFollower.StartFollowing(_currentFood, QuirkSettings.CurrentItemAcceleration);
        }

        public void EmoteHelper(string emote, string mp3)
        {
            _timerController.ResetIdleTimer();
            _gremlinState.UnlockState();
            _gremlinState.SetState(emote);
            Quirks.MediaManager.PlaySound(mp3, GetSelectedCharacter());
            _gremlinState.LockState();
        }

        public void HotSpot()
        {
            _hotspotController.ToggleHotspots();
        }
        public void ShowHotSpot()
        {
            _hotspotController.ShowHotspots();
        }
        public void ToggleGravity()
        {
            _timerController.ToggleGravity();
        }

        public void PlayIntro()
        {
            Quirks.MediaManager.PlaySound("intro.wav", GetSelectedCharacter());
            _frameCounts.LoadConfigChar(GetSelectedCharacter());
            _gremlinState.UnlockState();
            _gremlinState.SetState("Intro");
            _currentFrames.Idle = 0;
            _currentFrames.Intro = 0;
        }

        public PixelPoint? GetCursorScreen()
        {
            return _cursorScreen;
        }
        
        #region Keyboard Methods
        public void TriggerClickAnimation()
        {
            _timerController.ResetIdleTimer();
            _currentFrames.Click = 0;
            _currentFrames.Idle = 0;
            _gremlinState.UnlockState();
            _gremlinState.SetState("Click");
            Quirks.MediaManager.PlaySound("mambo.wav", GetSelectedCharacter());
            _gremlinState.LockState();
        }

        public void ToggleCursorFollow()
        {

            if (_isCombat)
            {
                return;
            }
            _keyboardController?.StopAllMovement();
            _movementController.StopRandomMove();

            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor && !_isCombat)
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Walking");
                _gremlinState.LockState();
                Quirks.MediaManager.PlaySound("run.wav", GetSelectedCharacter());
            }
            else
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Idle");
            }
        }
        public void TriggerFood()
        {
            if (_gremlinState.GetState("FollowItem"))
            {
                return;
            }
            _gremlinState.UnlockState();
            _gremlinState.SetState("FollowItem");
            _gremlinState.LockState();

            PixelRect workingArea = GetCombinedScreens();
            double screenWidth = workingArea.Width;
            double screenHeight = workingArea.Height;

            double randomLeft = _rng.NextDouble() * (screenWidth - Settings.FrameWidth) + workingArea.X;
            double randomTop = _rng.NextDouble() * (screenHeight - Settings.FrameHeight) + workingArea.Y;
            _currentFood = new Target();
            _currentFood.Position = new PixelPoint((int)Math.Round(randomLeft), (int)Math.Round(randomTop));
            _currentFood.Show();
            StartFollowingFood();
        }

        public void ToggleCombatMode()
        {
            if (!_isCombat)
            {
                _keyboardController.StopAllMovement();
                _movementController.StopRandomMove();
                MouseSettings.FollowCursor = false;
            }
            _isCombat = !_isCombat;

            if (string.IsNullOrEmpty(combatMode_originalSelectedChar))
            {
                combatMode_originalSelectedChar = GetSelectedCharacter();
                SetSelectedCharacter(Settings.CombatModeChar);
            }
            else
            {
                combatMode_originalSelectedChar = string.Empty;
                SetSelectedCharacter(combatMode_originalSelectedChar);
            }
            PlayIntro();
        }

        public void TriggerLeftEmote()
        {
            if (!string.IsNullOrEmpty(Settings.CombatModeChar) && !_isCombat)
            {
                return;
            }
            if (this.RenderTransform is ScaleTransform currentTransform && currentTransform.ScaleX < 0)
            {
                //this is needed because in this case the buttons are reversed, so the left button is actually the right emote
                TriggerRightEmote();
                return;
            }
            if (Settings.MirrorXSprite)
            {
                this.RenderTransform = new ScaleTransform(-1.0, 1.0);
                EmoteHelper("Emote3", "emote1.wav");
                _currentFrames.Emote3 = 0;
                _currentFrames.Idle = 0;
            }
            else
            {
                EmoteHelper("Emote1", "emote1.wav");
                _currentFrames.Emote1 = 0;
                _currentFrames.Idle = 0;
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
            _currentFrames.Emote3 = 0;
            _currentFrames.Idle = 0;

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
            if (this.RenderTransform is ScaleTransform currentTransform && currentTransform.ScaleX < 0)
            {
                //this is needed because in this case the buttons are reversed, so the left button is actually the right emote
                TriggerRightDownEmote();
                return;
            }
            _currentFrames.Emote2 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote2", "emote2.wav");
        }
        public void TriggerRightDownEmote()
        {
            if (this.RenderTransform is ScaleTransform currentTransform && currentTransform.ScaleX < 0)
            {
                //this is needed because in this case the buttons are reversed, so the right button is actually the left emote
                TriggerLeftDownEmote();
                return;
            }
            _currentFrames.Emote4 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote4", "emote4.wav");
        }
        
        public void ToggleSleep()
        {
            if (_isCombat)
            {
                return;
            }

            if (_gremlinState.GetState("Sleeping"))
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Idle");
                _timerController.ResetIdleTimer();
            }
            else
            {
                _gremlinState.UnlockState();
                Quirks.MediaManager.PlaySound("sleep.wav", GetSelectedCharacter());
                _gremlinState.SetState("Sleeping");
                _gremlinState.LockState();
            }
        }
        public void ToggleCompanion()
        {
            if (_companionInstance != null && _companionInstance.IsVisible && QuirkSettings.CompanionChar != null)
            {
                _companionInstance.Close();
                return;
            }
            _companionInstance = new Companion();
            _companionInstance.SetMainGremlin(this);
            _companionInstance.Closed += (s, args) => _companionInstance = null;
            _companionInstance.Show();
        }
        public void TriggerRandomMove()
        {
            _movementController.RandomMove();
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

