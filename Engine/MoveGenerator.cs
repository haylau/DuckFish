namespace Engine
{
    using System.Collections.Generic;
    using static MoveData;
    public class MoveGenerator
    {
        public int curTurnColor;
        public int opponentTurnColor;
        public List<Move> prevMoves;
        public List<Tuple<Move, int>> possibleMoves;
        public int[] boardData; // convert to bitboards
        public ulong bb_board;
        public List<int> whitePieces; // convert to bitboards
        public ulong bb_white;
        public List<int> blackPieces; // convert to bitboards
        public ulong bb_black;
        public List<int> pawnSquares; // convert to bitboards
        public ulong bb_pawn;
        public List<int> knightSquares; // convert to bitboards
        public ulong bb_knight;
        public List<int> bishopSquares; // convert to bitboards
        public ulong bb_bishop;
        public List<int> rookSquares; // convert to bitboards
        public ulong bb_rook;
        public List<int> queenSquares; // convert to bitboards
        public ulong bb_queen;
        public ulong bb_king;
        public bool[] attackedSquares;
        public ulong bb_attacked;
        public bool[] checkedSquares;
        public ulong bb_checked;
        public int[] pinnedSquares; // pinnedSquare[idx] = direction  
        public int pawnForwardDistance;
        public int pawnLeftDistance;
        public int pawnRightDistance;
        public int kingSquare;
        public int startingKingSquare;
        public int startingKingRook;
        public int startingQueenRook;
        public int whiteKingSquare;
        public int blackKingSquare;
        public bool kingCastle;
        public bool queenCastle;
        public int checkingPiece;
        public int checkDirection;
        public bool inCheck;
        public bool inDoubleCheck; // only check king moves
        public bool inKnightOrPawnCheck; // don't check for interposing moves
        public bool IsCheckmate
        {
            get
            {
                return this.PossibleMoves.Count == 0;
            }
        }
        public List<Move> PossibleMoves
        {
            get
            {
                // sort moves by move order
                possibleMoves = possibleMoves.OrderByDescending(b => b.Item2).ToList();
                List<Move> moves = new();
                foreach (Tuple<Move, int> move in this.possibleMoves)
                {
                    moves.Add(move.Item1);
                }
                return moves;
            }
        }
        public List<Move> OrderedMoves
        {
            get
            {
                List<Move> moves = new();
                foreach (Tuple<Move, int> move in this.possibleMoves)
                {
                    moves.Add(move.Item1);
                }
                return moves;
            }
        }

        // #TODO store move types
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
        public MoveGenerator(int[] boardData, int curTurnColor, List<Move>? prevMoves = default, Move curMove = default)
        {
            this.boardData = new int[boardData.Length];
            boardData.CopyTo(this.boardData, 0);
            ulong bitboard = BitBoard.convertToBitBoard(boardData);
            if (prevMoves is null)
            {
                this.prevMoves = new();
            }
            else
            {
                this.prevMoves = new(prevMoves);
            }
            this.curTurnColor = curTurnColor;
            if (!curMove.IsInvalid)
            {
                this.boardData = Move.MakeMove(this.boardData, this.curTurnColor, curMove);
                this.curTurnColor = this.curTurnColor == Piece.White ? Piece.Black : Piece.White; // swap game turn
                this.prevMoves.Add(curMove);
            }
            this.opponentTurnColor = this.curTurnColor == Piece.White ? Piece.Black : Piece.White;
            this.pawnForwardDistance = this.curTurnColor == Piece.White ? pawnForward : -1 * pawnForward;
            this.pawnLeftDistance = this.curTurnColor == Piece.White ? pawnLeftCapture : -1 * pawnLeftCapture;
            this.pawnRightDistance = this.curTurnColor == Piece.White ? pawnRightCapture : -1 * pawnRightCapture;
            this.possibleMoves = new();
            this.attackedSquares = new bool[64];
            this.checkedSquares = new bool[64];
            this.pinnedSquares = new int[64];
            this.checkingPiece = -1;
            this.whitePieces = new();
            this.blackPieces = new();
            this.pawnSquares = new();
            this.knightSquares = new();
            this.bishopSquares = new();
            this.rookSquares = new();
            this.queenSquares = new();
            LocatePieces();
            CalculateAttackedSquares();
            CalculatePinnedSquares();
            CalculateCheckedSquares();
            VerifyCastling();
            GenerateMoves();
        }

        private void AddMove(Move move)
        {
            int weight = 0;
            // Check if captures opponent
            if (Piece.Color(boardData[move.TargetSquare]) == opponentTurnColor)
            {
                weight += moveCapture;
            }
            // Check if checking opponent (m.Target square --> opponentKingSquare)
            int type = Piece.Type(boardData[move.StartSquare]);
            if (move.IsPromotion)
            {
                switch (move.MoveFlag)
                {
                    case (Move.Flag.PromoteToQueen):
                        {
                            type = Piece.Queen;
                            break;
                        }
                    case (Move.Flag.PromoteToRook):
                        {
                            type = Piece.Rook;
                            break;
                        }
                    case (Move.Flag.PromoteToBishop):
                        {
                            type = Piece.Bishop;
                            break;
                        }
                    case (Move.Flag.PromoteToKnight):
                        {
                            type = Piece.Knight;
                            break;
                        }
                }
            }
            int opponentKingSquare = (curTurnColor == Piece.White) ? blackKingSquare : whiteKingSquare;
            switch (type)
            {
                case (Piece.Queen):
                case (Piece.Rook):
                case (Piece.Bishop):
                    {
                        for (int direction = 0; direction < moveOffsets.Length; ++direction)
                        {
                            if (type == Piece.Bishop && direction % 2 == 0) continue; // Bishops cannot move cardinally
                            if (type == Piece.Rook && direction % 2 == 1) continue; // Rooks cannot move diagonally
                            for (int dist = 0; dist < distToEdge[opponentKingSquare][direction]; ++dist) // check for interposing moves 
                            {
                                int target = opponentKingSquare + moveOffsets[direction] * (dist + 1); // scan outwards from opponent king
                                if (move.TargetSquare == target) // reached the checking piece; no more interposing moves
                                {
                                    weight += moveCheck;
                                    goto AddMove; // move checks opponent; stop searching
                                }
                                if (Piece.Type(boardData[target]) != Piece.Empty) // Square can be used to block check
                                {

                                }
                            }
                        }
                        break;
                    }
                case (Piece.Knight):
                    {
                        foreach (int target in knightMoves[move.TargetSquare])
                        {
                            if (target == opponentKingSquare)
                            {
                                weight += moveCheck;
                                goto AddMove;
                            }
                        }
                        break;
                    }
                case (Piece.Pawn):
                    {
                        bool leftCapture = curTurnColor == Piece.White ? move.TargetSquare % 8 != 0 : (move.TargetSquare + 1) % 8 != 0;
                        bool rightCapture = curTurnColor == Piece.White ? (move.TargetSquare + 1) % 8 != 0 : move.TargetSquare % 8 != 0;
                        if (leftCapture && move.TargetSquare + pawnLeftDistance == opponentKingSquare)
                        {
                            weight += moveCheck;
                            goto AddMove;
                        }
                        if (rightCapture && move.TargetSquare + pawnRightDistance == opponentKingSquare)
                        {
                            weight += moveCheck;
                            goto AddMove;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        AddMove:
            // Add <move, weight>
            possibleMoves.Add(new Tuple<Move, int>(move, weight));
        }

        private void GenerateMoves() // generates a list of all current legal moves
        {

            if (inDoubleCheck) // King must move
            {
                // Check king moves
                for (int direction = 0; direction < moveOffsets.Length; ++direction)
                {
                    int target = kingSquare + moveOffsets[direction];
                    if (distToEdge[kingSquare][direction] > 0 && attackedSquares[target] == false)
                    {
                        if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                        AddMove(new Move(kingSquare, target));
                    }
                }
                return;
            }
            foreach (int idx in queenSquares)
            {
                GenerateLinearMoves(idx, boardData[idx], Piece.Queen);
            }
            foreach (int idx in rookSquares)
            {
                GenerateLinearMoves(idx, boardData[idx], Piece.Rook);
            }
            foreach (int idx in bishopSquares)
            {
                GenerateLinearMoves(idx, boardData[idx], Piece.Bishop);
            }
            foreach (int idx in knightSquares)
            {
                GenerateKnightMoves(idx);
            }
            foreach (int idx in pawnSquares)
            {
                GeneratePawnMoves(idx);
            }
            GenerateKingMoves(kingSquare);
        }

        private void GenerateLinearMoves(int idx, int piece, int type) // Queen / Rook / Bishop
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
                        if (pinnedSquares[idx] == -1 || pinnedSquares[idx] == direction || pinnedSquares[idx] == (direction + 4) % 8)
                        {
                            if (inCheck) // only legal if capturing checking piece
                            {
                                if (target == checkingPiece)
                                {
                                    AddMove(new Move(idx, target));
                                }
                            }
                            else
                            {
                                AddMove(new Move(idx, target));
                            }
                        }
                        break;
                    }
                    // can move in direction of pin
                    if (pinnedSquares[idx] == -1 || pinnedSquares[idx] == direction || pinnedSquares[idx] == (direction + 4) % 8)
                    {
                        if (inCheck) // only legal if interposing
                        {
                            if (!inKnightOrPawnCheck && checkedSquares[target])
                            {
                                AddMove(new Move(idx, target));
                            }
                        }
                        else
                        {
                            AddMove(new Move(idx, target));
                        }
                    }
                }
            }
        }
        private void GenerateKnightMoves(int idx)
        {
            if (pinnedSquares[idx] != -1) return; // pinned knights cannot move
            foreach (int target in knightMoves[idx]) // checking all legal knight moves
            {
                if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                if (inCheck)
                {
                    if (checkingPiece == target) // capturing checking piece is legal
                    {
                        AddMove(new Move(idx, target));
                    }
                    if (!inKnightOrPawnCheck) // interposing moves are also legal
                    {
                        if (checkedSquares[target])
                        {
                            AddMove(new Move(idx, target));
                        }
                    }
                }
                else
                {
                    AddMove(new Move(idx, target));
                }
            }
        }
        private void GeneratePawnMoves(int idx)
        {
            int pinnedDir = pinnedSquares[idx];
            int target;
            bool isPromotion;
            if (!inKnightOrPawnCheck) // No interposing moves exist
            {
                // Pawn Forward
                target = idx + pawnForwardDistance;
                isPromotion = ((target >= 0 && target <= 7) || target >= 56 && target <= 63);
                bool isTwoForward = ((curTurnColor == Piece.White && idx >= 48 && idx <= 55) || (curTurnColor == Piece.Black && idx >= 8 && idx <= 15));
                if (Piece.Type(boardData[target]) == Piece.Empty) // can only move into empty spaces
                {
                    if (pinnedDir == -1 || pinnedDir == 0 || pinnedDir == 4) // can move toward or away from pinning piece
                    {
                        AddPawnMoves(idx, target, isPromotion);
                    }
                }
                // Pawn Two Forward
                if (isTwoForward)
                {
                    target = idx + (pawnForwardDistance * 2);
                    int oneForward = idx + pawnForwardDistance;
                    if (Piece.Type(boardData[oneForward]) == Piece.Empty && Piece.Type(boardData[target]) == Piece.Empty)
                    {
                        if (pinnedDir == -1 || pinnedDir == 4 || pinnedDir == 0) // can move toward or away from pinning piece
                        {
                            AddPawnMoves(idx, target, isPromotion, Move.Flag.PawnTwoForward);
                        }
                    }
                }
            }
            // Pawn Captures 
            bool leftCapture = curTurnColor == Piece.White ? idx % 8 != 0 : (idx + 1) % 8 != 0;
            bool rightCapture = curTurnColor == Piece.White ? (idx + 1) % 8 != 0 : idx % 8 != 0;
            bool enPassant = curTurnColor == Piece.White ? (idx >= 24 && idx <= 31) : (idx >= 32 && idx <= 39);
            // Left Capture
            target = idx + pawnLeftDistance;
            isPromotion = ((target >= 0 && target <= 7) || target >= 56 && target <= 63);
            if (leftCapture && Piece.Color(boardData[target]) == opponentTurnColor)
            {
                if (pinnedDir == -1 || pinnedDir == 7 || pinnedDir == 3)
                {
                    AddPawnMoves(idx, target, isPromotion);
                }
            }
            // Right Capture
            target = idx + pawnRightDistance;
            isPromotion = ((target >= 0 && target <= 7) || target >= 56 && target <= 63);
            if (rightCapture && Piece.Color(boardData[target]) == opponentTurnColor)
            {
                if (pinnedDir == -1 || pinnedDir == 1 || pinnedDir == 5)
                {
                    AddPawnMoves(idx, target, isPromotion);
                }
            }
            if (enPassant && prevMoves.Count > 0 && prevMoves.Last().MoveFlag == Move.Flag.PawnTwoForward)
            {
                // Left En Passant
                if (leftCapture)
                {
                    int enpassantTarget = idx + pawnLeftDistance - pawnForwardDistance;
                    target = idx + pawnLeftDistance;
                    isPromotion = ((target >= 0 && target <= 7) || target >= 56 && target <= 63);
                    if (enpassantTarget == prevMoves.Last().TargetSquare && Piece.Color(boardData[enpassantTarget]) == opponentTurnColor)
                    {
                        if (pinnedDir == -1 || pinnedDir == 3 || pinnedDir == 7)
                        {
                            if (kingSquare - (kingSquare % 8) == idx - (idx % 8))
                            {
                                if (!CheckEnPassantCheck(idx, enpassantTarget))
                                {
                                    AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                                }
                            }
                            else
                            {
                                AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                            }
                        }
                    }
                }
                // Right En Passant
                if (rightCapture)
                {
                    int enpassantTarget = idx + pawnRightDistance - pawnForwardDistance;
                    target = idx + pawnRightDistance;
                    isPromotion = ((target >= 0 && target <= 7) || target >= 56 && target <= 63);
                    if (enpassantTarget == prevMoves.Last().TargetSquare && Piece.Color(boardData[enpassantTarget]) == opponentTurnColor)
                    {
                        if (pinnedDir == -1 || pinnedDir == 1 || pinnedDir == 5)
                        {
                            if (kingSquare - (kingSquare % 8) == idx - (idx % 8))
                            {
                                if (!CheckEnPassantCheck(idx, enpassantTarget))
                                {
                                    AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                                }
                            }
                            else
                            {
                                AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                            }
                        }
                    }
                }
            }
        }
        private void AddPawnMoves(int idx, int target, bool isPromotion, int flag = Move.Flag.None)
        {
            if (inCheck)
            {
                if (checkedSquares[target] || target == checkingPiece || (flag == Move.Flag.EnPassantCapture && checkingPiece == (target - pawnForwardDistance)))
                {
                    if (isPromotion)
                    {
                        AddMove(new Move(idx, target, Move.Flag.PromoteToQueen));
                        AddMove(new Move(idx, target, Move.Flag.PromoteToRook));
                        AddMove(new Move(idx, target, Move.Flag.PromoteToKnight));
                        AddMove(new Move(idx, target, Move.Flag.PromoteToBishop));
                    }
                    else AddMove(new Move(idx, target, flag));
                }
            }
            else if (isPromotion)
            {
                AddMove(new Move(idx, target, Move.Flag.PromoteToQueen));
                AddMove(new Move(idx, target, Move.Flag.PromoteToRook));
                AddMove(new Move(idx, target, Move.Flag.PromoteToKnight));
                AddMove(new Move(idx, target, Move.Flag.PromoteToBishop));
            }
            else AddMove(new Move(idx, target, flag));
        }
        private bool CheckEnPassantCheck(int startSquare, int enpassantSquare)
        {
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            int[] directions = { 2, 6 }; // check left and right of king
            foreach (int dir in directions)
            {
                for (int dist = 0; dist < distToEdge[kingSquare][dir]; ++dist)
                {
                    int target = kingSquare + moveOffsets[dir] * (dist + 1); // num moves in a given directio
                    if (target == startSquare || target == enpassantSquare) continue; // these squares are getting skipped
                    if (Piece.Color(boardData[target]) == curTurnColor) break; // piece blocks check
                    if (Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        int type = Piece.Type(boardData[target]);
                        if (type == Piece.Queen || type == Piece.Rook) return true; // taking en passant causes check 
                        else break; // opponent piece blocks checks
                    }
                }
            }
            return false; // did not find a discovered en passant check
        }
        private void GenerateKingMoves(int idx)
        {
            // Directional Movement
            for (int direction = 0; direction < moveOffsets.Length; ++direction)
            {
                int target = idx + moveOffsets[direction];
                if (distToEdge[idx][direction] > 0 && attackedSquares[target] == false)
                {
                    if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                    AddMove(new Move(idx, target));
                }
            }
            // Castling
            if (inCheck) return; // Cannot castle out of check
            if (kingCastle)
            {
                // These squares cannot be attacked and must be empty 
                int[] castlingSquares = new int[2];
                castlingSquares[0] = curTurnColor == Piece.White ? 62 : 6;
                castlingSquares[1] = curTurnColor == Piece.White ? 61 : 5;
                if (Piece.Type(boardData[castlingSquares[0]]) == Piece.Empty && Piece.Type(boardData[castlingSquares[1]]) == Piece.Empty)
                {
                    if (!attackedSquares[castlingSquares[0]] && !attackedSquares[castlingSquares[1]])
                    {
                        AddMove(new Move(kingSquare, startingKingRook - 1, Move.Flag.Castling));
                    }
                }
            }
            if (queenCastle)
            {
                // These squares cannot be attacked and must be empty
                int[] castlingSquares = new int[3];
                castlingSquares[0] = curTurnColor == Piece.White ? 57 : 1; // may be attacked as king does not move through this square
                castlingSquares[1] = curTurnColor == Piece.White ? 58 : 2;
                castlingSquares[2] = curTurnColor == Piece.White ? 59 : 3;
                if (Piece.Type(boardData[castlingSquares[0]]) == Piece.Empty && Piece.Type(boardData[castlingSquares[1]]) == Piece.Empty && Piece.Type(boardData[castlingSquares[2]]) == Piece.Empty)
                {
                    if (!attackedSquares[castlingSquares[1]] && !attackedSquares[castlingSquares[2]])
                    {
                        AddMove(new Move(kingSquare, startingQueenRook + 2, Move.Flag.Castling));
                    }
                }
            }
        }
        private void LocatePieces()
        {

            Action shiftBitBoards = () =>
            {
                this.bb_white <<= 1;
                this.bb_black <<= 1;

                this.bb_queen <<= 1;
                this.bb_rook <<= 1;
                this.bb_bishop <<= 1;
                this.bb_knight <<= 1;
                this.bb_pawn <<= 1;
            };

            Action<int> addBitBoards = piece =>
            {
                switch (Piece.Color(piece))
                {
                    case (Piece.White):
                        {
                            this.bb_white |= 1;
                            break;
                        }
                    case (Piece.Black):
                        {
                            this.bb_black |= 1;
                            break;
                        }
                }
                switch (Piece.Type(piece))
                {
                    case (Piece.Pawn):
                        {
                            this.bb_pawn |= 1;
                            break;
                        }
                    case (Piece.Knight):
                        {
                            this.bb_knight |= 1;
                            break;
                        }
                    case (Piece.Bishop):
                        {
                            this.bb_bishop |= 1;
                            break;
                        }
                    case (Piece.Rook):
                        {
                            this.bb_rook |= 1;
                            break;
                        }
                    case (Piece.Queen):
                        {
                            this.bb_queen |= 1;
                            break;
                        }
                    case (Piece.King):
                        {
                            this.bb_king |= 1;
                            break;
                        }
                };
                shiftBitBoards();
            };

            for (int idx = 0; idx < boardData.Length; ++idx)
            {
                int piece = boardData[idx];
                if (piece == Piece.Empty)
                {
                    shiftBitBoards();
                    continue;
                }
                addBitBoards(piece);
                if(Piece.Type(piece) == Piece.King)
                {
                    if(Piece.Color(piece) == Piece.White) whiteKingSquare = idx;
                    if(Piece.Color(piece) == Piece.Black) blackKingSquare = idx;
                }
            }
            this.kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
        }
        private void CalculateAttackedSquares()
        {
            List<int> opponentPieces = curTurnColor == Piece.White ? blackPieces : whitePieces;
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            foreach (int idx in opponentPieces)
            {
                int type = Piece.Type(boardData[idx]);
                int color = Piece.Color(boardData[idx]);
                if (type == Piece.Queen || type == Piece.Rook || type == Piece.Bishop || type == Piece.King)
                {
                    for (int direction = 0; direction < moveOffsets.Length; ++direction)
                    {
                        if (type == Piece.Bishop && direction % 2 == 0) continue; // Bishops cannot move cardinally
                        if (type == Piece.Rook && direction % 2 == 1) continue; // Rooks cannot move diagonally
                        for (int dist = 0; dist < distToEdge[idx][direction]; ++dist)
                        {
                            if (type == Piece.King && dist > 0) break; // Kings only move one space
                            int target = idx + moveOffsets[direction] * (dist + 1); // num moves in a given direction
                            if (Piece.Color(boardData[target]) == curTurnColor)
                            {
                                attackedSquares[target] = true;
                                if (kingSquare == target)
                                {
                                    if (inCheck) inDoubleCheck = true;
                                    inCheck = true;
                                    checkDirection = direction;
                                    if (!inDoubleCheck) checkingPiece = idx;
                                    continue; // King cannot interpose
                                }
                                break; // Friendly pieces interpose
                            }
                            if (Piece.Color(boardData[target]) == opponentTurnColor)
                            {
                                attackedSquares[target] = true;
                                break; // Opponent pieces interpose
                            }
                            if (Piece.Type(boardData[target]) == Piece.Empty)
                            {
                                attackedSquares[target] = true;
                                continue;
                            }
                        }
                    }
                }
                else if (type == Piece.Knight)
                {
                    foreach (int target in knightMoves[idx]) // checking all legal knight moves
                    {
                        attackedSquares[target] = true;
                        if (kingSquare == target)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = idx;
                        }
                    }
                }
                else if (type == Piece.Pawn)
                {
                    if (idx < 8 || idx > 55) continue; // I llegal pawn position, dont consider
                    if (color == Piece.White) // Opponent is white
                    {
                        if (idx % 8 != 0) // Pawn can attack left
                        {
                            attackedSquares[idx + pawnLeftCapture] = true;
                            if (kingSquare == idx + pawnLeftCapture)
                            {
                                if (inCheck) inDoubleCheck = true;
                                inCheck = true;
                                inKnightOrPawnCheck = true;
                                if (!inDoubleCheck) checkingPiece = idx;
                            }
                        }
                        if ((idx + 1) % 8 != 0) // Pawn can attack right
                        {
                            attackedSquares[idx + pawnRightCapture] = true;
                            if (kingSquare == idx + pawnRightCapture)
                            {
                                if (inCheck) inDoubleCheck = true;
                                inCheck = true;
                                inKnightOrPawnCheck = true;
                                if (!inDoubleCheck) checkingPiece = idx;
                            }
                        }
                    }
                    else if (color == Piece.Black) // Opponent is black
                    {
                        if ((idx + 1) % 8 != 0) // Pawn can attack left
                        {
                            attackedSquares[idx - pawnLeftCapture] = true;
                            if (kingSquare == idx - pawnLeftCapture)
                            {
                                if (inCheck) inDoubleCheck = true;
                                inCheck = true;
                                inKnightOrPawnCheck = true;
                                if (!inDoubleCheck) checkingPiece = idx;
                            }
                        }
                        if (idx % 8 != 0) // Pawn can attack right
                        {
                            attackedSquares[idx - pawnRightCapture] = true;
                            if (kingSquare == idx - pawnRightCapture)
                            {
                                if (inCheck) inDoubleCheck = true;
                                inCheck = true;
                                inKnightOrPawnCheck = true;
                                if (!inDoubleCheck) checkingPiece = idx;
                            }
                        }
                    }
                }
            }
        }
        private void CalculatePinnedSquares()
        {
            for (int i = 0; i < pinnedSquares.Length; ++i) pinnedSquares[i] = -1;
            // iterate every direction outward from the king
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            for (int direction = 0; direction < moveOffsets.Length; ++direction)
            {
                int pinnablePieceIdx = -1;
                for (int dist = 0; dist < distToEdge[kingSquare][direction]; ++dist)
                {
                    int target = kingSquare + moveOffsets[direction] * (dist + 1); // num moves in a given direction
                    if (Piece.Color(boardData[target]) == curTurnColor) // piece able to be pinned
                    {
                        if (pinnablePieceIdx != -1) break; // two friendly pieces block pins
                        pinnablePieceIdx = target;
                    }
                    if (Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        if (!(pinnablePieceIdx != -1)) break; // no friendly pieces between opponent piece
                        int type = Piece.Type(boardData[target]);
                        if (type == Piece.Queen)
                        {
                            pinnedSquares[pinnablePieceIdx] = direction;
                            continue;
                        }
                        else if (type == Piece.Rook && direction % 2 == 0)
                        {
                            pinnedSquares[pinnablePieceIdx] = direction;
                            continue;
                        }
                        else if (type == Piece.Bishop && direction % 2 == 1)
                        {
                            pinnedSquares[pinnablePieceIdx] = direction;
                            continue;
                        }
                        else
                        {
                            break; // enemy piece that cannot pin
                        }
                    }
                }
            }
        }
        private void CalculateCheckedSquares()
        {
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            if (!inCheck || inKnightOrPawnCheck || inDoubleCheck) return;
            for (int dist = 0; dist < distToEdge[kingSquare][(checkDirection + 4) % 8]; ++dist) // check for interposing moves 
            {
                int target = kingSquare + moveOffsets[(checkDirection + 4) % 8] * (dist + 1); // scan in the direction of the check
                if (Piece.Type(boardData[target]) == Piece.Empty) // Square can be used to block check
                {
                    checkedSquares[target] = true;
                }
                if (checkingPiece == target) break; // reached the checking piece; no more interposing moves
            }
        }
        private void VerifyCastling()
        {
            startingKingSquare = curTurnColor == Piece.White ? startingWhiteKingSquare : startingBlackKingSquare;
            startingKingRook = curTurnColor == Piece.White ? startingWhiteKingRook : startingBlackKingRook;
            startingQueenRook = curTurnColor == Piece.White ? startingWhiteQueenRook : startingBlackQueenRook; ;
            if (kingSquare == startingKingSquare)
            {
                kingCastle = true;
                queenCastle = true;
            }
            if (kingCastle || queenCastle)
            {
                foreach (Move move in prevMoves)
                {
                    if (move.StartSquare == startingKingSquare) // King has moved, castling is illegal
                    {
                        kingCastle = false;
                        queenCastle = false;
                        return; // Castling cannot be legal anymore
                    }
                    // Kingside rook has moved or has been taken; king side castling is illegal
                    if (move.StartSquare == startingKingRook || move.TargetSquare == startingKingRook)
                    {
                        kingCastle = false;
                    }
                    // Queenside rook has moved or has been taken; king side castling is illegal
                    if (move.StartSquare == startingQueenRook || move.TargetSquare == startingQueenRook)
                    {
                        queenCastle = false;
                    }
                }
            }
        }
    }
}
