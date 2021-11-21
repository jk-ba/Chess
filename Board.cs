using static Chess.Color;
using static Chess.PieceType;
using static System.Linq.Enumerable;

namespace Chess {
    internal class Board {

        internal Dictionary<Pos, Piece?> Cells { get; }

        public Board() {
            var lineup = new[] {
                Rook, Knight, Bishop, King, Queen, Bishop, Knight, Rook
            };

            IEnumerable<(Pos, Piece?)> Row(int row, IEnumerable<Piece> pieces) =>
                pieces.Select((p, i) => (new Pos(row, i), new Piece?(p)));

            Cells = new[] {
                Row(0, lineup.Select(t => new Piece(t, White))),
                Row(1, Repeat(new Piece(Pawn, White), 8))
            }
            .Concat(
                Range(2, 4).Select(r => 
                    Range(0, 8).Select(c => 
                        (new Pos(r, c), (Piece?)null)))
            )
            .Concat(new IEnumerable<(Pos, Piece?)>[] {
                Row(6, Repeat(new Piece(Pawn, Black), 8)),
                Row(7, lineup.Select(t => new Piece(t, Black)))
            })
            .Aggregate((ps, p) => ps.Concat(p))
            .ToDictionary(x => x.Item1, x => x.Item2);
        }

        internal Board(Board previous, Move move) {
            Cells = new Dictionary<Pos, Piece?>(previous.Cells);
            Cells[move.To] = Cells[move.From];
            Cells[move.From] = null;
        }

        internal Board Move(Move move) {
            if (Cells[move.From] == null) {
                throw new IllegalMoveException("No piece to move in this cell.");
            }
            return new Board(this, move);
        }

        public void Print() {
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
        }
    }
}