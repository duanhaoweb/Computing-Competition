using UnityEngine;

namespace DefaultNamespace
{
    public static class ColorHelper
    {
        public static Color32 Gold = new(255, 187, 0, 255);
        public static Color32 Red = Color.red;
        public static Color32 Green = HexToColor("#84fab0");
        public static Color32 LightBlue = HexToColor("#8fd3f4");

        public static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}