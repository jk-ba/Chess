using static Chess.Color;
using static Chess.PieceType;
using static System.Linq.Enumerable;

namespace Chess {
    public class Board {
        private List<Move> Moves { get; }

        public Color PlayerColor => Moves.Count == 0 ? White : Cells[Moves.Last().To]!.Color.Inverted();
        public Dictionary<Pos, Piece?> Cells { get; }

        public Board() {
            Moves = new List<Move>();

            var lineup = new[] {
                Rook, Knight, Bishop, King, Queen, Bishop, Knight, Rook
            };

            IEnumerable<(Pos, Piece?)> Row(int row, IEnumerable<Piece?> pieces) =>
                pieces.Select((p, i) => (new Pos(row, i), p));

            Cells = new[] {
                Row(0, lineup.Select(t => new Piece(t, White))),
                Row(1, Repeat(new Piece(Pawn, White), 8))
            }
            .Concat(
                Range(2, 4).Select(r => Row(r, Repeat((Piece?)null, 8)))
            )
            .Concat(new IEnumerable<(Pos, Piece?)>[] {
                Row(6, Repeat(new Piece(Pawn, Black), 8)),
                Row(7, lineup.Select(t => new Piece(t, Black)))
            })
            .Aggregate((ps, p) => ps.Concat(p))
            .ToDictionary(x => x.Item1, x => x.Item2);
        }

        private Board(Board previous, Move move) {
            Cells = new Dictionary<Pos, Piece?>(previous.Cells);
            Moves = previous.Moves.Append(move).ToList();
            Cells[move.To] = Cells[move.From];
            Cells[move.From] = null;
        }

        private bool IsCheck(Color color) {
            var otherColor = color.Inverted();
            var kingPos = Cells.Keys
                .Where(p => Cells[p]?.Equals(new Piece(King, PlayerColor)) ?? false)
                .First();
            return Cells.Keys
                .Where(p => IsValidMove(otherColor, p, kingPos))
                .Any();
        }

        public bool Check => IsCheck(PlayerColor);

        private bool IsCheckMate(Color color) =>
            IsCheck(color) && 
            AllValidMoves(color)
                .Select(m => UncheckedMove(m))
                .All(b => b.IsCheck(color));
        
        public bool CheckMate => IsCheckMate(PlayerColor);

        private Board UncheckedMove(Move move) => new Board(this, move);

        public Board Move(Pos from, Pos to) {
            if (!IsValidMove(PlayerColor, from, to)) {
                throw new IllegalMoveException("Illegal move.");
            }
            var move = new Move(from, to);
            var board = UncheckedMove(move);
            if (board.IsCheck(PlayerColor)) {
                throw new IllegalMoveException("Not allowed move, since would result in check.");
            }
            return board;
        }

        private IEnumerable<Move> AllValidMoves(Color color) =>
            Cells.Keys
                .Where(p => Cells[p]?.Color == color)
                .SelectMany(from => 
                    Cells.Keys
                        .Where(to => 
                            IsValidMove(color, from, to))
                        .Select(to => new Move(from, to)));

        private bool IsValidMove(Color color, Pos from, Pos to) {
            bool IsInside(Pos pos) =>
                pos.r >= 0 && pos.r < 8 && pos.c >= 0 && pos.c < 8;

            if (!IsInside(from) || !IsInside(to)) {
                return false;
            }

            var piece = Cells[from];

            if (piece == null) {
                return false;
            }

            if (piece.Color != color) {
                return false;
            }

            var target = Cells[to];

            if (target != null && target.Color == color) {
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

            if (!lineOfSight.All(p => Cells[p] == null)) {
                return false;
            }

            switch (piece.Type) {
                case Pawn:
                    if ((piece.Color == White && vDir != 1) ||
                        (piece.Color == Black && vDir != -1)) {
                        return false;
                    }
                    if (vDist == 1 && hDist == 1) {
                        return !(target == null || target.Color == color);
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

        public void PrintBoard() {
            var netural = Console.BackgroundColor;
            for (int r = 7; r >= 0; --r) {
                Console.BackgroundColor = netural;
                Console.Write($"{r + 1} ");
                for (int c = 0; c < 8; ++c) {
                    Console.BackgroundColor = ((r + c) % 2) == 1 ?
                        ConsoleColor.Black : ConsoleColor.DarkGray;
                    Console.Write(" " + (Cells[(r, c)]?.ToString() ?? " ") + " ");
                }
                Console.BackgroundColor = netural;
                Console.WriteLine();
            }
            Console.BackgroundColor = netural;
            Console.WriteLine("   a  b  c  d  e  f  g  h");
            if (IsCheckMate(PlayerColor)) {
                Console.WriteLine($"{PlayerColor} is checkmate.");
            } else if (IsCheck(PlayerColor)) {
                Console.WriteLine($"{PlayerColor} is checked.");
            }
        }
    }
}