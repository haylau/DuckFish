namespace Engine
{
    using System.IO;
    public static class MoveDepthCounter
    {
        private static string dir = System.IO.Directory.GetParent(Environment.CurrentDirectory).FullName;
        private static string path = Path.Combine(dir, "Debug\\perft.txt");
        public static FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
        public static StreamWriter sw = new StreamWriter(fs);
        public static int logDepth;
        public static int CountMoves(int depth, Board prevBoard)
        {
            if (depth == 0) return 0; // end search
            if (depth == 1)
            {
                if (logDepth == 1)
                {
                    foreach (Move m in prevBoard.possibleMoves)
                    {
                        sw.WriteLine(prevBoard.IndexToString(m.StartSquare) + prevBoard.IndexToString(m.TargetSquare) + ": 1");
                    }
                }
                return prevBoard.possibleMoves.Count;
            }
            int count = 0;
            foreach (Move move in prevBoard.possibleMoves)
            {
                Board board = new(prevBoard);
                board.PlayerMove(move.StartSquare, move.TargetSquare, move.MoveFlag);
                int depthCount = CountMoves(depth - 1, board);
                count += depthCount;
                if (logDepth > 1 && depth == logDepth)
                {
                    if (move.IsPromotion)
                    {
                        sw.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + move.PromotionPieceType + ": " + depthCount);
                    }
                    else sw.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + ": " + depthCount);
                }
            }
            return count;
        }
    }
}