using DesktopGremlin.Quirks.Companion;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private int _windowTrasnparent = 0x20;
        private int _wsxLayer = 0x80000;
        private int _exlLayaer = -20;
        private Random _rng = new Random();

        private AnimationStates GremlinState = new AnimationStates();
        private FrameCounts FrameCounts = new FrameCounts();
        private CurrentFrames CurrentFrames = new CurrentFrames();

        private AnimationController _animationController;
        private MovementController _movementController;
        private TimerController _timerController;
        private HotspotController _hotspotController;
        private KeyboardController _keyboardController;
        private FoodFollower _foodFollower;
        private Companion _companionInstance;
        private AppConfig _config;

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
            MediaManager.PlaySound("intro.wav", Settings.StartingChar);
        }

        public void ToggleClickThrough()
        {
            _clickThrough = !_clickThrough;

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
        }

        public void InitializeConfig()
        {
            ConfigManager.LoadMasterConfig();
            FrameCounts.LoadConfigChar(Settings.StartingChar);
            ConfigManager.ApplyXamlSettings(this);


            _timerController = new TimerController(this, GremlinState);
            _animationController = new AnimationController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage, _rng);
            _movementController = new MovementController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage, _rng);
            _hotspotController = new HotspotController(this, LeftHotspot, RightHotspot, TopHotspot, LeftDownHotspot, RightDownHotspot);
            _foodFollower = new FoodFollower(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage);
            _config = new AppConfig(this, GremlinState);

            if (Settings.AllowKeyboard)
            {
                _keyboardController = new KeyboardController(this, GremlinState, CurrentFrames, FrameCounts, _rng);
            }

            SpriteImage.Source = new CroppedBitmap();
            GremlinState.LockState();

            _animationController.Start();
            _timerController.Start();
        }

        public static void ErrorClose(string errorMessage, string errorTitle, bool close)
        {
            if (Settings.AllowErrorMessages)
            {
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (close)
            {
                Application.Current.Shutdown();
            }
        }

        private void SpriteImage_RightClick(object sender, MouseButtonEventArgs e)
        {
            _timerController.ResetIdleTimer();
            CurrentFrames.Click = 0;
            GremlinState.UnlockState();
            GremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
            GremlinState.LockState();
        }

        private void SpriteImage_MouseEnter(object sender, MouseEventArgs e)
        {
            _keyboardController?.DisableKeyboardMovement();
            GremlinState.SetState("Hover");
            if (GremlinState.GetState("Hover"))
            {
                MediaManager.PlaySound("hover.wav", Settings.StartingChar, 5);
            }
        }

        private void SpriteImage_MouseLeave(object sender, MouseEventArgs e)
        {
            _keyboardController?.EnableKeyboardMovement();
            GremlinState.SetState("Idle");
            CurrentFrames.Hover = 0;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _timerController.ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState("Grab");
            MediaManager.PlaySound("grab.wav", Settings.StartingChar);
            DragMove();
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

        private void TopHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            if (GremlinState.GetState("FollowItem"))
            {
                return;
            }
            GremlinState.UnlockState();
            GremlinState.SetState("FollowItem");
            GremlinState.LockState();

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            double randomLeft = _rng.NextDouble() * (screenWidth - Settings.FrameWidth) + SystemParameters.WorkArea.Left;
            double randomTop = _rng.NextDouble() * (screenHeight - Settings.FrameHeight) + SystemParameters.WorkArea.Top;
            _currentFood = new Target
            {
                Left = randomLeft,
                Top = randomTop
            };
            _currentFood.Show();
            StartFollowingFood();
        }

        private void LeftHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentFrames.Emote1 = 0;
            EmoteHelper("Emote1", "emote1.wav");
        }

        private void LeftDownHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentFrames.Emote2 = 0;
            EmoteHelper("Emote2", "emote2.wav");
        }

        private void RightHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentFrames.Emote3 = 0;
            EmoteHelper("Emote3", "emote3.wav");
        }

        private void RightDownHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentFrames.Emote4 = 0;
            EmoteHelper("Emote4", "emote4.wav");
        }

        private void StartFollowingFood()
        {
            MediaManager.PlaySound("food.wav", Settings.StartingChar);
            _foodFollower.StartFollowing(_currentFood, QuirkSettings.CurrentItemAcceleration);
        }

        private void EmoteHelper(string emote, string mp3)
        {
            _timerController.ResetIdleTimer();
            GremlinState.UnlockState();
            GremlinState.SetState(emote);
            MediaManager.PlaySound(mp3, Settings.StartingChar);
            GremlinState.LockState();
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
        public void TriggerClickAnimation()
        {
            _timerController.ResetIdleTimer();
            CurrentFrames.Click = 0;
            CurrentFrames.Idle = 0;
            GremlinState.UnlockState();
            GremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
            GremlinState.LockState();
        }

        public void ToggleCursorFollow()
        {
            _keyboardController?.StopAllMovement();
            _movementController.StopRandomMove();

            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor)
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
        public void TriggerFood()
        {
            if (GremlinState.GetState("FollowItem"))
            {
                return;
            }
            GremlinState.UnlockState();
            GremlinState.SetState("FollowItem");
            GremlinState.LockState();

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            double randomLeft = _rng.NextDouble() * (screenWidth - Settings.FrameWidth) + SystemParameters.WorkArea.Left;
            double randomTop = _rng.NextDouble() * (screenHeight - Settings.FrameHeight) + SystemParameters.WorkArea.Top;
            _currentFood = new Target
            {
                Left = randomLeft,
                Top = randomTop
            };
            _currentFood.Show();
            StartFollowingFood();
        }

        public void TriggerLeftEmote()
        {
            CurrentFrames.Emote1 = 0;
            CurrentFrames.Idle = 0;
            EmoteHelper("Emote1", "emote1.wav");
        }
        public void ForceShutDown()
        {
            Application.Current.Shutdown();
        }
        public void TriggerRightEmote()
        {
            CurrentFrames.Emote3 = 0;
            CurrentFrames.Idle = 0;
            EmoteHelper("Emote3", "emote3.wav");
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
            if (GremlinState.GetState("Sleeping"))
            {
                GremlinState.UnlockState();
                GremlinState.SetState("Idle");
                _timerController.ResetIdleTimer();
            }
            else
            {
                GremlinState.UnlockState();
                MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                GremlinState.SetState("Sleeping");
                GremlinState.LockState();
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
    }
}