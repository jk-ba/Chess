namespace Chess {
    public class IllegalMoveException : Exception {
        public IllegalMoveException(string message) : base(message) {}
    }
}