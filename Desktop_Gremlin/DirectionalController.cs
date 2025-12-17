using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DesktopGremlin
{
    public static class DirectionalController
    {
        public static string GetDirectionFromAngle(double dx, double dy, bool gravity)
        {
            if (gravity)
            {
                dy = 0;
            }

            double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
            if (angle < 0) angle += 360;

            if (gravity)
            {
                return dx >= 0 ? "Right" : "Left";
            }

            if (angle >= 337.5 || angle < 22.5) return "Right";
            if (angle < 67.5) return "DownRight";
            if (angle < 112.5) return "Down";
            if (angle < 157.5) return "DownLeft";
            if (angle < 202.5) return "Left";
            if (angle < 247.5) return "UpLeft";
            if (angle < 292.5) return "Up";
            return "UpRight";
        }
    }
}
