using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

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
            Quirks.MediaManager.PlaySound("foodSpawn.wav", GetSelectedCharacter());
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
            double gx = _window.Position.X + _window.Width / 2;
            double gy = _window.Position.Y + _window.Height / 2;

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
            _window.Position = new PixelPoint(_window.Position.X + (int)Math.Round(dx / distance * step), _window.Position.Y + (int)Math.Round(dy / distance * step));

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
                _window.Position = new PixelPoint(_window.Position.X + (int)Math.Round(moveX), _window.Position.Y);

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
                _window.Position = new PixelPoint(_window.Position.X, _window.Position.Y + (int)Math.Round(moveY));

                string dir = dy > 0 ? "Down" : "Up";
                PlayDirectionalAnimation(dir);
            }
        }
        private void StopFollowing()
        {
            Quirks.MediaManager.PlaySound("eat.wav", "Misc");
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

        private string GetSelectedCharacter()
        {
            if (_window is MainWindow mainWindow) return mainWindow.GetSelectedCharacter();
            else return Settings.StartingChar;
        }

        private void PlayDirectionalAnimation(string dir)
        {
            switch (dir)
            {
                case "Right":
                    _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", _currentFrames.Right, _frameCounts.Right, _spriteImage, GetSelectedCharacter());
                    break;
                case "DownRight":
                    _currentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run", _currentFrames.DownRight, _frameCounts.DownRight, _spriteImage, GetSelectedCharacter());
                    break;
                case "Down":
                    _currentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run", _currentFrames.Down, _frameCounts.Down, _spriteImage, GetSelectedCharacter());
                    break;
                case "DownLeft":
                    _currentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run", _currentFrames.DownLeft, _frameCounts.DownLeft, _spriteImage, GetSelectedCharacter());
                    break;
                case "Left":
                    _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run", _currentFrames.Left, _frameCounts.Left, _spriteImage, GetSelectedCharacter());
                    break;
                case "UpLeft":
                    _currentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run", _currentFrames.UpLeft, _frameCounts.UpLeft, _spriteImage, GetSelectedCharacter());
                    break;
                case "Up":
                    _currentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run", _currentFrames.Up, _frameCounts.Up, _spriteImage, GetSelectedCharacter());
                    break;
                case "UpRight":
                    _currentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run", _currentFrames.UpRight, _frameCounts.UpRight, _spriteImage, GetSelectedCharacter());
                    break;
                default:
                    _currentFrames.WalkIdle = SpriteManager.PlayAnimation("runIdle", "Actions", _currentFrames.WalkIdle, _frameCounts.WalkIdle, _spriteImage, GetSelectedCharacter());
                    break;
            }
        }
    }
}