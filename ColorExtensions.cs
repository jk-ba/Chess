using static Chess.Color;

namespace Chess {
    public static class ColorExtensions {
        public static Color Inverted(this Color color) => color == White ? Black : White;
    }
}