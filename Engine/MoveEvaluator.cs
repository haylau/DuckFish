
namespace Engine
{
    using static MoveData;
    public class MoveEvaluator
    {
        public const int whiteCheckmate = 100000;
        public const int blackCheckmate = -100000;
        public static readonly Dictionary<int, int> pieceValue = new()
        {
            {Piece.Pawn, 100},
            {Piece.Knight, 300},
            {Piece.Bishop, 300},
            {Piece.Rook, 500},
            {Piece.Queen, 900},
            {Piece.King, 0} // checkmate already set to -100000/100000 int
        };
        public static readonly int[] posVal_Pawns = {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 30, 30, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 55, 55, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };
        public static readonly int[] posVal_Knights = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };
        public static readonly int[] posVal_Bishops = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };
        public static readonly int[] posVal_Rooks = {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        };
        public static readonly int[] posVal_Queen = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };
        public static readonly int[] posVal_King = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            20, 20,  0,  0,  0,  0, 20, 20,
            20, 30, 10,  0,  0, 10, 30, 20
        };
        public static readonly int[] posVal_KingEndgame = {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };
        private static readonly Random rdm = new();
        private Move bestMove;
        private int numNodes;
        private int halfmove;
        private MoveGenerator originalPosition;
        public MoveEvaluator(MoveGenerator moveGen)
        {
            this.numNodes = 0;
            this.halfmove = 0;
            this.bestMove = Move.InvalidMove;
            this.originalPosition = new MoveGenerator(moveGen.boardData, moveGen.curTurnColor, moveGen.prevMoves);
        }
        private static int getPositionalValue(int idx, int type)
        {
            switch (type)
            {
                case (Piece.Pawn):
                    {
                        return posVal_Pawns[idx];
                    }
                case (Piece.Knight):
                    {
                        return posVal_Knights[idx];
                    }
                case (Piece.Bishop):
                    {
                        return posVal_Bishops[idx];
                    }
                case (Piece.Rook):
                    {
                        return posVal_Rooks[idx];
                    }
                case (Piece.Queen):
                    {
                        return posVal_Queen[idx];
                    }
                case (Piece.King):
                    {
                        return posVal_King[idx];
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public static int EvaluateMove(MoveGenerator moveGen)
        {
            // #TODO Improve eval function; currently only considers material
            if (moveGen.IsCheckmate) return moveGen.curTurnColor == Piece.White ? whiteCheckmate : blackCheckmate;
            int boardVal = 0;
            foreach (int idx in moveGen.whitePieces)
            {
                boardVal += pieceValue[Piece.Type(moveGen.boardData[idx])];
                boardVal += getPositionalValue(idx, Piece.Type(moveGen.boardData[idx]));
            }
            foreach (int idx in moveGen.blackPieces)
            {
                boardVal -= pieceValue[Piece.Type(moveGen.boardData[idx])];
                boardVal -= getPositionalValue(idx, Piece.Type(moveGen.boardData[idx]));
            }
            return boardVal;
        }

        public Move Search(int depth)
        {
            int eval = NegaMax(originalPosition, depth, blackCheckmate, whiteCheckmate);
            if(bestMove.IsInvalid)
            {
                return originalPosition.possibleMoves[rdm.Next(0, originalPosition.possibleMoves.Count)]; // #TODO prevent choosing no move 
            }
            return bestMove;
        }

        private int NegaMax(MoveGenerator moveGenerator, int depth, int alpha, int beta)
        {
            if (depth == 0)
            {
                ++this.numNodes;
                return EvaluateMove(moveGenerator);
            }
            int bound = alpha;
            Move runningBestMove = Move.InvalidMove;
            foreach (Move move in moveGenerator.possibleMoves)
            {
                ++this.halfmove;
                MoveGenerator nextPosition = new MoveGenerator(moveGenerator.boardData, moveGenerator.curTurnColor, moveGenerator.prevMoves, move);
                int score = -1 * NegaMax(nextPosition, depth: depth - 1, alpha: -1 * beta, beta: -1 * alpha);
                --this.halfmove;
                // hardfail cutoff
                if (score >= beta)
                {
                    return beta;
                }
                // new best move
                if (score > alpha)
                {
                    // increase lower a-b bound
                    alpha = score;
                    if (halfmove == 0)
                    {
                        runningBestMove = move;
                    }
                }
            }
            if (bound != alpha) bestMove = runningBestMove;
            return alpha;
        }
    }
}