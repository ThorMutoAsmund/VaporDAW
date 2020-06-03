using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;

namespace VaporDAW
{
    public static class Colors
    {
        public static Color Part = Color.FromArgb(160, 220, 220, 160);
        public static Color RefPart = Color.FromArgb(160, 220, 160, 220);
        public static Color PartSelected = Color.FromArgb(160, 250, 150, 80);

        public static Color PartBorder = Color.FromRgb(64, 64, 64);
        public static Color PartHandle = Color.FromArgb(0, 10, 0, 0);

        public static Color Track = Color.FromRgb(32, 36, 36);
        public static Color TrackSelected = Color.FromRgb(64, 72, 72);
        public static Color InaudibleTrack = Color.FromRgb(52, 52, 56);
        public static Color InaudibleTrackSelected = Color.FromRgb(84, 84, 92);
        public static Color TrackBorder = Color.FromRgb(64, 64, 64);

        public static Color TrackHead = Color.FromRgb(60, 60, 60);
        public static Color TrackHeadSelected = Color.FromRgb(112, 122, 122);
        public static Color TrackHeadBorder = Color.FromRgb(20, 20, 20);

        public static Color TimeRuler = Color.FromRgb(74, 82, 82);
    }
}
