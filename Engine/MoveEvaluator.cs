
namespace Engine
{
    using static MoveData;
    public class MoveEvaluator
    {
        private bool debug = true;
        private System.Diagnostics.Stopwatch? watch;
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
        private static readonly Dictionary<int, int> flippedBoard = new()
        {
            {0, 56},
            {1, 57},
            {2, 58},
            {3, 59},
            {4, 60},
            {5, 61},
            {6, 62},
            {7, 63},
            {8, 48},
            {9, 49},
            {10, 50},
            {11, 51},
            {12, 52},
            {13, 53},
            {14, 54},
            {15, 55},
            {16, 40},
            {17, 41},
            {18, 42},
            {19, 43},
            {20, 44},
            {21, 45},
            {22, 46},
            {23, 47},
            {24, 32},
            {25, 33},
            {26, 34},
            {27, 35},
            {28, 36},
            {29, 37},
            {30, 38},
            {31, 39},
            {32, 24},
            {33, 25},
            {34, 26},
            {35, 27},
            {36, 28},
            {37, 29},
            {38, 30},
            {39, 31},
            {40, 16},
            {41, 17},
            {42, 18},
            {43, 19},
            {44, 20},
            {45, 21},
            {46, 22},
            {47, 23},
            {48, 8},
            {49, 9},
            {50, 10},
            {51, 11},
            {52, 12},
            {53, 13},
            {54, 14},
            {55, 15},
            {56, 0},
            {57, 1},
            {58, 2},
            {59, 3},
            {60, 4},
            {61, 5},
            {62, 6},
            {63, 7}
        };
        public static readonly int[] posVal_Pawns = {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 30, 30, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
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
        private int numPieces; // number of non-pawn or king pieces
        private MoveGenerator originalPosition;
        public MoveEvaluator(MoveGenerator moveGen)
        {
            this.numNodes = 0;
            this.halfmove = 0;
            this.bestMove = Move.InvalidMove;
            this.originalPosition = new MoveGenerator(moveGen.boardData, moveGen.curTurnColor, moveGen.prevMoves);
            this.numPieces = originalPosition.knightSquares.Count
                           + originalPosition.bishopSquares.Count
                           + originalPosition.rookSquares.Count
                           + originalPosition.queenSquares.Count;
        }
        private int getPositionalValue(int idx, int type)
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
                        if (numPieces < 8) // king can be more active now
                        {
                            return posVal_KingEndgame[idx];
                        }
                        else
                        {
                            return posVal_King[idx];
                        }
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public int EvaluateMove(MoveGenerator moveGen)
        {
            // #TODO Improve eval function; currently only considers material
            if (moveGen.IsCheckmate) return moveGen.curTurnColor == Piece.White ? blackCheckmate : whiteCheckmate;
            int boardVal = 0;
            foreach (int idx in moveGen.whitePieces)
            {
                boardVal += pieceValue[Piece.Type(moveGen.boardData[idx])];
                boardVal += getPositionalValue(idx, Piece.Type(moveGen.boardData[idx]));
            }
            foreach (int idx in moveGen.blackPieces)
            {
                boardVal -= pieceValue[Piece.Type(moveGen.boardData[idx])];
                boardVal -= getPositionalValue(flippedBoard[idx], Piece.Type(moveGen.boardData[idx]));
            }
            return moveGen.curTurnColor == Piece.White ? boardVal : -1 * boardVal;
        }

        public Move Search(int depth)
        {
            if (debug)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
            }
            int eval = NegaMax(originalPosition, depth, blackCheckmate, whiteCheckmate);
            if (bestMove.IsInvalid)
            {
                return originalPosition.OrderedMoves[rdm.Next(0, originalPosition.OrderedMoves.Count)]; // #TODO prevent choosing no move 
            }
            if (debug)
            {
                if (watch is not null)
                {
                    watch.Stop();
                    Console.WriteLine("Searched: " + this.numNodes + " after " + watch.ElapsedMilliseconds + "ms\n" + " eval: " + eval);
                }
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
            foreach (Move move in moveGenerator.OrderedMoves)
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