namespace Chess {
    public struct Pos {
        public int r, c;

        public Pos(int r, int c) {
            this.r = r;
            this.c = c;
        }

        public static Pos Parse(string text) {
            text = text.Trim();
            if (text.Length != 2 || text[0] < 'a' || text[0] > 'h' || text[1] < '1' || text[1] > '8') {
                throw new FormatException("Invalid position format.");
            }
            return new Pos(text[1] - '1', text[0] - 'a');
        }

        public static implicit operator Pos((int r, int c) p) => new Pos(p.r, p.c);
    }
}