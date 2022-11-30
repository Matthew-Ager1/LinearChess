using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Evaluation
{

    public static float[] WhitePawn = {
                                      0, 0,  0,   0,   0,  0,  0,  0,
                                      5, 10, 10, -20, -20, 10, 10, 5,
                                      5, -5, -10,  0,  0, -10, -5, 5,
                                      0,  10,  10, 20, 20, 10,  10,  0,
                                      5,  5,  10,  25, 25, 10, 5,  5,
                                      10, 10, 20, 25, 25, 20, 10, 10,
                                      50, 50, 50, 50, 50, 50, 50, 50,
                                      0,  0,  0,  0,  0,  0,  0,  0
    };

    public static float[] BlackPawn = {
                                      0, 0,  0,   0,   0,  0,  0,  0,
                                      50, 50, 50, 50, 50, 50, 50, 50,
                                      10, 10, 20,  25, 25, 20, 10, 10,
                                      5,  5,  10,  25, 25, 10,  5,  5,
                                      0,  10,  10,   20, 20, 10,   10,  0,
                                      5, -5, -10,  0,  0, -10, -5,  5,
                                      5, 10, 10, -20, -20, 10,  10, 5,
                                      0,  0,  0,  0,   0,  0,   0,  0
    };

    public static float[] Knight = {
                                      -50, -40, -30, -30, -30, -30, -40, -50,
                                      -40, -20,  0,  5,   5,    0,  -20, -40,
                                      -30,  5,   10, 15,  15,   10,  5,  -30,
                                      -30,  5,  15,  20,  20,   15,  5,  -30,
                                      -30,  5,  15,  20,  20,   15,  5,  -30,
                                      -30,  5,  10,  15,  15,   10,  5,  -30,
                                      -40, -20,  0,  5,   5,    0,  -20, -40,
                                      -50, -40, -30, -30, -30, -30, -40, -50
    };

    public static float[] Bishops = {
                                      -20, -10, -10, -10, -10, -10, -10, -20,
                                      -10,  0,   0,   0,   0,   0,   0,   0,
                                      -10,  0,   5,   10,  10,  5,   0,  -10,
                                      -10,  5,   10,  15,  15,  10,  5,  -10,
                                      -10,  0,   5,   10,  10,  5,   0,  -10,
                                      -10,  0,   10,  15,  15,  10,  5,  -10,
                                      -10,  0,   5,   10,  10,  5,   0,  -10,
                                      -20, -10, -10, -10, -10, -10, -10, -20
    };

    public static float[] Rooks = {
                                       -5,  -5,  0,  0,  0,  0,  -5,  -5,
                                      -5, 10, 10, 10, 10, 10, 10, -5,
                                      -5, 0,  5,  5,  5,  5,  0, -5,
                                      -5, 0,  5,  5,  5,  5,  0, -5,
                                      -5, 0,  5,  5,  5,  5,  0, -5,
                                      -5, 0,  5,  5,  5,  5,  0, -5,
                                      -5, 10, 10, 10, 10, 10, 10,-5,
                                       -5,  -5,  0,  0,  0,  0,  -5,  -5
    };

    public static float[] Queen = {
                                       -20,-10,-10, 0, 0,-10,-10,-20,
                                       -10,  0,  0,  0,  0,  0,  0,-10,
                                       -10,  0,  0,  0,  0,  0,  0,-10,
                                       -5,   0,  0,  0,  0,  0,  0, -5,
                                        -5,   0,  0,  0,  0,  0,  0, -5,
                                       -10,  0,  0,  0,  0,  0,  0,-10,
                                       -10,  0,  0,  0,  0,  0,  0,-10,
                                       -20,-10,-10, 0, 0,-10,-10,-20

    };

    public static float[] WhiteKing = {
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -20,-30,-30,-40,-40,-30,-30,-20,
                                        -10,-20,-20,-20,-20,-20,-20,-10,
                                         20, 20,  0,  0,  0,  0, 20, 20,
                                         20, 40, 10,  0,  0, 10, 50, 20

    };

    public static float[] BlackKing = {
                                         20, 40, 10,  0,  0, 10, 50, 20,
                                         20, 20,  0,  0,  0,  0, 20, 20,
                                        -10,-20,-20,-20,-20,-20,-20,-10,
                                        -20,-30,-30,-40,-40,-30,-30,-20,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30,
                                        -30,-40,-40,-50,-50,-40,-40,-30

    };


    public static float[] CenterControl =
    {
                                         0,  0,  0,  0,  0,  0,  0,  0,
                                         0,  0,  0,  0,  0,  0,  0,  0,
                                         0,  0, 10, 15, 15, 10,  0,  0,
                                         0,  0, 15, 30, 30, 15,  0,  0,
                                         0,  0, 15, 30, 30, 15,  0,  0,
                                         0,  0, 10, 15, 15, 10,  0,  0,
                                         0,  0,  0,  0,  0,  0,  0,  0,
                                         0,  0,  0,  0,  0,  0,  0,  0
    };

    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
     
    */

    public static float GetScoreOfBoard(ulong board, int tag)
    {
        float score = 0;

        switch (tag)
        {
            case 1:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += WhitePawn[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 7:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= BlackPawn[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;

            case 2:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += Knight[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 8:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= Knight[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;

            case 3:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += Bishops[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 9:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= Bishops[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;

            case 4:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += Rooks[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 10:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= Rooks[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;

            case 5:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += Queen[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 11:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= Queen[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 6:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score += WhiteKing[pos];
                    score += CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;
            case 12:
                while (board != 0)
                {
                    int pos = BitOps.BitScanForward(board);
                    score -= BlackKing[pos];
                    score -= CenterControl[pos];
                    board ^= (ulong)Mathf.Pow(2, pos);
                }
                break;

        }

        return score;

    }
}
