using System.Text.RegularExpressions;

namespace Engine
{
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

        public const string start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        public const string debug = "Rq6/5N2/5r2/Rp5K/3Bp1P1/Q3n1Pp/3kP2P/1N5b";
        private readonly int[] boardData = default!;

        public Board()
        {
            boardData = new int[64];
        }

        public int GetTile(int tile)
        {
            if (tile < 0 || tile > 64) return -1;
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

        public bool Move(string fromTile, string toTile)
        {
            // Move should already be checked to be legal 
            int idxFrom = TileToIndex(fromTile);
            int idxTo = TileToIndex(toTile);
            if (idxFrom < 0 || idxFrom > 63 || idxTo < 0 || idxTo > 63) return false; // illegal idx
            boardData[idxTo] = boardData[idxFrom]; // from -> to
            boardData[idxFrom] = Piece.Empty; // from is now empty
            return true;
        }

        private static int TileToIndex(string tile) // converts tile name to the corresponding index value in the array
        {
            if (!Regex.IsMatch(tile, @"^([a-h]{1}[1-8]{1})$")) return -1;
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
        }

        public bool IsMoveable(string tile) {
            int idx = TileToIndex(tile);
            if (idx < 0 || idx > 63) return false; // illegal idx
            return true;
        }
        public bool IsLegal(string fromTile, string toTile)
        {
            int idxFrom = TileToIndex(fromTile);
            int idxTo = TileToIndex(toTile);
            if (idxFrom < 0 || idxFrom > 63 || idxTo < 0 || idxTo > 63) return false; // illegal idx
            // check piece type/color
            // check valid movement
            // check legal board state
            return true;
        }

    }
}