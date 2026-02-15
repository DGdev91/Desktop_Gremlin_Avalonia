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

        public bool UseStraightMovementOnly { get; set; } = true;
        private bool _movingHorizontalFirst = true;

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
            if (food == null)
            {
                return;
            }

            _currentFood = food;
            _currentSpeed = startingSpeed;
            MediaManager.PlaySound("foodSpawn.wav", Settings.StartingChar);
            _followTimer.Start();
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
                _currentFood.Close();
                StopFollowing();
                return;
            }

            _currentSpeed = Math.Min(_currentSpeed + QuirkSettings.ItemAcceleration, QuirkSettings.MaxItemAcceleration);
            double step = Math.Min(_currentSpeed, distance);

            if (Settings.StraightLine)
            {
                MoveStraightOnly(dx, dy, step);
            }
            else
            {
                MoveDiagonally(dx, dy, distance, step);
            }

            _gremlinState.SetState("Walking");
        }

        private void MoveDiagonally(double dx, double dy, double distance, double step)
        {
            _window.Left += (dx / distance) * step;
            _window.Top += (dy / distance) * step;

            string dir = GetDirectionFromAngle(dx, dy);
            PlayDirectionalAnimation(dir);
        }

        private void MoveStraightOnly(double dx, double dy, double step)
        {
            double absDx = Math.Abs(dx);
            double absDy = Math.Abs(dy);

            bool moveHorizontal = false;
            bool moveVertical = false;

            double alignmentThreshold = 5.0;

            if (absDx < alignmentThreshold)
            {
                moveVertical = true;
            }
            else if (absDy < alignmentThreshold)
            {
                moveHorizontal = true;
            }
            else
            {
                if (_movingHorizontalFirst)
                {
                    moveHorizontal = true;
                }
                else
                {
                    moveVertical = true;
                }
                if (moveHorizontal && absDx < step * 2)
                {
                    _movingHorizontalFirst = false;
                }
                else if (moveVertical && absDy < step * 2)
                {
                    _movingHorizontalFirst = true;
                }
            }
            if (moveHorizontal)
            {
                double moveX = dx > 0 ? step : -step;
                if (absDx < step)
                {
                    moveX = dx;
                }
                _window.Left += moveX;

                string dir = dx > 0 ? "Right" : "Left";
                PlayDirectionalAnimation(dir);
            }
            else if (moveVertical)
            {
                double moveY = dy > 0 ? step : -step;
                if (absDy < step)
                {
                    moveY = dy;
                }
                _window.Top += moveY;

                string dir = dy > 0 ? "Down" : "Up";
                PlayDirectionalAnimation(dir);
            }
        }
        private void StopFollowing()
        {
            MediaManager.PlaySound("eat.wav", "Misc");
            _followTimer.Stop();
            _gremlinState.UnlockState();
            _gremlinState.SetState("Sleeping");
        }

        private string GetDirectionFromAngle(double dx, double dy)
        {
            double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
            if (angle < 0)
            {
                angle += 360;
            }

            if (angle >= 337.5 || angle < 22.5)
            {
                return "Right";
            }
            if (angle < 67.5)
            {
                return "DownRight";
            }
            if (angle < 112.5)
            {
                return "Down";
            }
            if (angle < 157.5)
            {
                return "DownLeft";
            }
            if (angle < 202.5)
            {
                return "Left";
            }
            if (angle < 247.5)
            {
                return "UpLeft";
            }
            if (angle < 292.5)
            {
                return "Up";
            }
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