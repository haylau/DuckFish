using System;
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
        private static readonly Random rdm = new();
        public const string START = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -";
        public const string DEBUG = "Rq6/5N2/5r2/Rp5K/3Bp1P1/Q3n1Pp/3kP2P/1N5b w -";
        public const string DEPTHTEST_2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        public const string DEPTHTEST_3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
        public const string DEPTHTEST_4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        public const string DEPTHTEST_5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
        private bool AIDisabled = false;
        private bool AIMove_Random = false;
        private bool lockWhite = false;
        private bool lockBlack = false;
        private int[] boardData;
        private List<Move> prevMoves;
        private MoveGenerator moveGenerator = default!;
        private int boardOrientation = Piece.White;
        private int curTurn;
        private int playerColor;
        private int gameState;

        public Board()
        {
            boardData = new int[64];
            prevMoves = new();
        }

        public Board(Board board)
        {
            boardData = new int[64];
            prevMoves = new();
            SetBoard(board.boardData, board.prevMoves, board.curTurn);
        }
        public bool InCheck
        {
            get
            {
                return gameState == whiteCheck || gameState == blackCheck || gameState == whiteCheckmate || gameState == blackCheckmate;
            }
        }
        public bool Checkmate
        {
            get
            {
                return gameState == whiteCheckmate || gameState == blackCheckmate;
            }
        }
        public bool Stalemate
        {
            get
            {
                return gameState == stalemate;
            }
        }
        public int playerTurn
        {
            get
            {
                return curTurn;
            }
        }
        public int KingTile
        {
            get
            {
                return curTurn == Piece.White ? moveGenerator.whiteKingSquare : moveGenerator.blackKingSquare;
            }
        }
        public List<Move> possibleMoves
        {
            get
            {
                return moveGenerator.possibleMoves;
            }
        }
        public int[] BoardData
        {
            get
            {
                return boardData;
            }
        }

        public List<Move> PreviousMoves
        {
            get
            {
                return prevMoves;
            }
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
        public List<int> GetLegalTargets(string from)
        {
            return GetLegalTargets(TileToIndex(from));
        }
        public List<int> GetLegalTargets(int from)
        {
            List<int> legalTargets = new();
            foreach (Move m in moveGenerator.possibleMoves)
            {
                if (m.StartSquare == from && !legalTargets.Contains(m.TargetSquare))
                {
                    legalTargets.Add(m.TargetSquare);
                }
            }
            return legalTargets;
        }
        public List<int> GetPrevMove()
        {
            List<int> prevMove = new();
            prevMove.Add(prevMoves.Last().StartSquare);
            prevMove.Add(prevMoves.Last().TargetSquare);
            prevMove.Add(prevMoves.Last().MoveFlag);
            return prevMove;
        }
        public bool SetTile(int piece, string tile)
        {
            int idx = TileToIndex(tile);
            if (idx < 0 || idx > 63) return false; // illegal idx
            boardData[idx] = piece;
            return true;
        }

        public int TileToIndex(string tile) // converts tile name to the corresponding index value in the array
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
        public string IndexToString(int idx)
        {
            string str = "";
            int file = idx % 8;
            int row = idx - (idx % 8);
            foreach (var pair in files)
            {
                if (pair.Value == file) str += pair.Key;
            }
            foreach (var pair in rows)
            {
                if (pair.Value == row) str += pair.Key;
            }
            return str;
        }

        public void SetAIMovGen(string type)
        {
            if (type.ToLower().Equals("disable")) AIDisabled = true;
            if (type.ToLower().Equals("random")) AIMove_Random = true;
        }
        public void SelectColor(int color)
        {
            if (color == Piece.White) lockWhite = true;
            if (color == Piece.Bishop) lockBlack = true;
        }
        public void SetBoard() // Default board setup and random turn
        {
            // Decide Turn
            this.curTurn = Piece.White;
            if (lockWhite || (!lockBlack && rdm.Next() % 2 == 0))
            {
                this.boardOrientation = Piece.White;
                this.playerColor = Piece.White;
            }
            else
            {
                this.boardOrientation = Piece.Black;
                this.playerColor = Piece.Black;
                if (AIDisabled) this.playerColor = curTurn; // Lets player move opponent 
            }
            SetBoard(START);
        }

        /* 
            #TODO Castle Rights + Legal En Passant (not too important) 
        */
        public void SetBoard(string fen)
        {
            boardData = new int[64];
            prevMoves = new();
            bool rules = false, halfmove = false, enpassant = false;
            bool castle_wk = false, castle_wq = false, castle_bk = false, castle_bq = false;
            char[] enpassantMove = new char[] {'0', '0'};
            int idx = 0;
            foreach (char token in fen)
            {
                if (!rules && idx >= 64 || token.Equals(' ')) rules = true;
                else if (rules)
                {
                    if (token.Equals(' ') || token.Equals('-')) continue;
                    else if (token.Equals('w'))
                    {
                        this.curTurn = Piece.White;
                    }
                    else if (token.Equals('b'))
                    {
                        this.curTurn = Piece.Black;
                    }
                    else if (token.Equals('K')) castle_wk = true;
                    else if (token.Equals('Q')) castle_wq = true;
                    else if (token.Equals('k')) castle_bk = true;
                    else if (token.Equals('q')) castle_bq = true;
                    else if (enpassant && enpassantMove[1] == '0')
                    {
                        enpassantMove[1] = token;
                        continue;
                    }
                    else if (!halfmove && Char.IsDigit(token))
                    {
                        halfmove = true;
                        // set halfmove
                    }
                    else if (halfmove && Char.IsDigit(token))
                    {
                        int totalMoves = (int)char.GetNumericValue(token);
                        // Add moves to invalidate castle check
                        if (!castle_wk)
                        {
                            prevMoves.Add(new Move(startingWhiteKingRook, startingWhiteKingRook));
                            --totalMoves;
                        }
                        if (!castle_wq)
                        {
                            prevMoves.Add(new Move(startingWhiteQueenRook, startingWhiteQueenRook));
                            --totalMoves;
                        }
                        if (!castle_bk)
                        {
                            prevMoves.Add(new Move(startingBlackKingRook, startingBlackKingRook));
                            --totalMoves;
                        }
                        if (!castle_bq)
                        {
                            prevMoves.Add(new Move(startingBlackQueenRook, startingBlackQueenRook));
                            --totalMoves;
                        }
                        if (enpassant) // '6' != 6 
                        {
                            if (char.GetNumericValue(enpassantMove[1]) == 6)
                            {
                                prevMoves.Add(new Move(8 + files[enpassantMove[0]], 24 + files[enpassantMove[0]], Move.Flag.PawnTwoForward));
                            }
                            else if (char.GetNumericValue(enpassantMove[1]) == 3)
                            {
                                prevMoves.Add(new Move(48 + files[enpassantMove[0]], 32 + files[enpassantMove[0]], Move.Flag.PawnTwoForward));
                            }
                            else enpassant = false; // invalid enpassant target
                        }
                        --totalMoves; // current move has not happened yet
                        for (int i = 1; i < totalMoves; ++i)
                        {
                            // fill rest with meaningless moves
                            prevMoves.Add(Move.InvalidMove);
                        }
                    }
                    else if (!enpassant) // first en passant target
                    {
                        enpassantMove[0] = token;
                        enpassant = true;
                    }
                    continue;
                }
                // token is a newline
                else if (token.Equals('/') && !rules)
                {
                    if (idx % 8 == 0) continue; // already at end of row
                    idx += 8 - (idx % 8);
                    if (idx >= 64) rules = true; // end of board
                    continue;
                }
                // token empty space
                else if (char.IsDigit(token) && !rules)
                {
                    idx += (int)char.GetNumericValue(token);
                    if (idx >= 64) rules = true; // end of board
                    continue;
                }
                // token is a piece
                else if (char.IsLetter(token) && !rules)
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
            if (AIDisabled) this.playerColor = curTurn;
            moveGenerator = new MoveGenerator(boardData, curTurn, prevMoves);
        }
        public void SetBoard(int[] boardData, List<Move> prevMoves, int curTurn)
        {
            this.curTurn = curTurn;
            for (int i = 0; i < boardData.Length; ++i)
            {
                this.boardData[i] = boardData[i];
            }
            if (prevMoves is null) prevMoves = new();
            else
            {
                foreach (Move m in prevMoves)
                {
                    this.prevMoves.Add(m);
                }
            }
            if (AIDisabled) this.playerColor = curTurn;
            moveGenerator = new MoveGenerator(boardData, curTurn, prevMoves);
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
        public bool IsLegal(string fromTile, string toTile, int flag = Move.Flag.None) // checks if a move is legal
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
            if (possibleMoves.Contains(new Move(idxFrom, idxTo, flag))) return true;
            return false;
        }
        public bool IsPromotion(string fromTile, string toTile)
        {
            int idxFrom = TileToIndex(fromTile);
            int idxTo = TileToIndex(toTile);
            if (boardOrientation == Piece.Black)
            {
                idxFrom = flippedBoard[idxFrom];
                idxTo = flippedBoard[idxTo];
            }
            if (Piece.Type(boardData[idxFrom]) == Piece.Pawn) // pawn is being moved
            {
                if (idxFrom >= 8 && idxFrom <= 15 && idxTo >= 0 && idxTo <= 7)
                {
                    return true;
                }
                else if (idxFrom >= 48 && idxFrom <= 55 && idxTo >= 56 && idxTo <= 63)
                {
                    return true;
                }
            }
            return false;
        }

        public int MoveFlag(string tileFrom, string tileTo, int promotion = Piece.Empty)
        {
            return MoveFlag(TileToIndex(tileFrom), TileToIndex(tileTo), promotion);
        }
        public int MoveFlag(int idxFrom, int idxTo, int promotion)
        {
            if (boardOrientation == Piece.Black)
            {
                idxFrom = flippedBoard[idxFrom];
                idxTo = flippedBoard[idxTo];
            }
            if (Piece.Type(boardData[idxFrom]) == Piece.Pawn)
            {
                // Two forward
                if (Math.Abs(idxFrom - idxTo) == 16)
                {
                    return Move.Flag.PawnTwoForward;
                }
                // En passant
                if (prevMoves.Count > 0 && prevMoves.Last().MoveFlag == Move.Flag.PawnTwoForward)
                {
                    int enPassantTarget = prevMoves.Last().TargetSquare;
                    if (Math.Abs(enPassantTarget - idxTo) == 8)
                    {
                        return Move.Flag.EnPassantCapture;
                    }
                }
                // Promotion
                if (promotion != Piece.Empty && (idxTo >= 0 && idxTo <= 7) || (idxTo >= 56 && idxTo <= 63))
                {
                    if (promotion == Piece.Queen) return Move.Flag.PromoteToQueen;
                    if (promotion == Piece.Rook) return Move.Flag.PromoteToRook;
                    if (promotion == Piece.Knight) return Move.Flag.PromoteToKnight;
                    if (promotion == Piece.Bishop) return Move.Flag.PromoteToBishop;
                }
            }
            if (Piece.Type(boardData[idxFrom]) == Piece.King)
            {
                if (curTurn == Piece.White && idxFrom == startingWhiteKingSquare)
                {
                    if (idxTo == startingWhiteKingSquare - 2 || idxTo == startingWhiteKingSquare + 2) return Move.Flag.Castling;
                }
                if (curTurn == Piece.Black && idxFrom == startingBlackKingSquare)
                {
                    if (idxTo == startingBlackKingSquare - 2 || idxTo == startingBlackKingSquare + 2) return Move.Flag.Castling;
                }
            }
            return Move.Flag.None;
        }

        private void ResolveGame()
        {
            if (moveGenerator.inCheck) // curTurn player is in check with no legal moves
            {
                if (curTurn == Piece.White) gameState = blackCheckmate;
                else if (curTurn == Piece.Black) gameState = whiteCheckmate;
                else throw new Exception("Invalid Player State");
            }
            else gameState = stalemate; // curTurn player is NOT in check with no legal moves
        }
        private void MakeMove(string fromTile, string toTile, int flag)
        {
            MakeMove(TileToIndex(fromTile), TileToIndex(toTile), flag);
        }
        private void MakeMove(int idxFrom, int idxTo, int flag)
        {
            // Move should already be checked to be legal 
            if (boardOrientation == Piece.Black)
            {
                idxFrom = flippedBoard[idxFrom];
                idxTo = flippedBoard[idxTo];
            }
            prevMoves.Add(new Move(idxFrom, idxTo, flag));
            boardData[idxTo] = boardData[idxFrom]; // from -> to
            boardData[idxFrom] = Piece.Empty; // from is now empty
            if (flag == Move.Flag.PromoteToQueen)
            {
                boardData[idxTo] = curTurn + Piece.Queen;
            }
            if (flag == Move.Flag.PromoteToRook)
            {
                boardData[idxTo] = curTurn + Piece.Rook;
            }
            if (flag == Move.Flag.PromoteToKnight)
            {
                boardData[idxTo] = curTurn + Piece.Knight;
            }
            if (flag == Move.Flag.PromoteToBishop)
            {
                boardData[idxTo] = curTurn + Piece.Bishop;
            }
            if (flag == Move.Flag.EnPassantCapture)
            {
                // Remove pawn captured en passant
                if (curTurn == Piece.White)
                {
                    boardData[idxTo - pawnForward] = Piece.Empty;
                }
                if (curTurn == Piece.Black)
                {
                    boardData[idxTo + pawnForward] = Piece.Empty;
                }
            }
            if (flag == Move.Flag.Castling)
            {
                // Move Rook
                if (curTurn == Piece.White)
                {
                    if (idxTo == startingWhiteKingSquare - 2)
                    {
                        boardData[59] = Piece.White + Piece.Rook;
                        boardData[56] = Piece.Empty;
                    }
                    if (idxTo == startingWhiteKingSquare + 2)
                    {
                        boardData[61] = Piece.White + Piece.Rook;
                        boardData[63] = Piece.Empty;
                    }
                }
                if (curTurn == Piece.Black)
                {
                    if (idxTo == startingBlackKingSquare - 2)
                    {
                        boardData[3] = Piece.Black + Piece.Rook;
                        boardData[0] = Piece.Empty;
                    }
                    if (idxTo == startingBlackKingSquare + 2)
                    {
                        boardData[5] = Piece.Black + Piece.Rook;
                        boardData[7] = Piece.Empty;
                    }
                }
            }
        }
        public void PlayerMove(string fromTile, string toTile, int flag)
        {
            PlayerMove(TileToIndex(fromTile), TileToIndex(toTile), flag);
        }
        public void PlayerMove(int fromTile, int toTile, int flag)
        {
            // make player move
            MakeMove(fromTile, toTile, flag);
            // swap turns
            curTurn = curTurn == Piece.White ? Piece.Black : Piece.White; // swap game turn
            // generate opponent legal moves
            moveGenerator = new MoveGenerator(boardData, curTurn, prevMoves);
            if (moveGenerator.possibleMoves.Count == 0) ResolveGame(); // player has checkmate or stalemate is on board 
            else if (moveGenerator.inCheck)
            {
                gameState = playerTurn == Piece.White ? blackCheck : whiteCheck;
            }
            else
            {
                gameState = 0;
            }
            if (AIDisabled)
            {
                playerColor = curTurn; // player moves both
            }
        }
        public void OpponentMove()
        {
            if (AIDisabled) return; // player will reuse PlayerMove() 
            // #TODO Make AI Move
            if (AIMove_Random)
            {
                Move m = moveGenerator.possibleMoves[rdm.Next(0, moveGenerator.possibleMoves.Count)];
                MakeMove(m.StartSquare, m.TargetSquare, m.MoveFlag);
            }

            // generate player legal moves
            curTurn = curTurn == Piece.White ? Piece.Black : Piece.White; // swap game turn
            moveGenerator = new MoveGenerator(boardData, curTurn, prevMoves);
            if (moveGenerator.possibleMoves.Count == 0) ResolveGame(); // opponent has checkmate or stalemate is on board
            else if (moveGenerator.inCheck)
            {
                gameState = playerTurn == Piece.White ? whiteCheck : blackCheck;
            }
            else
            {
                gameState = 0;
            }

        }
    }
}