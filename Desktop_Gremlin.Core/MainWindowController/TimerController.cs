using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace DesktopGremlin
{
    public class TimerController
    {
        private IMainPetWindow _window;
        private AnimationStates _gremlinState;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _gravityTimer;
        private Image _spriteImage;
        public TimerController(IMainPetWindow window, AnimationStates gremlinState, Image spriteImage)
        {
            _window = window;
            _gremlinState = gremlinState;
            _spriteImage = spriteImage;
            InitializeTimers();
        }
        public void Start()
        {
            _idleTimer.Start();
            if (Settings.EnableGravity)
            {
                _gravityTimer.Start();
            }
        }
        private void InitializeTimers()
        {
            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(Settings.SleepTime);
            _idleTimer.Tick += IdleTimer_Tick;

            _gravityTimer = new DispatcherTimer();
            _gravityTimer.Interval = TimeSpan.FromMilliseconds(10);
            _gravityTimer.Tick += Gravity_Tick;
        }
        public void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
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
        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (_window.IsCombat)
            {
                ResetIdleTimer();
                return;
            }
            if (_gremlinState.GetState("Sleeping"))
            {
                return;
            }
            else
            {
                _gremlinState.UnlockState();
                Quirks.MediaManager.PlaySound("sleep.wav", _window.GetSelectedCharacter());
                _gremlinState.SetState("Sleeping");
                _gremlinState.LockState();
            }
        }
        private void Gravity_Tick(object sender, EventArgs e)
        {
            PixelRect? screenArea = _window.GetCurrentScreenWorkingArea();
            if (screenArea != null)
            {
                double bottomLimit;
                bottomLimit = screenArea.Value.Bottom;
                // _window.Height (not _spriteImage.Bounds.Height) - Bounds only reflects a real
                // Avalonia layout pass, which never runs for Android's off-screen SpriteImage (its
                // rendering is native), leaving Bounds.Height at 0 there and letting the character
                // fall past the bottom of the screen before this limit kicks in.
                bottomLimit -= _window.Height;
                if (_gremlinState.GetState("Grab"))
                {
                    return;
                }
                if (_window.Position.Y < bottomLimit && _window.Position.Y > 0)
                {
                    _window.Position = new PixelPoint(_window.Position.X, (int)(_window.Position.Y + Math.Round(Settings.SvGravity)));
                }
            }
        }
    }
}
