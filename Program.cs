// See https://aka.ms/new-console-template for more information
using static System.Linq.Enumerable;
using static Chess.PieceType;
using static Chess.Color;
using System.Linq;

namespace Chess {
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