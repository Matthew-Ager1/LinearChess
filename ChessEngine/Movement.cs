using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//HERE SOME FACTZ
// 1. Left shift << increases position, Right shift >> decreases position -->> index 16 >> 2 = index 14 -->> index 16 << 2 = index 18

/* Dir Chart
 * North = 0
 * South = 1
 * East = 2
 * West = 3
 * North East = 4
 * North West = 5;
 * South West = 6;
 * South East = 7;
 */

public static class Movement
{
    private static ulong[][] RayAttacks = new ulong[8][];
    private static ulong[] KnightAttacks = new ulong[64];
    private static ulong[] KingAttacks = new ulong[64];

    public static void InitAttackPatterns()
    {
        InitRayAttacks();
        InitKnightAttacks();
        InitKingAttacks();
        //Debug.Log(KnightAttacks[1]);
    }

    private static void InitRayAttacks()
    {
        for (int i = 0; i < 8; i++)
        {
            RayAttacks[i] = new ulong[64];
            for (int j = 0; j < 64; j++)
            {
                ulong board = (ulong)Mathf.Pow(2, j);
                for (int k = 0; k < 8; k++)
                {
                    if (i == 0)
                    {
                        board |= no(board);
                    }
                    else if (i == 1)
                    {
                        board |= so(board);
                    }
                    else if (i == 2)
                    {
                        board |= ea(board);
                    }
                    else if (i == 3)
                    {
                        board |= we(board);
                    }
                    else if (i == 4)
                    {
                        board |= noEa(board);
                    }
                    else if (i == 5)
                    {
                        board |= noWe(board);
                    }
                    else if (i == 6)
                    {
                        board |= soWe(board);
                    }
                    else
                    {
                        board |= soEa(board);
                    }

                }
                RayAttacks[i][j] = (board ^ (ulong)Mathf.Pow(2, j));
            }

        }
    }

    private static void InitKnightAttacks()
    {
        for (int i = 0; i < 64; i++)
        {
            ulong board = (ulong)Mathf.Pow(2, i);
            KnightAttacks[i] = noNoEa(board) | noEaEa(board) | soEaEa(board) | soSoEa(board) | noNoWe(board) | noWeWe(board) | soWeWe(board) | soSoWe(board);
        }
    }

    private static void InitKingAttacks()
    {
        for (int i = 0; i < 64; i++)
        {
            ulong board = (ulong)Mathf.Pow(2, i);
            KingAttacks[i] = no(board) | so(board) | ea(board) | we(board) | soEa(board) | noEa(board) | soWe(board) | noWe(board);
        }
    }


    private static ulong notHFile = 9187201950435737471;
    private static ulong notGFile = 13816973012072644543;
    private static ulong notHGFile = notHFile & notGFile;
    private static ulong notAFile = 18374403900871474942;
    private static ulong notBFile = 18229723555195321596;
    private static ulong notABFile = notAFile & notBFile;

    private static ulong not1Rank = 18446744073709551360;
    private static ulong not8Rank = 72057594037927935;

    private static ulong onlyRank4 = 0x00000000FF000000;
    private static ulong onlyRank5 = 0x000000FF00000000;

    //Knight Movements
    public static ulong noNoEa(ulong bb) { return (bb << 17) & notAFile; }
    public static ulong noEaEa(ulong bb) { return (bb << 10) & notABFile; }
    public static ulong soEaEa(ulong bb) { return (bb >> 6) & notABFile; }
    public static ulong soSoEa(ulong bb) { return (bb >> 15) & notAFile; }
    public static ulong noNoWe(ulong bb) { return (bb << 15) & notHFile; }
    public static ulong noWeWe(ulong bb) { return (bb << 6) & notHGFile; }
    public static ulong soWeWe(ulong bb) { return (bb >> 10) & notHGFile; }
    public static ulong soSoWe(ulong bb) { return (bb >> 17) & notHFile; }

    public static ulong no(ulong bb) { return (bb << 8) & not1Rank; }
    public static ulong so(ulong bb) { return (bb >> 8) & not8Rank; }
    public static ulong we(ulong bb) { return (bb >> 1) & notHFile; }
    public static ulong ea(ulong bb) { return (bb << 1) & notAFile; }
    public static ulong noEa(ulong bb) { return (bb << 9) & not1Rank & notAFile; }
    public static ulong noWe(ulong bb) { return (bb << 7) & not1Rank & notHFile; }
    public static ulong soEa(ulong bb) { return (bb >> 7) & not8Rank & notAFile; }
    public static ulong soWe(ulong bb) { return (bb >> 9) & not8Rank & notHFile; }

    public static ulong wSinglePushPawns(ulong bb, ulong empty) { return no(bb) & empty; }
    public static ulong bSinglePushPawns(ulong bb, ulong empty) { return so(bb) & empty; }
    public static ulong wDoublePushPawns(ulong bb, ulong empty)
    {
        ulong rank4 = 0x00000000FF000000;
        ulong singlePush = wSinglePushPawns(bb, empty);
        return no(singlePush) & empty & rank4;
    }
    public static ulong bDoublePushPawns(ulong bb, ulong empty)
    {
        ulong rank5 = 0x000000FF00000000;
        ulong singlePush = bSinglePushPawns(bb, empty);
        return so(singlePush) & empty & rank5;
    }

    /*public static ulong KingAttacks(ulong bb) //OUT OF DATE | DEPRECEIATED!!!!!!!!!!!!
    {
        return ea(bb) | we(bb) | no(bb) | so(bb) | noEa(bb) | noWe(bb) | soEa(bb) | soWe(bb);
    }*/

    public static ulong GetPositiveRayAttack(ulong occupied, int dir, int square)
    {
        ulong attacks = RayAttacks[dir][square];
        ulong blocker = attacks & occupied;
        if (blocker != 0)
        {
            square = BitOps.BitScanForward(blocker);
            attacks ^= RayAttacks[dir][square];
        }

        return attacks;
    }

    public static ulong GetNegativeRayAttack(ulong occupied, int dir, int square)
    {
        ulong attacks = RayAttacks[dir][square];
        ulong blocker = attacks & occupied;
        if (blocker != 0)
        {
            square = BitOps.BitScanReverse(blocker);
            attacks ^= RayAttacks[dir][square];
        }

        return attacks;
    }

    private static ulong GetDiagonalAttacks(ulong occ, int square)
    {
        return (GetPositiveRayAttack(occ, 4, square) | GetNegativeRayAttack(occ, 6, square));
    }
    private static ulong GetAntiDiagonalAttacks(ulong occ, int square)
    {
        return (GetPositiveRayAttack(occ, 5, square) | GetNegativeRayAttack(occ, 7, square));
    }
    private static ulong GetRankAttacks(ulong occ, int square)
    {
        return (GetPositiveRayAttack(occ, 2, square) | GetNegativeRayAttack(occ, 3, square));
    }
    private static ulong GetFileAttacks(ulong occ, int square)
    {
        return (GetPositiveRayAttack(occ, 0, square) | GetNegativeRayAttack(occ, 1, square));
    }

    //Sliding attacks always include possible capture so make sure to check if same color
    public static ulong GetRookAttacks(ulong occ, int square)
    {
        return (GetRankAttacks(occ, square) | GetFileAttacks(occ, square));
    }
    public static ulong GetBishopAttacks(ulong occ, int square)
    {
        return (GetDiagonalAttacks(occ, square) | GetAntiDiagonalAttacks(occ, square));
    }
    public static ulong GetQueenAttacks(ulong occ, int square)
    {
        return (GetRookAttacks(occ, square) | GetBishopAttacks(occ, square));
    }

    public static ulong GetKnightAttacks(int square)
    {
        return KnightAttacks[square];
    }

    /*public static ulong GetPawnAttacks(bool white, ulong bb, ulong empty)
    {
        if (white)
        {
            return wSinglePushPawns(bb, empty);// | wDoublePushPawns(bb, empty); //excludes attacks
        }
        else
        {
            return bSinglePushPawns(bb, empty);// | bDoublePushPawns(bb, empty);
        }
    }*/

    public static ulong GetSinglePawnPush(ulong bb, ulong empty, ulong oppositeColor, bool white)
    {
        if (white)
        {
            //return GetDoublePawnPush(bb, empty, white);
            return (no(bb) & empty) | GetDoublePawnPush(bb, empty, white) | GetSinglePawnAttacks(bb, oppositeColor, white);
        }
        else
        {
            //return GetDoublePawnPush(bb, empty, white);
            return (so(bb) & empty) | GetDoublePawnPush(bb, empty, white) | GetSinglePawnAttacks(bb, oppositeColor, white);
        }
    }

    public static ulong GetKingAttacks(int startSquare)
    {
        return KingAttacks[startSquare];
    }

    private static ulong GetDoublePawnPush(ulong bb, ulong empty, bool white)
    {
        if (white)
        {
            return no(no(bb) & empty) & empty & onlyRank4;
        }
        else
        {
            return so(so(bb) & empty) & empty & onlyRank5;
        }
    }

    public static ulong GetSinglePawnAttacks(ulong bb, ulong opposite, bool white)
    {
        if (white)
        {
            return (noEa(bb) & opposite) | (noWe(bb) & opposite);
        }
        else
        {
            return (soEa(bb) & opposite) | (soWe(bb) & opposite);
        }
    }

    public static ulong GetSinglePawnEnPassant(ulong pawn, ulong enpassantTargets, bool white)
    {
        if (white)
        {
            return (noEa(pawn) & enpassantTargets) | (noWe(pawn) & enpassantTargets); 
        }
        else
        {
            return (soEa(pawn) & enpassantTargets) | (soWe(pawn) & enpassantTargets);
        }
    }

}

    