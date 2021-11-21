namespace Chess {
    public struct Piece {
        public PieceType Type { get; }
        public Color Color { get; }

        public Piece(PieceType type, Color color) {
            Type = type;
            Color = color;
        }

        private static char[,] Symbols = new char[,] {
            { '♙', '♘', '♗', '♖', '♔', '♕' },
            { '♟', '♞', '♝', '♜', '♚', '♛' }
        };

        public override string ToString() =>
            Symbols[(int)Color, (int)Type].ToString();

        public static implicit operator Piece((PieceType t, Color c) t) => new Piece(t.t, t.c);
    }
}