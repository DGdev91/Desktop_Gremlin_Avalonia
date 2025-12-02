using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static ConfigManager;

namespace Desktop_Gremlin
{
    public class KeyboardController
    {
        private readonly Gremlin _gremlin;
        private readonly AnimationStates _gremlinState;
        private readonly CurrentFrames _currentFrames;
        private readonly FrameCounts _frameCounts;
        private readonly Random _rng;

        private bool _isKeyboardMoving = false;
        private DispatcherTimer _keyboardMoveTimer;
        private double _keyboardMoveSpeedX = 0;
        private double _keyboardMoveSpeedY = 0;
        private double KEY_MOVE_SPEED = MouseSettings.Speed;

        public bool IsKeyboardMoving => _isKeyboardMoving;

        public KeyboardController(Gremlin gremlin,AnimationStates gremlinState,CurrentFrames currentFrames,FrameCounts frameCounts,Random rng)
        {
            _gremlin = gremlin;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _rng = rng;

            InitializeKeyboardTimer();
            AttachKeyboardEvents();
        }

        private void InitializeKeyboardTimer()
        {
            _keyboardMoveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _keyboardMoveTimer.Tick += KeyboardMove_Tick;
        }

        private void AttachKeyboardEvents()
        {
            _gremlin.KeyDown += Gremlin_KeyDown;
            _gremlin.KeyUp += Gremlin_KeyUp;
            _gremlin.Focusable = true;
            _gremlin.Focus();
        }
        private void Gremlin_KeyDown(object sender, KeyEventArgs e)
        {
            _gremlin.ResetIdleTimer();

            if (e.Key == Key.W || e.Key == Key.Up ||e.Key == Key.S || e.Key == Key.Down ||
                e.Key == Key.A || e.Key == Key.Left ||
                e.Key == Key.D || e.Key == Key.Right)
            {
                HandleMovementKeyDown(e.Key);
                return;
            }
            switch (e.Key)
            {
                case Key.Space:
                    _gremlin.TriggerClickAnimation();
                    break;

                case Key.E:
                    _gremlin.ToggleCursorFollow();
                    break;

                case Key.Q:
                    _gremlin.ToggleCombatMode();
                    break;
                case Key.D1:
                case Key.NumPad1:
                    _gremlin.TriggerLeftEmote();
                    break;
                case Key.D2:
                case Key.NumPad2:
                    _gremlin.TriggerRightEmote();
                    break;

                case Key.R:
                    if (!_isKeyboardMoving)
                    {
                        _gremlin.TriggerRandomMove();
                    }
                    break;
                case Key.T:
                    _gremlin.ToggleSleep();
                    break;
                case Key.Escape:
                    StopAllMovement();
                    break;
                case Key.F1:
                    ShowKeyboardHelp();
                    break;
                case Key.X:
                    _gremlin.ForceShutDown();
                    break;
            }
        }
        private void Gremlin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W || e.Key == Key.Up ||e.Key == Key.S || e.Key == Key.Down ||
                e.Key == Key.A || e.Key == Key.Left ||
                e.Key == Key.D || e.Key == Key.Right)
            {
                HandleMovementKeyUp(e.Key);
            }
        }
        private void HandleMovementKeyDown(Key key)
        {
            if (_gremlin.IsCombat)
            {
                return;
            }
            if (MouseSettings.FollowCursor)
            {
                MouseSettings.FollowCursor = false;
                _gremlinState.UnlockState();
                _gremlinState.SetState("Idle");
            }

            bool wasMoving = _isKeyboardMoving;

            switch (key)
            {
                case Key.W:
                case Key.Up:
                    if (!Settings.EnableGravity)
                    {
                        _keyboardMoveSpeedY = -KEY_MOVE_SPEED;
                    }
                     
                    break;
                case Key.S:
                case Key.Down:
                    if (!Settings.EnableGravity)
                    {
                        _keyboardMoveSpeedY = KEY_MOVE_SPEED;
                    }                     
                    break;
                case Key.A:
                case Key.Left:
                    _keyboardMoveSpeedX = -KEY_MOVE_SPEED;
                    _gremlin.SpriteFlipTransform.ScaleX = -1;
                    break;
                case Key.D:
                case Key.Right:
                    _keyboardMoveSpeedX = KEY_MOVE_SPEED;
                    _gremlin.SpriteFlipTransform.ScaleX = 1;
                    break;
            }

            _isKeyboardMoving = (_keyboardMoveSpeedX != 0 || _keyboardMoveSpeedY != 0);

            if (_isKeyboardMoving && !wasMoving)
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Random");
                _keyboardMoveTimer.Start();
            }
        }
        private void HandleMovementKeyUp(Key key)
        {
            switch (key)
            {
                case Key.W:
                case Key.Up:
                    if (_keyboardMoveSpeedY < 0) _keyboardMoveSpeedY = 0;    
                    break;
                case Key.S:
                case Key.Down:
                    if (_keyboardMoveSpeedY > 0) _keyboardMoveSpeedY = 0;
                    break;
                case Key.A:
                case Key.Left:
                    if (_keyboardMoveSpeedX < 0) _keyboardMoveSpeedX = 0;
                    break;
                case Key.D:
                case Key.Right:
                    if (_keyboardMoveSpeedX > 0) _keyboardMoveSpeedX = 0;
                    break;
            }

            bool wasMoving = _isKeyboardMoving;
            _isKeyboardMoving = (_keyboardMoveSpeedX != 0 || _keyboardMoveSpeedY != 0);

            if (!_isKeyboardMoving && wasMoving)
            {
                _keyboardMoveTimer.Stop();
                _gremlinState.SetState("Idle");
            }
        }
        private void KeyboardMove_Tick(object sender, EventArgs e)
        {
            double newLeft = _gremlin.Left + _keyboardMoveSpeedX;
            double newTop = _gremlin.Top + _keyboardMoveSpeedY;
            newLeft = Math.Max(SystemParameters.WorkArea.Left,Math.Min(newLeft, SystemParameters.WorkArea.Right - _gremlin.SpriteImage.ActualWidth));
            newTop = Math.Max(SystemParameters.WorkArea.Top,Math.Min(newTop, SystemParameters.WorkArea.Bottom - _gremlin.SpriteImage.ActualHeight));
            _gremlin.Left = newLeft;
            _gremlin.Top = newTop;

            if (Math.Abs(_keyboardMoveSpeedX) > Math.Abs(_keyboardMoveSpeedY))
            {
                _currentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk",
                    _currentFrames.WalkRight, _frameCounts.WalkR, _gremlin.SpriteImage);
            }
        }
        public void StopAllMovement()
        {
            _keyboardMoveSpeedX = 0;
            _keyboardMoveSpeedY = 0;
            _isKeyboardMoving = false;
            _keyboardMoveTimer.Stop();

            if (!_gremlinState.GetState("Sleeping"))
            {
                _gremlinState.SetState("Idle");
            }
        }
        private void ShowKeyboardHelp()
        {
            //It looks ugly, might as well
            //just read the readme.txt about keyboard controls
            string helpText = @"KEYBOARD CONTROLS:

                MOVEMENT (Disabled in Combat Mode):
                    WASD / Arrow Keys - Move character
                    E - Toggle cursor following
                    R - Random movement
                    ESC - Stop all movement

                ACTIONS:
                    SPACE - Click animation
                    T - Toggle sleep/wake

                COMBAT MODE:
                    Q - Toggle combat mode
                    1 - Left emote/summon (combat only)
                    2 - Right emote/summon (combat only)

                HELP:
                    F1 - Show this help
                    X - Close Program";           
            MessageBox.Show(helpText, "Desktop Gremlin - Keyboard Controls",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}