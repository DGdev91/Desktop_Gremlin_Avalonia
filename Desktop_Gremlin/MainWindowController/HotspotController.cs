using System.Windows.Controls;
using System.Windows.Media;

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

            if (_hotspotDisable)
            {
                _leftHotspot.Background = new SolidColorBrush(Colors.Transparent);
                _rightHotspot.Background = new SolidColorBrush(Colors.Transparent);
                _topHotspot.Background = new SolidColorBrush(Colors.Transparent);
                _leftDownHotspot.Background = new SolidColorBrush(Colors.Transparent);
                _rightDownHotspot.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                _leftHotspot.Background = noColor;
                _rightHotspot.Background = noColor;
                _topHotspot.Background = noColor;
                _leftDownHotspot.Background = noColor;
                _rightDownHotspot.Background = noColor;
            }
        }

        public void ShowHotspots()
        {
            _hotspotVisible = !_hotspotVisible;
            if (_hotspotVisible)
            {
                _leftHotspot.Background = new SolidColorBrush(Colors.Red);
                _rightHotspot.Background = new SolidColorBrush(Colors.Blue);
                _topHotspot.Background = new SolidColorBrush(Colors.Purple);
                _leftDownHotspot.Background = new SolidColorBrush(Colors.Green);
                _rightDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                _leftHotspot.Background = noColor;
                _rightHotspot.Background = noColor;
                _topHotspot.Background = noColor;
                _leftDownHotspot.Background = noColor;
                _rightDownHotspot.Background = noColor;
            }
        }
    }
}