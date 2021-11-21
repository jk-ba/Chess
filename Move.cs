namespace Chess {
    internal struct Move {
        public Move(Pos from, Pos to) {
            From = from;
            To = to;
        }

        public Pos From { get; }
        public Pos To { get; }
    }
}