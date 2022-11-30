using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    /*Int Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
     * EMPTY = 0
    */

    public static int[] StartingBoard = new int[] 
    {
        "R"[0], "N"[0], "B"[0], "Q"[0], "K"[0], "B"[0], "N"[0], "R"[0],
        "P"[0], "P"[0], "P"[0], "P"[0], "P"[0], "P"[0], "P"[0], "P"[0],
        "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0],
        "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0],
        "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0],
        "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0], "-"[0],
        "p"[0], "p"[0], "p"[0], "p"[0], "p"[0], "p"[0], "p"[0], "p"[0],
        "r"[0], "n"[0], "b"[0], "q"[0], "k"[0], "b"[0], "n"[0], "r"[0],

    };

    public static int WHITE_KING = 6;
    public static int WHITE_KNIGHT = 2;
    public static int WHITE_BISHOP = 3;
    public static int WHITE_ROOK = 4;
    public static int WHITE_QUEEN = 5;
    public static int WHITE_PAWN = 1;

    public static int BLACK_KING = 12;
    public static int BLACK_KNIGHT = 8;
    public static int BLACK_BISHOP = 9;
    public static int BLACK_ROOK = 10;
    public static int BLACK_QUEEN = 11;
    public static int BLACK_PAWN = 7;

    public static int EMPTY = 0;

    public static ulong BLACK_PAWNS_START = 71776119061217280;
    public static ulong WHITE_PAWNS_START = 65280;

    public static ulong BLACK_KING_START = 1152921504606846976;
    public static ulong WHITE_KING_START = 16;

    public static ulong BLACK_QUEEN_START = 576460752303423488;
    public static ulong WHITE_QUEEN_START = 8;

    public static ulong BLACK_KNIGHTS_START = 4755801206503243776;
    public static ulong WHITE_KNIGHTS_START = 66;

    public static ulong BLACK_ROOKS_START = 9295429630892703744;
    public static ulong WHITE_ROOKS_START = 129;

    public static ulong BLACK_BISHOPS_START = 2594073385365405696;
    public static ulong WHITE_BISHOPS_START = 36;





}
