using static Chess.Color;
using static Chess.PieceType;
using static System.Linq.Enumerable;

namespace Chess {
    public class Game {
        private Board Board { get; set; }
        private List<Move> Moves { get; }
        public Color PlayerColor => 
            Moves.Count == 0 ? 
                White : 
                Board.Cells[Moves.Last().To]!.Value.Color.Inverted();

        public Game() {
            Board = new Board();
            Moves = new List<Move>();
        }

        private static bool IsCheck(Board board, Color color) {
            var otherColor = color.Inverted();
            var kingPos = board.Cells.Keys
                .Where(p => board.Cells[p]?.Equals(new Piece(King, color)) ?? false)
                .First();
            return board.Cells.Keys
                .Where(p => IsValidMove(board, otherColor, p, kingPos))
                .Any();
        }

        public bool Check => IsCheck(Board, PlayerColor);

        private bool IsCheckMate(Color color) =>
            IsCheck(Board, color) && 
            AllValidMoves(Board, color)
                .Select(m => new Board(Board, m))
                .All(b => IsCheck(Board, color));
        
        public bool CheckMate => IsCheckMate(PlayerColor);

        public void Move(Pos from, Pos to) {
            if (!IsValidMove(Board, PlayerColor, from, to)) {
                throw new IllegalMoveException("Illegal move.");
            }
            var move = new Move(from, to);
            var board = Board.Move(move);
            if (IsCheck(board, PlayerColor)) {
                throw new IllegalMoveException("Not allowed move, since would result in check.");
            }
            Board = board;
            Moves.Add(move);
        }

        private static IEnumerable<Move> AllValidMoves(Board board, Color color) =>
            board.Cells.Keys
                .Where(p => board.Cells[p]?.Color == color)
                .SelectMany(from => 
                    board.Cells.Keys
                        .Where(to => 
                            IsValidMove(board, color, from, to))
                        .Select(to => new Move(from, to)));

        private static bool IsValidMove(Board board, Color color, Pos from, Pos to) {
            bool IsInside(Pos pos) =>
                pos.r >= 0 && pos.r < 8 && pos.c >= 0 && pos.c < 8;

            if (!IsInside(from) || !IsInside(to)) {
                return false;
            }

            if (board.Cells[from] == null) {
                return false;
            }

            var piece = board.Cells[from]!.Value;

            if (piece.Color != color) {
                return false;
            }

            var target = board.Cells[to];

            if (target?.Color == color) {
                return false;
            }

            int vDist = Math.Abs(from.r - to.r);
            int hDist = Math.Abs(from.c - to.c);

            if (vDist == 0 && hDist == 0) {
                return false;
            }

            if (piece.Type == Knight) {
                return ((vDist == 2 && hDist == 1)
                        || (vDist == 1 && hDist == 2));
            }

            if (!(vDist == hDist || vDist == 0 || hDist == 0)) {
                return false;
            }

            int vDir = vDist == 0 ? 0 : (to.r - from.r) / vDist;
            int hDir = hDist == 0 ? 0 : (to.c - from.c) / hDist;

            var diagonalMove = vDist > 0 && hDist > 0;

            var lineOfSight = new List<Pos>();
            Pos pos = (from.r + vDir, from.c + hDir);
            while (!pos.Equals(to)) {
                lineOfSight.Add(pos);
                pos = (pos.r + vDir, pos.c + hDir);
            }

            if (!lineOfSight.All(p => board.Cells[p] == null)) {
                return false;
            }

            switch (piece.Type) {
                case Pawn:
                    if ((piece.Color == White && vDir != 1) ||
                        (piece.Color == Black && vDir != -1)) {
                        return false;
                    }
                    if (vDist == 1 && hDist == 1) {
                        return !(target?.Color == color);
                    }

                    if (vDist == 2 && hDist == 0) {
                        return (piece.Color == White && from.r == 1) ||
                            (piece.Color == Black && from.r == 6);
                    }

                    return vDist == 1 && hDist == 0;
                case Bishop:
                    return diagonalMove;
                case Rook:
                    return !diagonalMove;
                case Queen:
                    return true;
                case King:
                    return Math.Max(vDist, hDist) <= 1;
            }
            return false;
        }
        
        public void Print() {
            Board.Print();
            if (CheckMate) {
                Console.WriteLine($"{PlayerColor} is checkmate.");
            } else if (Check) {
                Console.WriteLine($"{PlayerColor} is checked.");
            }
        }
    }
}