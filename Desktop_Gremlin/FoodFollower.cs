using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DesktopGremlin
{
    public class FoodFollower
    {
        private readonly Window _window;
        private readonly AnimationStates _gremlinState;
        private readonly CurrentFrames _currentFrames;
        private readonly FrameCounts _frameCounts;
        private readonly Image _spriteImage;
        private readonly DispatcherTimer _followTimer;
        private Target _currentFood;
        private double _currentSpeed;

        public FoodFollower(Window window, AnimationStates gremlinState, CurrentFrames currentFrames, FrameCounts frameCounts, Image spriteImage)
        {
            _window = window;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;
            _followTimer = new DispatcherTimer();
            _followTimer.Tick += FollowTick;
            _followTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate);
        }
        public void StartFollowing(Target food, double startingSpeed)
        {
            if (food == null) return;

            _currentFood = food;
            _currentSpeed = startingSpeed;
            _followTimer.Start();
            _gremlinState.LockState();
            _gremlinState.SetState("FollowItem");
        }
        private void FollowTick(object sender, EventArgs e)
        {
            if (_currentFood == null || !_currentFood.IsVisible)
            {
                StopFollowing();
                return;
            }

            var foodCenter = _currentFood.GetCenter();
            double gx = _window.Left + _window.ActualWidth / 2;
            double gy = _window.Top + _window.ActualHeight / 2;

            double dx = foodCenter.X - gx;
            double dy = foodCenter.Y - gy;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance - 25 < Settings.SpriteSize / 2 || !_gremlinState.GetState("FollowItem"))
            {
                MediaManager.PlaySound("eat.wav", "Misc");
                _currentFood.Close();
                StopFollowing();
                return;
            }

            _currentSpeed = Math.Min(_currentSpeed + Quirks.ItemAcceleration, Quirks.MaxItemAcceleration);
            double step = Math.Min(_currentSpeed, distance);

            _window.Left += (dx / distance) * step;
            _window.Top += (dy / distance) * step;

            string dir = GetDirectionFromAngle(dx, dy);
            PlayDirectionalAnimation(dir);

            _gremlinState.SetState("Walking");
        }
        private void StopFollowing()
        {
            _followTimer.Stop();
            _gremlinState.UnlockState();
            _gremlinState.SetState("Sleeping");
        }
        private string GetDirectionFromAngle(double dx, double dy)
        {
            double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
            if (angle < 0) angle += 360;

            if (angle >= 337.5 || angle < 22.5) return "Right";
            if (angle < 67.5) return "DownRight";
            if (angle < 112.5) return "Down";
            if (angle < 157.5) return "DownLeft";
            if (angle < 202.5) return "Left";
            if (angle < 247.5) return "UpLeft";
            if (angle < 292.5) return "Up";
            return "UpRight";
        }

        private void PlayDirectionalAnimation(string dir)
        {
            switch (dir)
            {
                case "Right":
                    _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", _currentFrames.Right, _frameCounts.Right, _spriteImage);
                    break;
                case "DownRight":
                    _currentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run", _currentFrames.DownRight, _frameCounts.DownRight, _spriteImage);
                    break;
                case "Down":
                    _currentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run", _currentFrames.Down, _frameCounts.Down, _spriteImage);
                    break;
                case "DownLeft":
                    _currentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run", _currentFrames.DownLeft, _frameCounts.DownLeft, _spriteImage);
                    break;
                case "Left":
                    _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run", _currentFrames.Left, _frameCounts.Left, _spriteImage);
                    break;
                case "UpLeft":
                    _currentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run", _currentFrames.UpLeft, _frameCounts.UpLeft, _spriteImage);
                    break;
                case "Up":
                    _currentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run", _currentFrames.Up, _frameCounts.Up, _spriteImage);
                    break;
                case "UpRight":
                    _currentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run", _currentFrames.UpRight, _frameCounts.UpRight, _spriteImage);
                    break;
                default:
                    _currentFrames.WalkIdle = SpriteManager.PlayAnimation("runIdle", "Actions", _currentFrames.WalkIdle, _frameCounts.WalkIdle, _spriteImage);
                    break;
            }
        }
    }
}
