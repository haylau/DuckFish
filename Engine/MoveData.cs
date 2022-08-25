namespace Engine
{
    public static class MoveData
    {
        /*     
        *   -9 -8 -7  7 0 1 
        *   -1 () +1  6 - 2
        *   +7 +8 +9  5 4 3
        */
        public static readonly int[] moveOffsets = { -8, -7, 1, 9, 8, 7, -1, -9 }; // Clockwise from Up  
        public static readonly int[] queenOffsets = { -8, -7, 1, 9, 8, 7, -1, -9 };
        public static readonly int[] kingOffsets = { -8, -7, 1, 9, 8, 7, -1, -9 };
        public static readonly int[] bishopOffsets = { -7, 9, 7, -9 };
        public static readonly int[] rookOffsets = { -8, 1, 8, -1 };
        /*     
        *   --- -17 --- -15 ---
        *   -10 --- --- --- -06
        *   --- --- (_) --- ---
        *   +10 --- --- --- +06
        *   --- +17 --- +15 ---
        */
        public static readonly int[] knightOffsets = { -17, -15, -10, -6, 10, 6, 17, 15 };
        public static readonly int[][] distToEdge; // Distance from every tile to every other tile
        public static readonly int[][] knightMoves; // Valid knight moves from every tile to every other tile
        public const int pawnForward = -8;
        public const int pawnTwoForward = -16;
        public const int pawnLeftCapture = -9;
        public const int pawnRightCapture = -7;
        public const int pawnLeftEnPassant = -1;
        public const int pawnRightEnPassant = +1;
        public const int startingWhiteKingSquare = 60;
        public const int startingBlackKingSquare = 4;
        public const int startingWhiteKingRook = 63;
        public const int startingWhiteQueenRook = 56;
        public const int startingBlackKingRook = 7;
        public const int startingBlackQueenRook = 0;
        public const int whiteCheck = 1;
        public const int blackCheck = 2;
        public const int whiteCheckmate = 3; // White has a victory
        public const int blackCheckmate = 4; // Black has a victory
        public const int stalemate = 5;
        static MoveData()
        {
            /* 
            * a8 b8 c8 d8 e8 f8 g8 h8 | 00 01 02 03 04 05 06 07
            * a7 b7 c7 d7 e7 f7 g7 h7 | 08 09 10 11 12 13 14 15
            * a6 b6 c6 d6 e6 f6 g6 h6 | 16 17 18 19 20 21 22 23 
            * a5 b5 c5 d5 e5 f5 g5 h5 | 24 25 26 27 28 29 30 31
            * a4 b4 c4 d4 e4 f4 g4 h4 | 32 33 34 35 36 37 38 39
            * a3 b3 c3 d3 e3 f3 g3 h3 | 40 41 42 43 44 45 46 47  
            * a2 b2 c2 d2 e2 f2 g2 h2 | 48 49 50 51 52 53 54 55
            * a1 b1 c1 d1 e1 f1 g1 h1 | 56 57 58 59 60 61 62 63
            */

            distToEdge = new int[64][];
            knightMoves = new int[64][];
            for (int file = 0; file < 8; ++file)
            {
                for (int rank = 0; rank < 8; ++rank)
                {
                    // Legal moves in Cardinal and Diagonal directions
                    // Queen Rook Bishop
                    int distUp = rank;
                    int distRight = 7 - file;
                    int distDown = 7 - rank;
                    int distLeft = file;
                    int idx = rank * 8 + file;

                    distToEdge[idx] = new int[8];
                    distToEdge[idx][0] = distUp;
                    distToEdge[idx][1] = Math.Min(distUp, distRight);
                    distToEdge[idx][2] = distRight;
                    distToEdge[idx][3] = Math.Min(distRight, distDown);
                    distToEdge[idx][4] = distDown;
                    distToEdge[idx][5] = Math.Min(distDown, distLeft);
                    distToEdge[idx][6] = distLeft;
                    distToEdge[idx][7] = Math.Min(distLeft, distUp);

                    // Legal knight moves   
                    List<int> legalKnightMoves = new();
                    foreach (int knightJump in knightOffsets)
                    {
                        int target = idx + knightJump;
                        if (target >= 0 && target < 64) // knight is somewhere in the board
                        {
                            int dy = Math.Abs(rank - (target / 8));
                            int dx = Math.Abs(file - (target % 8));
                            if ((dy == 2 && dx == 1) || (dy == 1 && dx == 2)) // knight has moved in an L shape
                            {
                                legalKnightMoves.Add(target);
                            }
                        }
                    }
                    knightMoves[idx] = legalKnightMoves.ToArray();
                }
            }
        }
    }
}