using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChessEngine
{
    static int transpositionTableSize = (int)Mathf.Pow(10, 18);

    public static Move emptyPV = new Move(-1, -1, -1, -1, -1);
    public static int count = 0;

    public static int maxDepth = 0;

    /*public static MoveScore NegamaxItterativeDeepening(Bitboard bb, int depth, bool color)
    {
        MoveScore best = new MoveScore();
        for (int i = 1; i <= depth; i++)
        {
            if (i == 1)
            {
                best = Negamax(bb, i, color, -Mathf.Infinity, Mathf.Infinity, emptyPV);
            }
            else
            {
                best = Negamax(bb, i, color, -Mathf.Infinity, Mathf.Infinity, best.move);
            }
            
        }
        return best;
    }

    public static MoveScore NegaScoutItterativeDeepening(Bitboard bb, int depth, bool color)
    {
        MoveScore best = new MoveScore();
        for (int i = 1; i <= depth; i++)
        {
            best = NegaScout(bb, i, color, -Mathf.Infinity, Mathf.Infinity);
            /*if (i == 1)
            {
                best = NegaScout(bb, i, color, -Mathf.Infinity, Mathf.Infinity);
            }
            else
            {
                best = NegaScout(bb, i, color, -Mathf.Infinity, Mathf.Infinity);
            }

        }
        return best;
    }




    public static MoveScore Negamax(Bitboard bb, int depth, bool color, float alpha, float beta, Move pvMove)
    {
        bb.ColorToMove = color;

        float alphaOrig = alpha;
        count++;
        

        if (depth == 0)
        {
            MoveScore ms = QuiesceSearch(bb, alpha, beta, color);
            //ms.score *= -1;
            return ms;
            //return new MoveScore(bb.Evaluate(), new Move());
        }

        Move bestSoFar = new Move();

        //Transposition Control
        //___________________________________________________________________________________________________________________________________________
        ttEntry entry = TranspositionTable.GetEntry((int)(bb.ZorbristHash % transpositionTableSize));
        if (entry.depth != -1 && entry.depth >= depth) //check depth but all my depth is the same for rn: && ttEntry.depth ≥ depth
        {
            if (entry.flag == 0) //flag is exact
            {
                //Debug.Log("Used Tranpositions Table");
                return new MoveScore(entry.eval, entry.move);
            }
            else if (entry.flag == 1) //flag is lowerbound
            {
                if (alpha < entry.eval)
                {
                    //Debug.Log("Used Tranpositions Table");
                    alpha = entry.eval;
                    bestSoFar = entry.move;
                }
            }
            else if (entry.flag == 2) //flag is upperbound
            {
                if (beta > entry.eval)
                {
                    //Debug.Log("Used Tranpositions Table");
                    beta = entry.eval;
                    bestSoFar = entry.move;
                }
            }

            if (alpha >= beta)
            {
                //.Log("Used Tranpositions Table");
                return new MoveScore(entry.eval, entry.move);
            }
        }
        //___________________________________________________________________________________________________________________________________________
        
        List<Move> moves = bb.GenerateMoves(color, false);
        moves = Utility.SortMoveList(moves, color);

        if (pvMove.startSquare != -1)
        {
            moves.Insert(0, pvMove);
        }

        for (int i = 0; i < moves.Count; i++)
        {
            Bitboard newBB = bb.Duplicate();
            newBB.MakeMove(moves[i]);

            MoveScore score = (Negamax(newBB, depth - 1, !color, -beta, -alpha, emptyPV));
            score.score *= -1;

            if (score.score >= beta)
            {
                return new MoveScore(beta, moves[i]);
            }
            if (score.score > alpha)
            {
                alpha = score.score;
                bestSoFar = moves[i];
            }
        }

        int flag = 0;
        if (alpha <= alphaOrig)
        {
            flag = 2;
        }
        if (alpha >= beta)
        {
            flag = 1;
        }
        ttEntry newEntry = new ttEntry((int)(TranspositionTable.GetZobristKey(bb) % transpositionTableSize), depth, flag, alpha, bestSoFar);
        TranspositionTable.AddValueToTable(newEntry.ZobristKey, newEntry);


        return new MoveScore(alpha, bestSoFar);
    }*/

    public static MoveScore SimpleQuiesce(Bitboard bb, bool color)
    {
        bb.ColorToMove = color;
        bb.ResetEnPassents(color);
        float standingPat = bb.Evaluate();

        List<Move> moves = bb.GenerateMoves(color, false);
        moves = Utility.CullNonCaptures(moves);
        moves = Utility.SortMoveList(moves, color);

        float max = -Mathf.Infinity;

        if (moves.Count == 0)
        {
            return new MoveScore(standingPat, emptyPV);
        }

        for (int i = 0; i < moves.Count; i++)
        {
            Bitboard newBB = bb.Duplicate();
            newBB.MakeMove(moves[i]);

            MoveScore score = SimpleQuiesce(newBB, !color);
            score.score *= -1;

            if (score.score > max)
            {
                max = score.score;
            }
        }

        return new MoveScore(max, emptyPV);
    }

    public static MoveScore QuiesceSearch(Bitboard bb, float alpha, float beta, bool color)
    {
        bb.ColorToMove = color;
        bb.ResetEnPassents(color);
        if (alpha == Mathf.Infinity)
        {
            Debug.LogError("Given Infiniity");
        }

        float standPat = bb.Evaluate();
        if (standPat >= beta)
        {
            return new MoveScore(beta, emptyPV);
        }
        if (standPat > alpha)
        {
            alpha = standPat;
        }
       

        Move best = new Move(-1, -1, -1, -1, -1);

        List<Move> moves = bb.GenerateMoves(color, false);
        moves = Utility.SortMoveList(moves, color);
        moves = Utility.CullNonCaptures(moves);
        if (moves.Count == 0)
        {
            return new MoveScore(standPat, emptyPV);
        }

        for (int i = 0; i < moves.Count; i++)
        {
            //if (moves[i].capture != 0)
            //{
                Bitboard newBB = bb.Duplicate();
                newBB.MakeMove(moves[i]);

                MoveScore score = QuiesceSearch(newBB, -alpha, -beta, !color);
                score.score *= -1;

                if (score.score >= beta)
                {
                    return new MoveScore(beta, moves[i]);
                }
                if (score.score > alpha)
                {
                    alpha = score.score;
                    best = moves[i];
                }
            //}
        }

        if (best.startSquare == -1)
        {
            return new MoveScore(standPat, best);
        }

        return new MoveScore(alpha, best);
    }

    public static MoveScore Negamax(Bitboard bb, int depth, bool color, float alpha, float beta)
    {
        bb.ColorToMove = color;
        bb.ResetEnPassents(color);
        count++;
        Move best = new Move();

        //Debug.Log("Alpha Start = " + alphaOrig);

        if (depth == 0)
        {
            return new MoveScore(bb.Evaluate(), emptyPV);
            //float tempScore = QuiesceSearch(bb, -Mathf.Infinity, Mathf.Infinity, color).score;
            //float tempScore = SimpleQuiesce(bb, color).score;
            //Debug.Log("Quiesce Search Score = " + tempScore);
            //return new MoveScore(tempScore, emptyPV);
        }

        List<Move> moves = bb.GenerateMoves(color, false);

        moves = Utility.SortMoveList(moves, color);

        if (moves.Count == 0 && depth != maxDepth)
        {
            if (!bb.CheckLegal(color))
            {
                return new MoveScore(-Mathf.Infinity, emptyPV);
            }
            else
            {
                return new MoveScore(0, emptyPV);
            }

        }

        for (int i = 0; i < moves.Count; i++)
        {
            Bitboard newBB = bb.Duplicate();
            newBB.MakeMove(moves[i]);

            MoveScore score = Negamax(newBB, depth - 1, !color, -beta, -alpha);

            score.score *= -1;

            if (score.score > alpha)
            {
                alpha = score.score;
                best = moves[i];
            }
            if (alpha >= beta)
            {
                if (moves[i].capture == 0)
                {
                    int side2move = color ? 0 : 1;
                    Utility.HistoryTable[side2move][moves[i].startSquare][moves[i].targetSquare] += (depth * depth);
                }
                return new MoveScore(alpha, moves[i]);
            }
        }

        if (depth == maxDepth && best.startSquare == 0 && best.targetSquare == 0)
        {
            //Debug.Log("Alpha = " + alpha);
            //Debug.Log("Move Count = " + moves.Count);
            //bb.DebugMoveList(moves);
            if (!bb.CheckLegal(color))
            {
                alpha = -Mathf.Infinity;
            }
            else
            {
                alpha = 0;
            }
            Debug.Log("THIS IS LOSING FOR SURE FOR - " + bb.ColorToMove);

            best = moves[Random.Range(0, moves.Count)];
        }

        return new MoveScore(alpha, best);
    }


    public static MoveScore NegaScout(Bitboard bb, int depth, bool color, float alpha, float beta)//, bool nullMoveAllowed = true)
    {
        bb.ColorToMove = color;
        bb.ResetEnPassents(color);
        count++;
        Move best = new Move();


        //Debug.Log("Alpha Start = " + alphaOrig);

        if (depth == 0)
        {
            return new MoveScore(bb.Evaluate(), emptyPV);
            //float tempScore = QuiesceSearch(bb, -Mathf.Infinity, Mathf.Infinity, color).score;
            //float tempScore = SimpleQuiesce(bb, color).score;
            //Debug.Log("Quiesce Search Score = " + tempScore);
            //return new MoveScore(tempScore, emptyPV);
        }

        float b = beta;

        List<Move> moves = bb.GenerateMoves(color, false);

        moves = Utility.SortMoveList(moves,color);

        if (moves.Count == 0 && depth != maxDepth)
        {
            if (!bb.CheckLegal(color))
            {
                return new MoveScore(-Mathf.Infinity, emptyPV);
            }
            else
            {
                return new MoveScore(0, emptyPV);
            }
            
        }

        for (int i = 0; i < moves.Count; i++)
        {
            Bitboard newBB = bb.Duplicate();
            newBB.MakeMove(moves[i]);

            MoveScore score = NegaScout(newBB, depth - 1, !color, -b, -alpha);
            
            
            score.score *= -1;
            
            

            if (score.score > alpha && score.score < beta && i > 0)
            {
                score = NegaScout(newBB, depth - 1, !color, -beta, -alpha);
                score.score *= -1;
                
            }
            if (score.score > alpha)
            {
                alpha = score.score;
                best = moves[i];
            }
            if (alpha >= beta)
            {
                if (moves[i].capture == 0)
                {
                    int side2move = color ? 0 : 1;
                    Utility.HistoryTable[side2move][moves[i].startSquare][moves[i].targetSquare] += (depth * depth);
                }
                return new MoveScore(alpha, moves[i]);
            }
            b = alpha + 1;
        }

        if (depth == maxDepth && best.startSquare == 0 && best.targetSquare == 0)
        {
            //Debug.Log("Alpha = " + alpha);
            //Debug.Log("Move Count = " + moves.Count);
            //bb.DebugMoveList(moves);
            if (!bb.CheckLegal(color))
            {
                alpha = -Mathf.Infinity;
            }
            else
            {
                alpha = 0;
            }
            Debug.Log("THIS IS LOSING FOR SURE FOR - " + bb.ColorToMove);

            best = moves[Random.Range(0, moves.Count)];
        }


        return new MoveScore(alpha, best);
    }

    public static void DebugMove(Move m)
    {
        Debug.Log("Move Info:\n\t" + "Start : " + m.startSquare + "\n\tEnd : " + m.targetSquare + "\n\tTag : " + m.tag + "\n\tCastle Info : " + m.castleAdj + "\n\tCapture Rating : " + m.capture + "\n\tPromotion : " + m.promotion);
    }
}


public struct MoveScore
{
    public float score;
    public Move move;
    public MoveScore(float _score, Move _move)
    {
        score = _score;
        move = _move;
    }
}

