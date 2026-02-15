using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using DesktopGremlin.Quirks.Companion;
using System;

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

        public CompanionFollowController(Companion companion, AnimationStates gremlinState, CurrentFrames currentFrames, FrameCounts frameCounts, Image spriteImage)
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
            Screen screen = TopLevel.GetTopLevel(_companion)?.Screens?.ScreenFromVisual(_companion);
            if (screen != null)
            {
                double bottomLimit;
                bottomLimit = screen?.WorkingArea.Bottom ?? 0;
                bottomLimit -= _spriteImage.Bounds.Height;
                if (_gremlinState.GetState("Grab"))
                {
                    return;
                }
                if (_companion.Position.Y < bottomLimit && _companion.Position.Y > 0)
                {
                    _companion.Position = new PixelPoint(
                        _companion.Position.X,
                        (int)(_companion.Position.Y + Math.Round(Settings.SvGravity))
                    );
                }
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

            double halfW = _spriteImage.Bounds.Width > 0 ? _spriteImage.Bounds.Width / 2.0 : Settings.FrameWidth / 2.0;
            double halfH = _spriteImage.Bounds.Height > 0 ? _spriteImage.Bounds.Height / 2.0 : Settings.FrameHeight / 2.0;

            var compCenter = new Point(_companion.Position.X + halfW, _companion.Position.Y + halfH);

            double mainHalfW = _mainGremlin.Width / 2.0;
            double mainHalfH = _mainGremlin.Height / 2.0;
            var mainCenter = new Point(_mainGremlin.Position.X + mainHalfW, _mainGremlin.Position.Y + mainHalfH);

            double dx = mainCenter.X - compCenter.X;
            double dy = mainCenter.Y - compCenter.Y;

            if (Settings.EnableGravity)
            {
                dy = 0;
            }

            double centerToCenterDistance = Math.Sqrt(dx * dx + dy * dy);

            if (centerToCenterDistance < 1.0)
            {
                _gremlinState.SetState("Idle");
                return;
            }

            double mainGremlinRadius = Math.Max(_mainGremlin.Width, _mainGremlin.Height) / 2.0;
            double companionRadius = Math.Max(_spriteImage.Bounds.Width, _spriteImage.Bounds.Height) / 2.0;

            double edgeToEdgeDistance = centerToCenterDistance - mainGremlinRadius - companionRadius;

            double desiredEdgeDistance = QuirkSettings.CompanionFollow;
            double startMoveDistance = desiredEdgeDistance + 20;
            double stopMoveDistance = desiredEdgeDistance + 5;

            if (!_isMoving)
            {
                if (edgeToEdgeDistance > startMoveDistance)
                {
                    _isMoving = true;
                }
            }
            else
            {
                if (edgeToEdgeDistance <= stopMoveDistance)
                {
                    _isMoving = false;
                    _gremlinState.SetState("Idle");
                    return;
                }
            }

            if (!_isMoving)
            {
                _lastStillTime = DateTime.Now;
                _gremlinState.SetState("Idle");
                return;
            }

            double nx = dx / centerToCenterDistance;
            double ny = dy / centerToCenterDistance;

            if (Settings.EnableGravity)
            {
                ny = 0;
            }

            double excessDistance = edgeToEdgeDistance - desiredEdgeDistance;
            double step = Math.Min(MouseSettings.Speed, excessDistance);

            if (step > 0)
            {
                double moveX = nx * step;
                double moveY = 0.0;

                if (!Settings.EnableGravity)
                {
                    moveY = ny * step;
                }

                _companion.Position = new PixelPoint((int)(_companion.Position.X + Math.Round(moveX)), (int)(_companion.Position.Y + Math.Round(moveY)));

                UpdateDirectionAnimation(nx, ny, Settings.EnableGravity);
            }
            else
            {
                _gremlinState.SetState("Idle");
            }
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
                    _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run",
                        _currentFrames.Right, _frameCounts.Right, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
                }
                else
                {
                    _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run",
                        _currentFrames.Left, _frameCounts.Left, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
                }
                return;
            }

            if (angle >= 337.5 || angle < 22.5)
            {
                _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run",
                    _currentFrames.Right, _frameCounts.Right, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                _currentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run",
                    _currentFrames.DownRight, _frameCounts.DownRight, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                _currentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run",
                    _currentFrames.Down, _frameCounts.Down, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                _currentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run",
                    _currentFrames.DownLeft, _frameCounts.DownLeft, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run",
                    _currentFrames.Left, _frameCounts.Left, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                _currentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run",
                    _currentFrames.UpLeft, _frameCounts.UpLeft, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                _currentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run",
                    _currentFrames.Up, _frameCounts.Up, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
            else if (angle >= 292.5 && angle < 337.5)
            {
                _currentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run",
                    _currentFrames.UpRight, _frameCounts.UpRight, _spriteImage, QuirkSettings.CompanionChar, false, SpriteManager.CharacterType.Companion);
            }
        }
    }
}