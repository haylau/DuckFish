
namespace Engine
{
    using static MoveData;
    public static class MoveEvaluator
    {
        public const int whiteCheckmate = int.MaxValue;
        public const int blackCheckmate = int.MinValue;
        public static readonly Dictionary<int, int> pieceValue = new()
        {
            {Piece.Pawn, 1},
            {Piece.Knight, 3},
            {Piece.Bishop, 3},
            {Piece.Rook, 5},
            {Piece.Queen, 9},
            {Piece.King, 0} // checkmate already set to min/max int
        };
        
        public static int EvaluateMove(MoveGenerator moveGen)
        {  
            
            // #TODO Improve eval function; currently only considers material
            if(moveGen.IsCheckmate()) return moveGen.curTurnColor == Piece.White ? whiteCheckmate : blackCheckmate;
            int boardVal = 0;
            foreach(int idx in moveGen.whitePieces)
            {
                boardVal += pieceValue[Piece.Type(moveGen.boardData[idx])];
            }
            foreach(int idx in moveGen.blackPieces)
            {
                boardVal -= pieceValue[Piece.Type(moveGen.boardData[idx])];
            }
            return boardVal;
        }

        public static Move NegaMax(MoveGenerator moveGen, int a, int b) 
        {
            int eval;
            Move max;
            foreach(Move move in moveGen.possibleMoves)
            {
                EvaluateMove(new(moveGen.boardData, moveGen.curTurnColor, curMove: move, calc: false));
            }
            return max;
        }
    }
}