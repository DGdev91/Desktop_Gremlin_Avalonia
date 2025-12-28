using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public class TimerController
    {
        private MainWindow _window;
        private AnimationStates _gremlinState;
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _gravityTimer;
        private Image _spriteImage;

        public TimerController(MainWindow window, AnimationStates gremlinState)
        {
            _window = window;
            _gremlinState = gremlinState;
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
            _gravityTimer.Interval = TimeSpan.FromMilliseconds(30);
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
            if (_gremlinState.GetState("Sleeping"))
            {
                return;
            }
            else
            {
                _gremlinState.UnlockState();
                MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                _gremlinState.SetState("Sleeping");
                _gremlinState.LockState();
            }
        }

        private void Gravity_Tick(object sender, EventArgs e)
        {

            double bottomLimit = SystemParameters.WorkArea.Bottom - _window.SpriteImage.ActualHeight;

            if (_gremlinState.GetState("Grab"))
            {
                return;
            }

            if (_window.Top + 5 < bottomLimit)
            {
                _window.Top += Settings.SvGravity;
            }
        }
    }
}
