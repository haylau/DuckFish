using System.Text.RegularExpressions;

namespace Engine
{
    public class Board
    {

        private static Dictionary<char, int> files = new Dictionary<char, int> {
            {'a', 0},
            {'b', 1},
            {'c', 2},
            {'d', 3},
            {'e', 4},
            {'f', 5},
            {'g', 6},
            {'h', 7}
        };
        private static Dictionary<char, int> rows = new Dictionary<char, int> {
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
        public const string debug = "KKKKKKKK/KKKKKKKK/8/8/8/8/KKKKKKKK/KKKKKKKK";
        private int[] Square = default!;

        public Board()
        {
            Square = new int[64];
        }

        public int getTile(int tile)
        {
            if (tile < 0 || tile > 64) return -1;
            return Square[tile];
        }

        public int getTile(string tile)
        {
            return getTile(tileToIndex(tile));
        }

        private int tileToIndex(string tile)
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

        public void setBoard(string fen)
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
                    Square[idx] += char.IsUpper(token) ? Piece.White : Piece.Black;
                    switch (char.ToUpper(token))
                    {
                        case 'P':
                            {
                                Square[idx] += Piece.Pawn;
                                break;
                            }
                        case 'N':
                            {
                                Square[idx] += Piece.Knight;
                                break;
                            }
                        case 'B':
                            {
                                Square[idx] += Piece.Bishop;
                                break;
                            }
                        case 'R':
                            {
                                Square[idx] += Piece.Rook;
                                break;
                            }
                        case 'Q':
                            {
                                Square[idx] += Piece.Queen;
                                break;
                            }
                        case 'K':
                            {
                                Square[idx] += Piece.King;
                                break;
                            }
                    }
                    ++idx;
                    continue;
                }

            }
        }

    }
}