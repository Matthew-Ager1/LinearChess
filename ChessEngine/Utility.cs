using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static int[][][] HistoryTable = new int[2][][];

    public static void InitHistoryTable()
    {
        for (int i = 0; i < 2; i++)
        {
            HistoryTable[i] = new int[64][];
        }
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                HistoryTable[i][j] = new int[64];
            }
        }
    }

    public static void TestSort()
    {
        testMoves = SortMoveList(testMoves, true);
        for (int i = 0; i < testMoves.Count; i++)
        {
            Debug.Log("Test Move Score = " + testMoves[i].capture);
        }
    }

    public static List<Move> testMoves = new List<Move>()
    {
        new Move(60, 53, 12, 6, 0, 0),
        new Move(60, 59, 12, 6, 49099, 0)
      
    };
    public static List<Move> SortMoveList(List<Move> moves, bool color)
    {
        List<Move> HistoryMoves = new List<Move>();
        List<Move> CaptureMoves = new List<Move>();

        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].capture == 0)
            {
                HistoryMoves.Add(moves[i]);
            }
            else
            {
                CaptureMoves.Add(moves[i]);
            }
        }

        Move[] movesArrCap = CaptureMoves.ToArray();
        Move[] movesArrHist = HistoryMoves.ToArray();

        if (movesArrCap.Length > 1)
        {
            SortMoveArrayScoreOffset(movesArrCap, 0, movesArrCap.Length - 1);
        }
        if (movesArrHist.Length > 1)
        {
            SortMoveArrayHistory(movesArrHist, 0, movesArrHist.Length - 1, color);
        }


        CaptureMoves.Clear();
        HistoryMoves.Clear();
        moves.Clear();

        for (int i = 0; i < movesArrHist.Length; i++)
        {
            HistoryMoves.Add(movesArrHist[i]);
        }

        bool inserted = false;
        for (int i = 0; i < movesArrCap.Length; i++)
        {
            if (!inserted && movesArrCap[i].capture <= 0)
            {
                inserted = true;
                moves.AddRange(HistoryMoves);
                HistoryMoves.Clear();
            }
            moves.Add(movesArrCap[i]);
        }       

        if (HistoryMoves.Count != 0)
        {
            moves.Reverse();
            HistoryMoves.Reverse();
            moves.AddRange(HistoryMoves);
            return moves;
        }

        moves.Reverse();

        return moves;
    }

    public static void SortMoveArrayScoreOffset(Move[] moves, int low, int up)
    {
        if (low >= up)
        {
            return;
        }
        int p = PartitionMoveArrayScoreOffset(moves, low, up);
        SortMoveArrayScoreOffset(moves, low, p - 1);
        SortMoveArrayScoreOffset(moves, p + 1, up);
    }

    public static int PartitionMoveArrayScoreOffset(Move[] moves, int low, int up)
    {
        Move pivot = moves[low];
        int i = low + 1;
        int j = up;

        while (i <= j)
        {
            while (moves[i].capture < pivot.capture && i < up)
            {
                i++;
            }
            while (moves[j].capture > pivot.capture)
            {
                j--;
            }

            if (i < j)
            {
                Move temp = moves[i];
                moves[i] = moves[j];
                moves[j] = temp;
                i++;
                j--;
            }
            else
            {
                break;
            }
        }

        moves[low] = moves[j];
        moves[j] = pivot;
        return j;
    }

    public static void SortMoveArrayHistory(Move[] moves, int low, int up, bool color)
    {
        if (low >= up)
        {
            return;
        }
        int p = PartitionMoveArrayHistory(moves, low, up, color);
        SortMoveArrayHistory(moves, low, p - 1, color);
        SortMoveArrayHistory(moves, p + 1, up, color);
    }

    public static int PartitionMoveArrayHistory(Move[] moves, int low, int up, bool color)
    {
        Move pivot = moves[low];
        int i = low + 1;
        int j = up;

        while (i <= j)
        {
            while (GetHistory(moves[i], color) < GetHistory(pivot, color) && i < up)
            {
                i++;
            }
            while (GetHistory(moves[j], color) > GetHistory(pivot, color))
            {
                j--;
            }

            if (i < j)
            {
                Move temp = moves[i];
                moves[i] = moves[j];
                moves[j] = temp;
                i++;
                j--;
            }
            else
            {
                break;
            }
        }

        moves[low] = moves[j];
        moves[j] = pivot;
        return j;
    }

    public static float GetHistory(Move m, bool color)
    {
        int side2move = color ? 0 : 1;
        return HistoryTable[side2move][m.startSquare][m.targetSquare];
    }


    public static List<Move> CullNonCaptures(List<Move> moves)
    {
        List<Move> captures = new List<Move>();
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].capture != 0)
            {
                captures.Add(moves[i]);
            }
        }
        return captures;
    }

    public static string Bitboard_To_FEN(Bitboard bb)
    {
        string FEN = "";

        char[] boardRep = new char[64];
        char[] tempRep = bb.CreateCharArr();

        for (int i = tempRep.Length - 1; i >= 0; i--)
        {
            boardRep[63 - i] = tempRep[i];
        }

        //We need to horizontally invert the array (if we image as an 8,8 2d array instead of a 64 1d array) so imma just do it with ducktape
        for (int i = 0; i < boardRep.Length; i+=8)
        {
            char pos1 = boardRep[i + 0];
            char pos2 = boardRep[i + 1];
            char pos3 = boardRep[i + 2];
            char pos4 = boardRep[i + 3];
            char pos5 = boardRep[i + 4];
            char pos6 = boardRep[i + 5];
            char pos7 = boardRep[i + 6];
            char pos8 = boardRep[i + 7];
            //reverse all positions
            boardRep[i + 0] = pos8;
            boardRep[i + 1] = pos7;
            boardRep[i + 2] = pos6;
            boardRep[i + 3] = pos5;
            boardRep[i + 4] = pos4;
            boardRep[i + 5] = pos3;
            boardRep[i + 6] = pos2;
            boardRep[i + 7] = pos1;
        }

        int deadSpaceCount = 0;

        for (int i = 0; i < boardRep.Length; i++)
        {
            if (i % 8 == 0 && i != 0)
            {
                if (deadSpaceCount != 0)
                {
                    FEN += deadSpaceCount;
                    deadSpaceCount = 0;
                }
                Debug.Log(i);
                FEN += "/";
            }

            if (boardRep[i] != 0)
            {
                if (deadSpaceCount != 0)
                {
                    FEN += deadSpaceCount;
                    deadSpaceCount = 0;
                }
                FEN += boardRep[i].ToString();
            }
            else
            {
                deadSpaceCount++;
            }
        }

        string turnStr = bb.ColorToMove ? " w " : " b ";
        FEN += turnStr;

        if (bb.castleState.WK == 0)
        {
            FEN += "K";
        }
        if (bb.castleState.WQ == 0)
        {
            FEN += "Q";
        }
        if (bb.castleState.BK == 0)
        {
            FEN += "k";
        }
        if (bb.castleState.BQ == 0)
        {
            FEN += "q";
        }

        FEN += " 0 1";

        return FEN;
    }

    public static Bitboard FEN_To_Bitboard(string FEN) //i guess i hate myself this should be fun
    {
        bool main = true;
        int file = 7;
        int rank = 0;

        Bitboard bb = new Bitboard();
        bb.Clear();
        bb.castleState = new CastleState(1, 1, 1, 1);

        for (int i = 0; i < FEN.Length; i++)
        {
            if (main)
            {
                if (FEN[i] == 32 && main) // is " "
                {
                    main = false;
                }
                else if (FEN[i] > 47 && FEN[i] < 58) //is digit of 0-9
                {
                    rank += int.Parse(FEN[i].ToString());
                    continue;
                }
                else if (FEN[i] == 47) // is "/"
                {
                    file--;
                    rank = 0;
                    continue;
                }
                else //is letter
                {
                    AddPiece(FEN[i].ToString(), bb, rank + file * 8);

                    rank++;
                    continue;
                }

            }
            else
            {
                if (FEN[i] == "w"[0])
                {
                    bb.ColorToMove = true;
                }
                if (FEN[i] == "b"[0])
                {
                    bb.ColorToMove = false;
                }
                if (FEN[i] == "K"[0])
                {
                    bb.castleState.WK = 0;
                }
                if (FEN[i] == "Q"[0])
                {
                    bb.castleState.WQ = 0;
                }
                if (FEN[i] == "k"[0])
                {
                    bb.castleState.BK = 0;
                }
                if (FEN[i] == "q"[0])
                {
                    bb.castleState.BQ = 0;
                }
            }
        }
        bb.SetBitBoards(-1); //no exeption
        bb.SetState();
        return bb;
    }

    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
     index is (tag - 1)
    */

    public static void AddPiece(string piece, Bitboard bb, int pos)
    {
        switch (piece)
        {
            case "P":
                bb.AddPieceToBoard(0, pos);
                return;
            case "N":
                bb.AddPieceToBoard(1, pos);
                return;
            case "B":
                bb.AddPieceToBoard(2, pos);
                return;
            case "R":
                bb.AddPieceToBoard(3, pos);
                return;
            case "Q":
                bb.AddPieceToBoard(4, pos);
                return;
            case "K":
                bb.AddPieceToBoard(5, pos);
                return;
            case "p":
                bb.AddPieceToBoard(6, pos);
                return;
            case "n":
                bb.AddPieceToBoard(7, pos);
                return;
            case "b":
                bb.AddPieceToBoard(8, pos);
                return;
            case "r":
                bb.AddPieceToBoard(9, pos);
                return;
            case "q":
                bb.AddPieceToBoard(10, pos);
                return;
            case "k":
                bb.AddPieceToBoard(11, pos);
                return;
        }
    }

}
