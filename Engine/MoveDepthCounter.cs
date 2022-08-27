namespace Engine
{
    public static class MoveDepthCounter
    {
        public static int CountMoves(int depth, Board prevBoard)
        {
            if (depth == 0) return 0; // end search
            if (depth == 1) return prevBoard.possibleMoves.Count;
            int count = 0;
            foreach (Move move in prevBoard.possibleMoves)
            {
                Board board = new(prevBoard);
                board.PlayerMove(move.StartSquare, move.TargetSquare, move.MoveFlag);
                count += CountMoves(depth - 1, board);
            }
            return count;
        }
    }
}