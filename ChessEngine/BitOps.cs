using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BitOps
{
    private const ulong deBruijn64 = 0x03f79d71b4cb0a89;
    private const ulong deBruijn64Rev = 0x03f79d71b4cb0a89;


    private static int[] index64 = {
        0,  1, 48,  2, 57, 49, 28,  3,
        61, 58, 50, 42, 38, 29, 17,  4,
        62, 55, 59, 36, 53, 51, 43, 22,
        45, 39, 33, 30, 24, 18, 12,  5,
        63, 47, 56, 27, 60, 41, 37, 16,
        54, 35, 52, 21, 44, 32, 23, 11,
        46, 26, 40, 15, 34, 20, 31, 10,
        25, 14, 19,  9, 13,  8,  7,  6

    };

    private static int[] index64Rev = {
    0, 47,  1, 56, 48, 27,  2, 60,
   57, 49, 41, 37, 28, 16,  3, 61,
   54, 58, 35, 52, 50, 42, 21, 44,
   38, 32, 29, 23, 17, 11,  4, 62,
   46, 55, 26, 59, 40, 36, 15, 53,
   34, 51, 20, 43, 31, 22, 10, 45,
   25, 39, 14, 33, 19, 30,  9, 24,
   13, 18,  8, 12,  7,  6,  5, 63
};


    public static int BitScanForward(ulong bb)
    {
        return index64Rev[((bb ^ (bb - 1)) * deBruijn64) >> 58];
    }

    public static int BitScanReverse(ulong bb)
    {
        bb |= bb >> 1;
        bb |= bb >> 2;
        bb |= bb >> 4;
        bb |= bb >> 8;
        bb |= bb >> 16;
        bb |= bb >> 32;
        return index64Rev[(bb * deBruijn64Rev) >> 58];
    }

    public static int PopulationCount(ulong bb)
    {
        int cnt = 0;
        while (bb != 0)
        {
            bb &= (bb - 1);
            cnt++;
        }

        return cnt;
    }
}
