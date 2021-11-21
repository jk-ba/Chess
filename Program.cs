// See https://aka.ms/new-console-template for more information
using static System.Linq.Enumerable;
using static Chess.PieceType;
using static Chess.Color;
using System.Linq;
using System.IO;

namespace Chess {
    public static class Program {
        public static Pos ReadPos(string prompt) {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input == null) {
                throw new EndOfStreamException();
            }
            return Pos.Parse(input);
        }

        public static void Main(string[] args) {
            var board = new Board();
            while(!board.CheckMate) {
                board.PrintBoard();
                try { 
                    board = board.Move(
                        ReadPos(prompt: $"{board.Player} move from: "), 
                        ReadPos(prompt: "Move to: "));
                }
                catch (EndOfStreamException) {
                    return;
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
    }
}