namespace Engine
{
    public readonly struct Move
    {

        public readonly struct Flag
        {
            public const int None = 0;
            public const int EnPassantCapture = 1;
            public const int Castling = 2;
            public const int PromoteToQueen = 3;
            public const int PromoteToKnight = 4;
            public const int PromoteToRook = 5;
            public const int PromoteToBishop = 6;
            public const int PawnTwoForward = 7;
        }

        readonly ushort moveValue;

        const ushort startSquareMask = 0b0000000000111111;
        const ushort targetSquareMask = 0b0000111111000000;
        const ushort flagMask = 0b1111000000000000;

        public Move(ushort moveValue)
        {
            this.moveValue = moveValue;
        }

        public Move(int from, int to)
        {
            moveValue = (ushort)(from | to << 6);
        }

        public Move(int from, int to, int flag)
        {
            moveValue = (ushort)(from | to << 6 | flag << 12);
        }

        public int StartSquare
        {
            get
            {
                return moveValue & startSquareMask;
            }
        }

        public int TargetSquare
        {
            get
            {
                return (moveValue & targetSquareMask) >> 6;
            }
        }

        public bool IsPromotion
        {
            get
            {
                int flag = MoveFlag;
                return flag == Flag.PromoteToQueen || flag == Flag.PromoteToRook || flag == Flag.PromoteToKnight || flag == Flag.PromoteToBishop;
            }
        }

        public int MoveFlag
        {
            get
            {
                return moveValue >> 12;
            }
        }

        public int PromotionPieceType
        {
            get
            {
                switch (MoveFlag)
                {
                    case Flag.PromoteToRook:
                        return Piece.Rook;
                    case Flag.PromoteToKnight:
                        return Piece.Knight;
                    case Flag.PromoteToBishop:
                        return Piece.Bishop;
                    case Flag.PromoteToQueen:
                        return Piece.Queen;
                    default:
                        return Piece.Empty;
                }
            }
        }

        public static Move InvalidMove
        {
            get
            {
                return new Move(0);
            }
        }

        public static bool SameMove(Move a, Move b)
        {
            return a.moveValue == b.moveValue;
        }

        public ushort Value
        {
            get
            {
                return moveValue;
            }
        }

        public bool IsInvalid
        {
            get
            {
                return moveValue == 0;
            }
        }
    }
}