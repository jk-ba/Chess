namespace Chess {
    public struct Move {
        public Move(Piece piece, Pos from, Pos to, Piece? capturing) {
            Piece = piece;
            From = from;
            To = to;
            Capturing = capturing;
        }

        public Piece Piece { get; }
        public Pos From { get; }
        public Pos To { get; }
        public Piece? Capturing { get; }
    }
}