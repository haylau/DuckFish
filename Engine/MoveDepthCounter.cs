namespace Engine
{
    public static class MoveDepthCounter
    {
        public static int CountMoves(int depth, Board prevBoard)
        {
            if (depth == 0) return 0; // end search
            if (depth == 1)
            {
                // foreach (Move m in prevBoard.possibleMoves)
                // {
                //     Console.WriteLine(prevBoard.IndexToString(m.StartSquare) + prevBoard.IndexToString(m.TargetSquare) + ": 1");
                // }
                return prevBoard.possibleMoves.Count;
            }
            int count = 0;
            foreach (Move move in prevBoard.possibleMoves)
            {
                Board board = new(prevBoard);
                board.PlayerMove(move.StartSquare, move.TargetSquare, move.MoveFlag);
                int depthCount = CountMoves(depth - 1, board);
                count += depthCount;
                // if (depth == 2)
                // {
                //     if (move.IsPromotion)
                //     {
                //         Console.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + move.PromotionPieceType + ": " + depthCount);
                //     }
                //     else Console.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + ": " + depthCount);
                // }
            }
            return count;
        }
    }
}