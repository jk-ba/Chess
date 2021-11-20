// See https://aka.ms/new-console-template for more information
using static System.Linq.Enumerable;
using static Chess.PieceType;
using static Chess.Color;
using System.Linq;

namespace Chess {
    public enum PieceType {
        Pawn, Knight, Bishop, Rook, Queen, King
    }

    public enum Color {
        White, Black
    }

    public static class ColorExtensions {
        public static Color Invert(this Color color) => color == White ? Black : White;
    }

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
    }

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

    public class IllegalMoveException : Exception {
        public IllegalMoveException(string message) : base(message) {}
    }

    public class Board {
        private List<Move> Moves { get; }

        public Color PlayerColor => Moves.Count == 0 ? White : Cells[Moves.Last().To]!.Color.Invert();
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
            var otherColor = color.Invert();
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
            var move = new Move(Cells[from]!, from, to, Cells[to]);
            var board = UncheckedMove(move);
            if (IsCheck(PlayerColor) && board.IsCheck(PlayerColor)) {
                throw new IllegalMoveException("Invalid move. Player still checked.");
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
                        .Select(to => 
                            new Move(
                                    Cells[from]!,
                                    from,
                                    to,
                                    Cells[to])));

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
                        return piece.FirstMove;
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

    public class Player {
        public Player(string name, Color color) {
            Name = name;
            Color = color;
        }
        public string Name { get; }
        public Color Color { get; }
    }

    public static class Program {
        public static void Main(string[] args) {
            var board = new Board();
            while(!board.CheckMate) {
                board.PrintBoard();
                Pos from, to;
                try { 
                    Console.Write($"{board.PlayerColor} move from: ");
                    var input = Console.ReadLine();
                    if (input == null) {
                        return;
                    }
                    from = Pos.Parse(input);
                    Console.Write($"Move to: ");
                    input = Console.ReadLine();
                    if (input == null) {
                        return;
                    }
                    to = Pos.Parse(input);
                    board = board.Move(from, to);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
    }
}