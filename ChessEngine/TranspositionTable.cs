using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TranspositionTable
{
    public static Hashtable table = new Hashtable();

    public static int[,] PieceSquareValues = new int[64, 12];

    static int transpositionTableSize = (int)Mathf.Pow(10, 18);

    public static void AddValueToTable(long key, ttEntry value)
    {
        if (table.ContainsKey(key))
        {
            ttEntry curEntry = (ttEntry)table[key];
            if (curEntry.depth <= value.depth)
            {
                table.Remove(key);
            }
            else
            {
                return;
                //Debug.LogError()
            }


        }
        table.Add(key, value);
    }

    public static ttEntry CreateHashEntry(Bitboard bb, int flag, Move move, float eval, int depth)
    {
        long zobristKey = bb.ZorbristHash;//GetZobristKey(bb);
        //Debug.Log(zobristKey);
        return new ttEntry(zobristKey, depth, flag, eval, move);
    }

    public static ttEntry GetEntry(long key)
    {
        if (table[key] != null)
        {
            return (ttEntry)table[key];
        }
        else
        {
            return new ttEntry(-1, -1, -1, -1, new Move(-1, -1, -1, -1, -1));
        }

    }

    public static void InitilizeHashTable()
    {
        for (int x = 0; x < PieceSquareValues.GetLength(0); x++)
        {
            for (int y = 0; y < PieceSquareValues.GetLength(1); y++)
            {
                PieceSquareValues[x, y] = Random.Range(1, 100000);
            }
        }
    }

    public static long GetZobristKey(Bitboard bb)
    {
        long curKey = 0;
        ulong[] boards = bb.DuplicateBoardArr();
        for (int i = 0; i < boards.Length; i++)
        {
            while (boards[i] != 0)
            {
                int pos = BitOps.BitScanForward(boards[i]);
                if (curKey == 0)
                {
                    curKey = PieceSquareValues[pos, i];
                }
                else
                {
                    curKey ^= PieceSquareValues[pos, i];
                }
                boards[i] ^= (ulong)Mathf.Pow(2, pos);
            }
        }

        return curKey;
    }

    public static void ClearTranspoitionTable()
    {
        table.Clear();
    }

    public static bool BoardsAreSame(ulong[] set1, ulong[] set2)
    {
        for (int i = 0; i < set1.Length; i++)
        {
            if (set1[i] != set2[i])
            {
                return false;
            }
        }
        return true;
    }
}

public struct ttEntry
{
    public long ZobristKey;
    public int depth;
    public int flag;
    public float eval;
    public Move move;
    //public ulong[] Boards;

    public ttEntry(long ZobristKey_, int depth_, int flag_, float eval_, Move move_)//, ulong[] Boards_)
    {
        ZobristKey = ZobristKey_;
        depth = depth_;
        flag = flag_;
        eval = eval_;
        move = move_;
    }
}
