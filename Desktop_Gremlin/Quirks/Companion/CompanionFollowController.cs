using DesktopGremlin.Quirks.Companion;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public class CompanionFollowController
    {
        private Companion _companion;
        private AnimationStates _gremlinState;
        private CurrentFrames _currentFrames;
        private FrameCounts _frameCounts;
        private Image _spriteImage;
        private Window _mainGremlin;
        private DispatcherTimer _gravityTimer;

        private const int MoveDelayMs = 1000;
        private DateTime _lastStillTime = DateTime.Now;
        private bool _isMoving = false;

        public CompanionFollowController(Companion companion, AnimationStates gremlinState,CurrentFrames currentFrames, FrameCounts frameCounts, Image spriteImage)
        {
            _companion = companion;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;
            InitializeGravity();
        }
        private void InitializeGravity()
        {
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
            double bottomLimit = SystemParameters.WorkArea.Bottom - _spriteImage.ActualHeight;

            if (_gremlinState.GetState("Grab"))
            {
                return;
            }

            if (_companion.Top + 5 < bottomLimit)
            {
                _companion.Top += Settings.SvGravity;
            }
        }
        public void ToggleGravity()
        {
            if (Settings.EnableGravity)
            {
                _gravityTimer.Start();
            }
            else
            {
                _gravityTimer.Stop();
            }
        }   
        public void SetMainGremlin(Window mainGremlin)
        {
            _mainGremlin = mainGremlin;
        }

        public void FollowMainGremlin()
        {
            if (_mainGremlin == null)
            {
                return;
            }
            ToggleGravity();

            if (_gremlinState.GetState("Grab"))
            {
                return;
            }

            double halfW = _spriteImage.ActualWidth > 0 ? _spriteImage.ActualWidth / 2.0 : QuirkSettings.CompanionWidth / 2.0;
            double halfH = _spriteImage.ActualHeight > 0 ? _spriteImage.ActualHeight / 2.0 : QuirkSettings.CompanionHeight / 2.0;

            var compCenter = new Point(_companion.Left + halfW, _companion.Top + halfH);

            double mainHalfW = _mainGremlin.Width / 2.0;
            double mainHalfH = _mainGremlin.Height / 2.0;
            var mainCenter = new Point(_mainGremlin.Left + mainHalfW, _mainGremlin.Top + mainHalfH);

            double dx = mainCenter.X - compCenter.X;
            double dy = mainCenter.Y - compCenter.Y;

            if (Settings.EnableGravity)
            {
                dy = 0;
            }

            double distance = Math.Sqrt(dx * dx + dy * dy);

            double startMoveDistance = QuirkSettings.CompanionFollow + 20; 
            double stopMoveDistance = QuirkSettings.CompanionFollow - 10;

            double followDistance = distance - _spriteImage.Width / 2;

            if (!_isMoving)
            {
                if (followDistance > startMoveDistance)
                {
                    _isMoving = true;
                }
            }
            else
            {
                if (followDistance < stopMoveDistance)
                {
                    _isMoving = false;
                }
            }

            if (!_isMoving)
            {
                _lastStillTime = DateTime.Now;
                _gremlinState.SetState("Idle");
                return;
            }

            double nx = dx / distance;
            double ny = dy / distance;

            if (Settings.EnableGravity)
            {
                ny = 0;
            }

            double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);

            _companion.Left += nx * step;

            if (!Settings.EnableGravity)
            {
                _companion.Top += ny * step;
            }

            UpdateDirectionAnimation(nx, ny, Settings.EnableGravity);
        }

        private void UpdateDirectionAnimation(double nx, double ny, bool gravity)
        { 
            if (gravity)
            {
                ny = 0;
            }

            double angle = Math.Atan2(ny, nx) * 180.0 / Math.PI;
            if (angle < 0)
            {
                angle += 360;
            }

            if (gravity)
            {
                if (nx >= 0)
                {
                    _currentFrames.Right = SpriteManagerCompanion.PlayAnimation("runRight", "Run",
                        _currentFrames.Right, _frameCounts.Right, _spriteImage);
                }
                else
                {
                    _currentFrames.Left = SpriteManagerCompanion.PlayAnimation("runLeft", "Run",
                        _currentFrames.Left, _frameCounts.Left, _spriteImage);
                }
                return;
            }

            if (angle >= 337.5 || angle < 22.5)
            {
                _currentFrames.Right = SpriteManagerCompanion.PlayAnimation("runRight", "Run",
                    _currentFrames.Right, _frameCounts.Right, _spriteImage);
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                _currentFrames.DownRight = SpriteManagerCompanion.PlayAnimation("downRight", "Run",
                    _currentFrames.DownRight, _frameCounts.DownRight, _spriteImage);
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                _currentFrames.Down = SpriteManagerCompanion.PlayAnimation("runDown", "Run",
                    _currentFrames.Down, _frameCounts.Down, _spriteImage);
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                _currentFrames.DownLeft = SpriteManagerCompanion.PlayAnimation("downLeft", "Run",
                    _currentFrames.DownLeft, _frameCounts.DownLeft, _spriteImage);
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                _currentFrames.Left = SpriteManagerCompanion.PlayAnimation("runLeft", "Run",
                    _currentFrames.Left, _frameCounts.Left, _spriteImage);
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                _currentFrames.UpLeft = SpriteManagerCompanion.PlayAnimation("upLeft", "Run",
                    _currentFrames.UpLeft, _frameCounts.UpLeft, _spriteImage);
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                _currentFrames.Up = SpriteManagerCompanion.PlayAnimation("runUp", "Run",
                    _currentFrames.Up, _frameCounts.Up, _spriteImage);
            }
            else if (angle >= 292.5 && angle < 337.5)
            {
                _currentFrames.UpRight = SpriteManagerCompanion.PlayAnimation("upRight", "Run",
                    _currentFrames.UpRight, _frameCounts.UpRight, _spriteImage);
            }
        }
    }
}