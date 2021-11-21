namespace Chess {
    public class Piece {
        public PieceType Type { get; }
        public Color Color { get; }

        public bool FirstMove { get; }

        public Piece(PieceType type, Color color) {
            Type = type;
            Color = color;
            FirstMove = true;
        }

        public override bool Equals(object? obj) {
            var rhs = obj as Piece;
            if (rhs == null) {
                return false;
            }
            return Type == rhs.Type && Color == rhs.Color;
        }

        private static char[,] Symbols = new char[,] {
            { '♙', '♘', '♗', '♖', '♔', '♕' },
            { '♟', '♞', '♝', '♜', '♚', '♛' }
        };

        public override string ToString() =>
            Symbols[(int)Color, (int)Type].ToString();

        public static implicit operator Piece((PieceType t, Color c) t) => new Piece(t.t, t.c);

        public override int GetHashCode() => (Type, Color).GetHashCode();
    }
}