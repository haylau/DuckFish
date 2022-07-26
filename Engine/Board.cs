using System.Text.RegularExpressions;

namespace Engine
{
    using static MoveData;
    public class Board
    {

        private static readonly Dictionary<char, int> files = new()
        {
            {'a', 0},
            {'b', 1},
            {'c', 2},
            {'d', 3},
            {'e', 4},
            {'f', 5},
            {'g', 6},
            {'h', 7}
        };
        private static readonly Dictionary<char, int> rows = new()
        {
            {'8', 0},
            {'7', 8},
            {'6', 16},
            {'5', 24},
            {'4', 32},
            {'3', 40},
            {'2', 48},
            {'1', 56}
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
        public const string START = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        public const string DEBUG = "Rq6/5N2/5r2/Rp5K/3Bp1P1/Q3n1Pp/3kP2P/1N5b";
        private bool debug;
        private readonly int[] boardData;
        private static readonly MoveGenerator playerMoveGen = new();
        private List<Move> playerMoves = default!;
        private int boardOrientation = Piece.White;
        private int curTurn;
        private int playerColor;

        public Board()
        {
            boardData = new int[64];
        }

        public int GetTile(int tile)
        {
            if (tile < 0 || tile > 64) return -1;
            if (boardOrientation == Piece.Black)
            {
                return boardData[flippedBoard[tile]];
            }
            return boardData[tile];
        }

        public int GetTile(string tile)
        {
            return GetTile(TileToIndex(tile));
        }

        public bool SetTile(int piece, string tile)
        {
            int idx = TileToIndex(tile);
            if (idx < 0 || idx > 63) return false; // illegal idx
            boardData[idx] = piece;
            return true;
        }

        private static int TileToIndex(string tile) // converts tile name to the corresponding index value in the array
        {
            if (!Regex.IsMatch(tile, @"^([a-h]{1}[1-8]{1})$")) return -1; // is not a1-h8 
            try
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

                return files[tile[0]] + rows[tile[1]];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + " : " + tile);
                return -1;
            }

        }

        public void EnableDebug()
        {
            debug = true;
        }

        public void SetBoard() // Default board setup and random turn
        {
            // Decide Turn
            this.curTurn = Piece.White;
            Random rdm = new();
            if (rdm.Next() % 2 == 0)
            {
                this.boardOrientation = Piece.White;
                this.playerColor = Piece.White;

            }
            else
            {
                this.boardOrientation = Piece.Black;
                this.playerColor = Piece.Black;
                if (debug) this.playerColor = curTurn; // Lets player move opponent 
            }
            SetBoard(START);
        }

        /* 
            Todo: 
            Active Color
            + Castle Rights + Legal En Passant (not too important) 
        */
        public void SetBoard(string fen)
        {
            int idx = 0;
            foreach (char token in fen)
            {
                // token is a newline
                if (token.Equals('/'))
                {
                    if (idx % 8 == 0) continue; // already at end of row
                    idx += 8 - (idx % 8);
                    if (idx >= 64) return; // end of board
                    continue;
                }
                // token empty space
                if (char.IsDigit(token))
                {
                    idx += (int)char.GetNumericValue(token);
                    if (idx >= 64) return; // end of board
                    continue;
                }
                // token is a piece
                if (char.IsLetter(token))
                {
                    boardData[idx] += char.IsUpper(token) ? Piece.White : Piece.Black;
                    switch (char.ToUpper(token))
                    {
                        case 'P':
                            {
                                boardData[idx] += Piece.Pawn;
                                break;
                            }
                        case 'N':
                            {
                                boardData[idx] += Piece.Knight;
                                break;
                            }
                        case 'B':
                            {
                                boardData[idx] += Piece.Bishop;
                                break;
                            }
                        case 'R':
                            {
                                boardData[idx] += Piece.Rook;
                                break;
                            }
                        case 'Q':
                            {
                                boardData[idx] += Piece.Queen;
                                break;
                            }
                        case 'K':
                            {
                                boardData[idx] += Piece.King;
                                break;
                            }
                    }
                    ++idx;
                    continue;
                }
            }
            playerMoves = playerMoveGen.GenerateMoves(boardData, curTurn);
        }

        public bool IsMoveable(string tile) // checks if a piece is able to be moved
        {
            if (curTurn != playerColor) return false; // not the players turn
            int idx = TileToIndex(tile);
            if (boardOrientation == Piece.Black)
            {
                idx = flippedBoard[idx];
            }
            if (Piece.Color(boardData[idx]) != playerColor) return false; // not the player's piece
            if (idx < 0 || idx > 63) return false; // illegal idx
            return true;
        }
        public bool IsLegal(string fromTile, string toTile) // checks if a move is legal
        {
            if (fromTile == toTile) return false; // cannot move to itself
            int idxFrom = TileToIndex(fromTile);
            int idxTo = TileToIndex(toTile);
            if (idxFrom < 0 || idxFrom > 63 || idxTo < 0 || idxTo > 63) return false; // illegal idx
            if (boardOrientation == Piece.Black)
            {
                idxFrom = flippedBoard[idxFrom];
                idxTo = flippedBoard[idxTo];
            }
            int moveDist = idxFrom - idxTo;
            int fromColor = Piece.Color(boardData[idxFrom]);
            int toColor = Piece.Color(boardData[idxTo]);
            if (fromColor == toColor) return false; // cannot capture your own pieces
            int fromPiece = Piece.Type(boardData[idxFrom]);
            if (playerMoves.Contains(new Move(idxFrom, idxTo))) return true;
            return false;
        }
        public bool Move(string fromTile, string toTile)
        {
            // Move should already be checked to be legal 
            int idxFrom = TileToIndex(fromTile);
            int idxTo = TileToIndex(toTile);
            if (idxFrom < 0 || idxFrom > 63 || idxTo < 0 || idxTo > 63) return false; // illegal idx
            if (boardOrientation == Piece.Black)
            {
                idxFrom = flippedBoard[idxFrom];
                idxTo = flippedBoard[idxTo];
            }
            boardData[idxTo] = boardData[idxFrom]; // from -> to
            boardData[idxFrom] = Piece.Empty; // from is now empty
            curTurn = curTurn == Piece.White ? Piece.Black : Piece.White; // swap game turn
            // CalculateAIMove();
            if (debug == true) // player moves both
            {
                playerColor = curTurn;
                playerMoves = playerMoveGen.GenerateMoves(boardData, curTurn);
            }
            return true;
        }
    }
}