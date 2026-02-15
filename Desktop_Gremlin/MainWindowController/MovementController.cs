using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace DesktopGremlin
{
    public class MovementController
    {
        private MainWindow _window;
        private AnimationStates _gremlinState;
        private CurrentFrames _currentFrames;
        private FrameCounts _frameCounts;
        private Image _spriteImage;
        private Random _rng;
        private DispatcherTimer _activeRandomMoveTimer;

        public MovementController(MainWindow window, AnimationStates gremlinState, CurrentFrames currentFrames,
            FrameCounts frameCounts, Image spriteImage, Random rng)
        {
            _window = window;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;
            _rng = rng;
        }

        public void RandomMove()
        {
            StopRandomMove();
            _gremlinState.SetState("Random");

            double moveX = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;
            double moveY = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;

            PixelRect workingArea = _window.GetCombinedScreens();
            double targetLeft = Math.Max(workingArea.X, Math.Min(_window.Position.X + moveX, workingArea.Right - _spriteImage.Bounds.Width));
            double targetTop = Math.Max(workingArea.Y, Math.Min(_window.Position.Y + moveY, workingArea.Bottom - _spriteImage.Bounds.Height));

            int step = Settings.WalkDistance;
            double dx = (targetLeft - _window.Position.X) / step;
            double dy = (targetTop - _window.Position.Y) / step;

            DispatcherTimer moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _activeRandomMoveTimer = moveTimer;

            int moveCount = 0;

            moveTimer.Tick += (s, e) =>
            {
                _window.Position = new PixelPoint((int)(_window.Position.X + Math.Round(dx)), (int)(_window.Position.Y + Math.Round(dy)));
                moveCount++;

                if (Settings.EnableGravity)
                {
                    dy = 0;
                }

                PlayWalkAnimation(dx, dy);

                if (moveCount >= step || !_gremlinState.GetState("Random"))
                {
                    moveTimer.Stop();
                    _gremlinState.SetState("Idle");
                    _activeRandomMoveTimer = null;
                }
            };
            moveTimer.Start();
        }

        public void StopRandomMove()
        {
            if (_activeRandomMoveTimer != null)
            {
                _activeRandomMoveTimer.Stop();
                _activeRandomMoveTimer = null;
            }
        }

        private void PlayWalkAnimation(double dx, double dy)
        {
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0)
                {
                    _window.RenderTransform = new ScaleTransform(1.0, 1.0);
                    _currentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk",
                        _currentFrames.WalkRight, _frameCounts.WalkRight, _spriteImage, _window.GetSelectedCharacter());
                }
                else
                {
                    if (Settings.MirrorXSprite)
                    {
                        _window.RenderTransform = new ScaleTransform(-1.0, 1.0);
                        _currentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk",
                            _currentFrames.WalkRight, _frameCounts.WalkRight, _spriteImage, _window.GetSelectedCharacter());
                    }
                    else
                    {
                        _currentFrames.WalkLeft = SpriteManager.PlayAnimation("walkLeft", "Walk",
                            _currentFrames.WalkLeft, _frameCounts.WalkLeft, _spriteImage, _window.GetSelectedCharacter());
                    }
                }
            }
            else
            {
                if (dy > 0)
                {
                    _currentFrames.WalkDown = SpriteManager.PlayAnimation("walkDown", "Walk",
                        _currentFrames.WalkDown, _frameCounts.WalkDown, _spriteImage, _window.GetSelectedCharacter());
                }
                else
                {
                    _currentFrames.WalkUp = SpriteManager.PlayAnimation("walkUp", "Walk",
                        _currentFrames.WalkUp, _frameCounts.WalkUp, _spriteImage, _window.GetSelectedCharacter());
                }
            }
        }
    }
}