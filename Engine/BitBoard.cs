using System;

namespace Engine
{
    public static class BitBoard
    {
        public static ulong convertToBitBoard(int[] boardData)
        {
            ulong bitboard = 0;
            foreach (int piece in boardData)
            {
                ulong bit = piece != Piece.Empty ? (ulong)0b0001 : (ulong)0b0000;
                bitboard |= bit;
                bitboard <<= 1;
            }
            return bitboard;
        }
    }
}
