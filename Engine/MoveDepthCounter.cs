namespace Engine
{
    using System.IO;
    public static class MoveDepthCounter
    {
        public static FileStream? fs;
        public static StreamWriter? sw;
        public static int logDepth;
        public static void Open()
        {
            var dir = System.IO.Directory.GetParent(Environment.CurrentDirectory);
            if (dir is null) return;
            string path = Path.Combine(dir.FullName, "Debug\\perft.txt");
            File.Create(path).Close(); // Empty output file
            fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs);
        }
        public static void Close()
        {
            if (sw is null) return;
            sw.Close();
            if (fs is null) return;
            fs.Close();
        }
        public static int CountMoves(int depth, Board board)
        {
            return CountMoves(depth, new MoveGenerator(board.BoardData, board.CurrentTurn, board.PreviousMoves));
        }
        public static int CountMoves(int depth, MoveGenerator curMoves)
        {
            if (depth == 0) return 0; // end search
            if (depth == 1)
            {
                if (logDepth == 1)
                {
                    foreach (Move m in curMoves.PossibleMoves)
                    {
                        if (sw is null) throw new Exception("Depth Counter was not opened");
                        sw.WriteLine(Board.IndexToString(m.StartSquare) + Board.IndexToString(m.TargetSquare) + ": 1");
                    }
                }
                return curMoves.PossibleMoves.Count;
            }
            int count = 0;
            foreach (Move move in curMoves.PossibleMoves)
            {
                int depthCount = CountMoves(depth - 1, new MoveGenerator(curMoves.boardData, curMoves.curTurnColor, new List<Move>(curMoves.prevMoves), move));
                count += depthCount;
                if (logDepth > 1 && depth == logDepth)
                {
                    if (move.IsPromotion)
                    {
                        if (sw is null) throw new Exception("Depth Counter was not opened");
                        sw.WriteLine(Board.IndexToString(move.StartSquare) + Board.IndexToString(move.TargetSquare) + move.PromotionPieceType + ": " + depthCount);
                    }
                    else
                    {
                        if (sw is null) throw new Exception("Depth Counter was not opened");
                        sw.WriteLine(Board.IndexToString(move.StartSquare) + Board.IndexToString(move.TargetSquare) + ": " + depthCount);
                    }
                }
            }
            return count;
        }
    }
}