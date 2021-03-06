namespace Chess {
    internal static class MoveExtensions {
        internal static Piece Piece(this Move move, Board board) => board.Cells[move.From]!.Value;

        internal static Piece? Captured(this Move move, Board board) => board.Cells[move.To];
    }
}