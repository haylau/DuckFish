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
        public bool[] pinnedSquares;
        public int whiteKingSquare;
        public int blackKingSquare;
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
            possibleMoves = new();
            attackedSquares = new bool[64];
            pinnedSquares = new bool[64];
            checkingPiece = 0;
            whitePieces = new();
            blackPieces = new();
            LocatePieces();
            CalculateAttackedSquares();
            GenerateMoves();
        }

        public void GenerateMoves() // generates a list of all current legal moves
        {
            foreach (int idx in (curTurnColor == Piece.White) ? whitePieces : blackPieces)
            {
                // in check check ()
                // double check (only king moves are legal)
                // knight check (no legal interposing moves)
                if (pinnedSquares[idx]) continue;
                int piece = boardData[idx];
                int color = Piece.Color(piece);
                int type = Piece.Type(piece);

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
                else if (type == Piece.Knight)
                {
                    foreach (int target in knightMoves[idx]) // checking all legal knight moves
                    {
                        if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                        possibleMoves.Add(new Move(idx, target));
                    }
                }
                else if (type == Piece.Pawn)
                {
                    if (curTurnColor == Piece.White) // Can only move up
                    {
                        if (idx < 8) throw new Exception("Illegal Pawn position");
                        // Forward
                        int target = idx + pawnForward;
                        if (Piece.Type(boardData[target]) == Piece.Empty)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        // Adjacent capture
                        target = idx + pawnLeftCapture;
                        if (idx % 8 != 0 && Piece.Color(boardData[target]) == opponentTurnColor)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        target = idx + pawnRightCapture;
                        if ((idx + 1) % 8 != 0 && Piece.Color(boardData[target]) == opponentTurnColor)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        // Forward Twice and En Passant only legal on original pawn positions
                        if (idx >= 48 && idx <= 55)
                        {
                            // Forward twice
                            // Can only move into/through empty tiles
                            if (Piece.Type(boardData[idx + pawnForward]) == Piece.Empty && Piece.Type(boardData[idx + pawnTwoForward]) == Piece.Empty)
                            {
                                possibleMoves.Add(new Move(idx, idx + pawnTwoForward, Move.Flag.PawnTwoForward));
                            }
                        }
                        // En Passant
                        if (prevMoves.Count > 0 && prevMoves.Last().MoveFlag == Move.Flag.PawnTwoForward && idx >= 24 && idx <= 31) // Previous move was a double pawn push
                        {
                            int enPassantTarget = prevMoves.Last().TargetSquare;
                            // Adjacent capture
                            target = idx + pawnLeftEnPassant;
                            if (target == enPassantTarget && Piece.Color(boardData[target]) == opponentTurnColor)
                            {
                                possibleMoves.Add(new Move(idx, idx + pawnLeftCapture, Move.Flag.EnPassantCapture));
                            }
                            target = idx + pawnRightEnPassant;
                            if (target == enPassantTarget && Piece.Color(boardData[target]) == opponentTurnColor)
                            {
                                possibleMoves.Add(new Move(idx, idx + pawnRightCapture, Move.Flag.EnPassantCapture));
                            }
                        }
                    }
                    if (curTurnColor == Piece.Black) // Can only move up
                    {
                        if (idx > 55) throw new Exception("Illegal Pawn position");
                        // Forward
                        int target = idx - pawnForward;
                        if (Piece.Type(boardData[target]) == Piece.Empty)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        // Adjacent capture
                        target = idx - pawnLeftCapture;
                        if (idx % 8 != 0 && Piece.Color(boardData[target]) == opponentTurnColor)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        target = idx - pawnRightCapture;
                        if ((idx - 1) % 8 != 0 && Piece.Color(boardData[target]) == opponentTurnColor)
                        {
                            possibleMoves.Add(new Move(idx, target));
                        }
                        // Forward Twice and En Passant only legal on original pawn positions
                        if (idx >= 8 && idx <= 15)
                        {
                            // Forward twice
                            // Can only move into/through empty tiles
                            if (Piece.Type(boardData[idx - pawnForward]) == Piece.Empty && Piece.Type(boardData[idx - pawnTwoForward]) == Piece.Empty)
                            {
                                possibleMoves.Add(new Move(idx, idx - pawnTwoForward, Move.Flag.PawnTwoForward));
                            }
                        }
                        // En Passant
                        if (prevMoves.Count > 0 && prevMoves.Last().MoveFlag == Move.Flag.PawnTwoForward && idx >= 32 && idx <= 39) // Previous move was a double pawn push
                        {
                            int enPassantTarget = prevMoves.Last().TargetSquare;
                            // Adjacent capture
                            target = idx - pawnLeftEnPassant;
                            if (target == enPassantTarget && Piece.Color(boardData[target]) == opponentTurnColor)
                            {
                                possibleMoves.Add(new Move(idx, idx - pawnLeftCapture, Move.Flag.EnPassantCapture));
                            }
                            target = idx - pawnRightEnPassant;
                            if (target == enPassantTarget && Piece.Color(boardData[target]) == opponentTurnColor)
                            {
                                possibleMoves.Add(new Move(idx, idx - pawnRightCapture, Move.Flag.EnPassantCapture));
                            }
                        }
                    }
                }
                else if (type == Piece.King)
                {
                    for (int direction = 0; direction < moveOffsets.Length; ++direction)
                    {
                        int target = idx + moveOffsets[direction];
                        if (distToEdge[idx][direction] > 1 && attackedSquares[target] == false)
                        {
                            if (Piece.Color(boardData[target]) == curTurnColor) continue; // cannot capture own piece
                            possibleMoves.Add(new Move(idx, target));
                        }
                    }
                }
            }
            // Remove illegal moves due to check
            if (inCheck) // Only captures of checking piece, interposing, or moving the king is legal
            {
                int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
                List<Move> legalMoves = new();
                foreach (Move move in possibleMoves)
                {
                    if (inDoubleCheck) // no legal captures exist
                    {
                        if (Piece.Type(boardData[move.StartSquare]) == Piece.King) legalMoves.Add(move); // King move
                    }
                    else if (inKnightOrPawnCheck) // no legal interposing moves exist 
                    {
                        if (Piece.Type(boardData[move.StartSquare]) == Piece.King) legalMoves.Add(move); // King move
                        if (move.TargetSquare == checkingPiece) legalMoves.Add(move); // Capture of checking piece
                    }
                    else
                    {
                        if (Piece.Type(boardData[move.StartSquare]) == Piece.King) legalMoves.Add(move); // King move
                        if (move.TargetSquare == checkingPiece) legalMoves.Add(move); // Capture of checking piece
                        for (int dist = 0; dist < distToEdge[kingSquare][checkDirection]; ++dist) // check for interposing moves 
                        {
                            int target = kingSquare + moveOffsets[checkDirection] * (dist + 1); // scan in the direction of the check
                            if (target == move.TargetSquare) // Interposing move
                            {
                                legalMoves.Add(move);
                                break;
                            }
                            if (checkingPiece == target) break; // reached the checking piece; no more interposing moves

                        }
                    }
                }
                possibleMoves = legalMoves;
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
        }

        private void CalculateAttackedSquares()
        {
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            // Iterate through each square to see if an enemy piece attacks it
            // Skips to next idx when an attacking piece is found
            for (int idx = 0; idx < boardData.Length; ++idx)
            {
                // #TODO CHECK FOR EN PASSANT
                // Pawns
                if (curTurnColor == Piece.White && idx > 7)
                {
                    // Left-Facing Pawn 
                    int target = idx + pawnLeftCapture;
                    if (idx % 8 != 0 && target >= 0 && target < 64 && Piece.Color(boardData[target]) == opponentTurnColor && Piece.Type(boardData[target]) == Piece.Pawn)
                    {
                        attackedSquares[idx] = true;
                        if (kingSquare == idx)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = target;
                        }
                        else continue;
                    }
                    // Right-Facing Pawn 
                    target = idx + pawnRightCapture;
                    if ((idx + 1) % 8 != 0 && target >= 0 && target < 64 && Piece.Color(boardData[target]) == opponentTurnColor && Piece.Type(boardData[target]) == Piece.Pawn)
                    {
                        attackedSquares[idx] = true;
                        if (kingSquare == idx)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = target;
                        }
                        else continue;
                    }
                }
                if (curTurnColor == Piece.Black && idx < 56)
                {
                    // Left-Facing Pawn 
                    int target = idx - pawnLeftCapture;
                    if (idx % 8 != 0 && target >= 0 && target < 64 && Piece.Color(boardData[target]) == opponentTurnColor && Piece.Type(boardData[target]) == Piece.Pawn)
                    {
                        attackedSquares[idx] = true;
                        if (kingSquare == idx)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = target;
                        }
                        else continue;
                    }
                    // Right-Facing Pawn 
                    target = idx - pawnRightCapture;
                    if ((idx - 1) % 8 != 0 && target >= 0 && target < 64 && Piece.Color(boardData[target]) == opponentTurnColor && Piece.Type(boardData[target]) == Piece.Pawn)
                    {
                        attackedSquares[idx] = true;
                        if (kingSquare == idx)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = target;
                        }
                        else continue;
                    }
                }
                if (attackedSquares[idx]) continue;
                // Directional Pieces
                for (int direction = 0; direction < moveOffsets.Length; ++direction)
                {
                    int target = idx + moveOffsets[direction];
                    if (!(target >= 0 && target <= 63)) continue;
                    // King
                    if (distToEdge[idx][direction] > 0 && Piece.Type(boardData[target]) == Piece.King && Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        attackedSquares[idx] = true;
                        break;
                    }
                    // Queen Rook Bishop
                    for (int dist = 0; dist < distToEdge[idx][direction]; ++dist)
                    {
                        target = idx + moveOffsets[direction] * (dist + 1);
                        int color = Piece.Color(boardData[target]);
                        int type = Piece.Type(boardData[target]);
                        if (color == curTurnColor) break; // friendly piece blocks any attacks
                        if (color == opponentTurnColor) // enemy piece found
                        {
                            if (type == Piece.Queen)
                            {
                                attackedSquares[idx] = true;
                                if (kingSquare == idx)
                                {
                                    if (inCheck) inDoubleCheck = true;
                                    inCheck = true;
                                    checkDirection = direction;
                                    if (!inDoubleCheck) checkingPiece = target;
                                }
                                break;
                            }
                            else if (type == Piece.Rook && direction % 2 == 0)
                            {
                                attackedSquares[idx] = true;
                                if (kingSquare == idx)
                                {
                                    if (inCheck) inDoubleCheck = true;
                                    inCheck = true;
                                    checkDirection = direction;
                                    if (!inDoubleCheck) checkingPiece = target;
                                }
                                break;
                            }
                            else if (type == Piece.Bishop && direction % 2 == 1)
                            {
                                attackedSquares[idx] = true;
                                if (kingSquare == idx)
                                {
                                    if (inCheck) inDoubleCheck = true;
                                    inCheck = true;
                                    checkDirection = direction;
                                    if (!inDoubleCheck) checkingPiece = target;
                                }
                                break;
                            }
                            else break; // enemy piece is not a queen rook or bishop therefore blocks attacks
                        }
                    }
                }
                if (attackedSquares[idx]) continue;
                // Knight Moves
                foreach (int target in knightMoves[idx]) // checking all legal knight moves
                {
                    if (Piece.Type(boardData[target]) == Piece.Knight && Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        attackedSquares[idx] = true;
                        if (kingSquare == idx)
                        {
                            if (inCheck) inDoubleCheck = true;
                            inCheck = true;
                            inKnightOrPawnCheck = true;
                            if (!inDoubleCheck) checkingPiece = target;
                        }
                        else break;
                    }
                }
                if (attackedSquares[idx]) continue;
                // No enemy pieces attack this square
                attackedSquares[idx] = false;
            }
        }
        private void CalculatePinnedSquares()
        {
            int kingSquare = (curTurnColor == Piece.White) ? whiteKingSquare : blackKingSquare;
            for (int direction = 0; direction < moveOffsets.Length; ++direction)
            {
                bool pinnablePiece = false;
                int pinnablePieceIdx;
                for (int dist = 0; dist < distToEdge[kingSquare][direction]; ++dist)
                {
                    int target = kingSquare + moveOffsets[direction] * (dist + 1); // num moves in a given direction
                    if (Piece.Color(boardData[target]) == curTurnColor) // piece able to be pinned
                    {
                        pinnablePiece = true;
                        pinnablePieceIdx = target;
                    }
                    if (Piece.Color(boardData[target]) == opponentTurnColor)
                    {
                        if (!pinnablePiece) break; // no friendly pieces between opponent piece
                        int type = Piece.Type(boardData[target]);
                        if (type == Piece.Queen)
                        {
                            pinnedSquares[target] = true;
                            continue;
                        }
                        else if (type == Piece.Rook && direction % 2 == 0)
                        {
                            pinnedSquares[target] = true;
                            continue;
                        }
                        else if (type == Piece.Bishop && direction % 2 == 1)
                        {
                            pinnedSquares[target] = true;
                            continue;
                        }
                    }
                }
            }
        }
    }
}