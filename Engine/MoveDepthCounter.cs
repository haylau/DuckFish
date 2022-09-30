namespace Engine
{
    using System.IO;
    public static class MoveDepthCounter
    {
        public static FileStream? fs;
        public static StreamWriter? sw;
        public static int logDepth;
        public static void Open() {   
            var dir = System.IO.Directory.GetParent(Environment.CurrentDirectory);
            if (dir is null) return;
            string path = Path.Combine(dir.FullName, "Debug\\perft.txt");
            File.Create(path).Close(); // Empty output file
            fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs);
        }
        public static void Close() {
            if(sw is null) return;
            sw.Close();
            if(fs is null) return;
            fs.Close();
        }
        public static int CountMoves(int depth, Board prevBoard)
        {
            if (depth == 0) return 0; // end search
            if (depth == 1)
            {
                if (logDepth == 1)
                {
                    foreach (Move m in prevBoard.possibleMoves)
                    {
                        if(sw is null) throw new Exception("Depth Counter was not opened");
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
                        if(sw is null) throw new Exception("Depth Counter was not opened");
                        sw.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + move.PromotionPieceType + ": " + depthCount);
                    }
                    else 
                    {
                        if(sw is null) throw new Exception("Depth Counter was not opened");
                        sw.WriteLine(board.IndexToString(move.StartSquare) + board.IndexToString(move.TargetSquare) + ": " + depthCount);
                    } 
                }
            }
            return count;
        }
    }
}