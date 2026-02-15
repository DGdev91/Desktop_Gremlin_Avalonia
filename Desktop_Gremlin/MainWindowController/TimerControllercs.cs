using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace DesktopGremlin
{
    public class TimerController
    {
        private MainWindow _window;
        private AnimationStates _gremlinState;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _gravityTimer;
        private Image _spriteImage;
        public TimerController(MainWindow window, AnimationStates gremlinState, Image spriteImage)
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
                Quirks.MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                _gremlinState.SetState("Sleeping");
                _gremlinState.LockState();
            }
        }
        private void Gravity_Tick(object sender, EventArgs e)
        {
            Screen screen = TopLevel.GetTopLevel(_window)?.Screens.ScreenFromVisual(_window);
            if (screen != null)
            {
                double bottomLimit;
                bottomLimit = screen?.WorkingArea.Bottom ?? 0;
                bottomLimit -= _spriteImage.Bounds.Height;
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
