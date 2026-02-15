using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace DesktopGremlin
{
    public class HotspotController
    {
        private MainWindow _window;
        private Border _leftHotspot;
        private Border _rightHotspot;
        private Border _topHotspot;
        private Border _leftDownHotspot;
        private Border _rightDownHotspot;
        private bool _hotspotVisible = false;
        private bool _hotspotDisable = false;

        public HotspotController(MainWindow window, Border leftHotspot, Border rightHotspot,Border topHotspot, Border leftDownHotspot, Border rightDownHotspot)
        {
            _window = window;
            _leftHotspot = leftHotspot;
            _rightHotspot = rightHotspot;
            _topHotspot = topHotspot;
            _leftDownHotspot = leftDownHotspot;
            _rightDownHotspot = rightDownHotspot;
        }

        public void ToggleHotspots()
        {
            _hotspotDisable = !_hotspotDisable;
            _leftHotspot.IsEnabled = !_leftHotspot.IsEnabled;
            _rightHotspot.IsEnabled = !_rightHotspot.IsEnabled;
            _topHotspot.IsEnabled = !_topHotspot.IsEnabled;
            _leftDownHotspot.IsEnabled = !_leftDownHotspot.IsEnabled;
            _rightDownHotspot.IsEnabled = !_rightDownHotspot.IsEnabled;

            if (_hotspotVisible)
            {
                if (_hotspotDisable)
                {
                    _leftHotspot.Background = new SolidColorBrush(Colors.Red, 0.5);
                    _rightHotspot.Background = new SolidColorBrush(Colors.Blue, 0.5);
                    _topHotspot.Background = new SolidColorBrush(Colors.Purple, 0.5);
                    _leftDownHotspot.Background = new SolidColorBrush(Colors.Green, 0.5);
                    _rightDownHotspot.Background = new SolidColorBrush(Colors.Yellow, 0.5);
                }
                else
                {
                    _leftHotspot.Background = new SolidColorBrush(Colors.Red);
                    _rightHotspot.Background = new SolidColorBrush(Colors.Blue);
                    _topHotspot.Background = new SolidColorBrush(Colors.Purple);
                    _leftDownHotspot.Background = new SolidColorBrush(Colors.Green);
                    _rightDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
        }

        public void ShowHotspots()
        {
            _hotspotVisible = !_hotspotVisible;
            if (_hotspotVisible)
            {
                if (_hotspotDisable)
                {
                    _leftHotspot.Background = new SolidColorBrush(Colors.Red, 0.5);
                    _rightHotspot.Background = new SolidColorBrush(Colors.Blue, 0.5);
                    _topHotspot.Background = new SolidColorBrush(Colors.Purple, 0.5);
                    _leftDownHotspot.Background = new SolidColorBrush(Colors.Green, 0.5);
                    _rightDownHotspot.Background = new SolidColorBrush(Colors.Yellow, 0.5);
                }
                else
                {
                    _leftHotspot.Background = new SolidColorBrush(Colors.Red);
                    _rightHotspot.Background = new SolidColorBrush(Colors.Blue);
                    _topHotspot.Background = new SolidColorBrush(Colors.Purple);
                    _leftDownHotspot.Background = new SolidColorBrush(Colors.Green);
                    _rightDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                ImmutableSolidColorBrush noColor = (ImmutableSolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                _leftHotspot.Background = noColor;
                _rightHotspot.Background = noColor;
                _topHotspot.Background = noColor;
                _leftDownHotspot.Background = noColor;
                _rightDownHotspot.Background = noColor;
            }
        }
    }
}