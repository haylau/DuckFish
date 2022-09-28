namespace Engine
{
    using System.Collections.Generic;
    using static MoveData;
    public class MoveGenerator
    {
        public int curTurnColor;
        public int opponentTurnColor;
        public List<Move> prevMoves;
        public List<Move> possibleMoves;
        public int[] boardData;
        public List<int> whitePieces;
        public List<int> blackPieces;
        public bool[] attackedSquares;
        public bool[] checkedSquares;
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
            if (prevMoves is List<Move>)
            {
                this.prevMoves = prevMoves;
            }
            else
            {
                this.prevMoves = new();
            }
            this.curTurnColor = curTurnColor;
            this.opponentTurnColor = curTurnColor == Piece.White ? Piece.Black : Piece.White;
            this.pawnForwardDistance = curTurnColor == Piece.White ? pawnForward : -1 * pawnForward;
            this.pawnLeftDistance = curTurnColor == Piece.White ? pawnLeftCapture : -1 * pawnLeftCapture;
            this.pawnRightDistance = curTurnColor == Piece.White ? pawnRightCapture : -1 * pawnRightCapture;
            this.possibleMoves = new();
            this.attackedSquares = new bool[64];
            this.checkedSquares = new bool[64];
            this.pinnedSquares = new int[64];
            this.checkingPiece = -1;
            this.whitePieces = new();
            this.blackPieces = new();
            LocatePieces();
            CalculateAttackedSquares();
            CalculatePinnedSquares();
            CalculateCheckedSquares();
            VerifyCastling();
            GenerateMoves();
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
                        possibleMoves.Add(new Move(kingSquare, target));
                    }
                }
                return;
            }
            foreach (int idx in (curTurnColor == Piece.White) ? whitePieces : blackPieces)
            {
                int piece = boardData[idx];
                int type = Piece.Type(piece);
                switch (type)
                {
                    case Piece.Queen:
                    case Piece.Rook:
                    case Piece.Bishop:
                        {
                            GenerateLinearMoves(idx, piece, type);
                            break;
                        }
                    case Piece.Knight:
                        {
                            GenerateKnightMoves(idx);
                            break;
                        }
                    case Piece.Pawn:
                        {
                            GeneratePawnMoves(idx);
                            break;
                        }
                    case Piece.King:
                        {
                            GenerateKingMoves(idx);
                            break;
                        }
                }
            }
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
                                    possibleMoves.Add(new Move(idx, target));
                                }
                            }
                            else
                            {
                                possibleMoves.Add(new Move(idx, target));
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
                                possibleMoves.Add(new Move(idx, target));
                            }
                        }
                        else
                        {
                            possibleMoves.Add(new Move(idx, target));
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
                        possibleMoves.Add(new Move(idx, target));
                    }
                    if (!inKnightOrPawnCheck) // interposing moves are also legal
                    {
                        if (checkedSquares[target])
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                    }
                }
                else
                {
                    possibleMoves.Add(new Move(idx, target));
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
                                if (!CheckEnPassantCheck(idx, enpassantTarget)) AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                            }
                            else AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
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
                                if (!CheckEnPassantCheck(idx, enpassantTarget)) AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                            }
                            else AddPawnMoves(idx, target, isPromotion, Move.Flag.EnPassantCapture);
                        }
                    }
                }
            }
        }
        private void AddPawnMoves(int idx, int target, bool isPromotion, int flag = Move.Flag.None)
        {
            if (inCheck)
            {
                if (checkedSquares[target] || target == checkingPiece || (flag == Move.Flag.EnPassantCapture && checkingPiece == (target - pawnForwardDistance)) )
                {
                    if (isPromotion)
                    {
                        this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToQueen));
                        this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToRook));
                        this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToKnight));
                        this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToBishop));
                    }
                    else possibleMoves.Add(new Move(idx, target, flag));
                }
            }
            else if (isPromotion)
            {
                this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToQueen));
                this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToRook));
                this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToKnight));
                this.possibleMoves.Add(new Move(idx, target, Move.Flag.PromoteToBishop));
            }
            else possibleMoves.Add(new Move(idx, target, flag));
        }
        private bool CheckEnPassantCheck(int startSquare, int enpassantSquare)
        {
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            int direction = -1;
            for (int i = 0; i < 2; ++i)
            {
                // check left and right of king
                if (direction == -1) direction = 2;
                else if (direction == 2) direction = 6;
                for (int dist = 0; dist < distToEdge[kingSquare][direction]; ++dist)
                {
                    int target = kingSquare + moveOffsets[direction] * (dist + 1); // num moves in a given directio
                    if (target == startSquare || target == enpassantSquare) continue; // these squares are getting skipped
                    if (Piece.Color(boardData[target]) == curTurnColor) return false; // piece blocks check
                    if (Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        int type = Piece.Type(boardData[target]);
                        if (type == Piece.Queen || type == Piece.Rook) return true;
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
                    possibleMoves.Add(new Move(idx, target));
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
                        possibleMoves.Add(new Move(kingSquare, startingKingRook - 1, Move.Flag.Castling));
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
                        possibleMoves.Add(new Move(kingSquare, startingQueenRook + 2, Move.Flag.Castling));
                    }
                }
            }
        }
        private void LocatePieces()
        {
            for (int idx = 0; idx < boardData.Length; ++idx)
            {
                if (Piece.Color(boardData[idx]) == Piece.White)
                {
                    whitePieces.Add(idx);
                    if (Piece.Type(boardData[idx]) == Piece.King) whiteKingSquare = idx;
                }
                else if (Piece.Color(boardData[idx]) == Piece.Black)
                {
                    blackPieces.Add(idx);
                    if (Piece.Type(boardData[idx]) == Piece.King) blackKingSquare = idx;
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
