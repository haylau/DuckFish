namespace Engine
{
    using System.Collections.Generic;
    using static MoveData;
    public class MoveGenerator
    {
        public int curTurnColor;
        public int opponentTurnColor;
        public List<Move>? prevMoves;
        public List<Move> possibleMoves;
        public int[] boardData;
        public int[] attackedSquares;
        public int whiteKingSquare;
        public int blackKingSquare;
        /* 
            * a8 b8 c8 d8 e8 f8 g8 h8 | 00 01 02 03 04 05 06 07
            * a7 b7 c7 d7 e7 f7 g7 h7 | 08 09 10 11 12 13 14 15
            * a6 b6 c6 d6 e6 f6 g6 h6 | 16 17 18 19 20 21 22 23 
            * a5 b5 c5 d5 e5 f5 g5 h5 | 24 25 26 27 28 29 30 31
            * ------------------------|------------------------
            * a4 b4 c4 d4 e4 f4 g4 h4 | 32 33 34 35 36 37 38 39
            * a3 b3 c3 d3 e3 f3 g3 h3 | 40 41 42 43 44 45 46 47  
            * a2 b2 c2 d2 e2 f2 g2 h2 | 48 49 50 51 52 53 54 55
            * a1 b1 c1 d1 e1 f1 g1 h1 | 56 57 58 59 60 61 62 63
            */


        public MoveGenerator(int[] boardData, int curTurnColor, List<Move> prevMoves)
        {
            this.boardData = boardData;
            this.prevMoves = prevMoves;
            this.curTurnColor = curTurnColor;
            this.opponentTurnColor = curTurnColor == Piece.White ? Piece.Black : Piece.White;
            possibleMoves = new();
            attackedSquares = new int[64];
            CalculateKingDanger();
            GenerateMoves();
        }

        public List<Move> GenerateMoves() // generates a list of all current legal moves
        {

            for (int idx = 0; idx < 64; ++idx)
            {
                // in check check?
                // double check check (only king moves are legal)
                // pinned piece check
                int piece = boardData[idx];
                int color = Piece.Color(piece);
                int type = Piece.Type(piece);

                if (color == curTurnColor)
                {
                    // These pieces all share similar move searches; search in straight lines
                    if (type == Piece.Queen || type == Piece.Rook || type == Piece.Bishop)
                    {
                        /*  7 0 1 
                        *   6 - 2
                        *   5 4 3
                        */
                        for (int direction = 0; direction < moveOffsets.Length; ++direction)
                        {
                            if (type == Piece.Bishop && direction % 2 == 0) continue; // Bishops cannot move cardinally
                            if (type == Piece.Rook && direction % 2 == 1) continue; // Rooks cannot move diagonally
                            for (int dist = 0; dist < distToEdge[idx][direction]; ++dist)
                            {
                                int target = idx + moveOffsets[direction] * (dist + 1); // num moves in a given direction
                                if (Piece.Color(boardData[target]) == curTurnColor) break; // cannot capture any further from own pieces
                                if (Piece.Color(boardData[target]) == opponentTurnColor)
                                {
                                    possibleMoves.Add(new Move(idx, target));
                                    break;
                                }
                                possibleMoves.Add(new Move(idx, target));
                            }
                        }
                    }
                    if (type == Piece.Knight)
                    {
                        foreach (int target in knightMoves[idx]) // checking all legal knight moves
                        {
                            if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                            possibleMoves.Add(new Move(idx, target));
                        }
                    }
                    if (type == Piece.Pawn)
                    {
                        // -8 / +8 offsets; One tile pawn push
                        if (curTurnColor == Piece.White) // Can only move up
                        {
                            // Forward
                            if (idx - 8 >= 0 && idx - 8 < 64 && Piece.Type(boardData[idx - 8]) == Piece.Empty) // Can only move into empty tiles
                            {
                                possibleMoves.Add(new Move(idx, idx - 8));
                            }
                            // Adjacent capture
                            if (idx - 7 >= 0 && idx - 7 < 64 && Piece.Color(boardData[idx - 7]) == opponentTurnColor) // Can only capture enemy pieces
                            {
                                possibleMoves.Add(new Move(idx, idx - 7));
                            }
                            if (idx - 9 >= 0 && idx - 9 < 64 && Piece.Color(boardData[idx - 9]) == opponentTurnColor) // Can only capture enemy pieces
                            {
                                possibleMoves.Add(new Move(idx, idx - 9));
                            }
                            // Forward twice
                            if (idx >= 48 && idx <= 55)
                            {
                                // Can only move into/through empty tiles
                                if (idx - 16 >= 0 && idx - 16 < 64 && Piece.Type(boardData[idx - 8]) == Piece.Empty && Piece.Type(boardData[idx - 16]) == Piece.Empty)
                                {
                                    possibleMoves.Add(new Move(idx, idx - 16));
                                }
                            }
                        }
                        if (curTurnColor == Piece.Black) // Can only move down
                        {
                            // Forward
                            if (idx + 8 >= 0 && idx + 8 < 64 && Piece.Type(boardData[idx + 8]) == Piece.Empty) // Can only move into empty tiles
                            {
                                possibleMoves.Add(new Move(idx, idx + 8));
                            }
                            // Adjacent capture
                            if (idx + 7 >= 0 && idx + 7 < 64 && Piece.Color(boardData[idx + 7]) == opponentTurnColor) // Can only capture enemy pieces
                            {
                                possibleMoves.Add(new Move(idx, idx + 7));
                            }
                            if (idx + 9 >= 0 && idx + 9 < 64 && Piece.Color(boardData[idx + 9]) == opponentTurnColor) // Can only capture enemy pieces
                            {
                                possibleMoves.Add(new Move(idx, idx + 9));
                            }
                            // Forward twice
                            if (idx >= 8 && idx <= 15)
                            {
                                // Can only move into/through empty tiles
                                if (idx + 16 >= 0 && idx + 16 < 64 && Piece.Type(boardData[idx + 8]) == Piece.Empty && Piece.Type(boardData[idx + 16]) == Piece.Empty)
                                {
                                    possibleMoves.Add(new Move(idx, idx + 16));
                                }
                            }
                        }
                    }
                }
            }
            return possibleMoves;
        }

        private void CalculateKingDanger()
        {
            for (int idx = 0; idx < 64; ++idx)
            {
                // Locate Kings
                if (Piece.Type(boardData[idx]) == Piece.King)
                {
                    if (Piece.Color(boardData[idx]) == Piece.White)
                    {
                        whiteKingSquare = idx;
                    }
                    if (Piece.Color(boardData[idx]) == Piece.Black)
                    {
                        blackKingSquare = idx;
                    }
                }
            }
        }
    }
}