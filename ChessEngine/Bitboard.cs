using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bitboard
{
    int W_R_Q_Pos = 0;
    int W_R_K_Pos = 7;
    int B_R_Q_Pos = 56;
    int B_R_K_Pos = 63;

    private Move emptyMove = new Move(-1, -1, -1, -1, -1);

    private ulong WhiteKing;
    private ulong WhitePawns;
    private ulong WhiteKnights;
    private ulong WhiteRooks;
    private ulong WhiteBishops; 
    private ulong WhiteQueen;

    private ulong BlackKing;
    private  ulong BlackPawns;//set back to private
    private ulong BlackKnights;
    private ulong BlackRooks;
    private ulong BlackBishops;
    private ulong BlackQueen;

    private ulong WhitePieces;
    private ulong BlackPieces;
    private ulong SquaresOccupied;

    private ulong[] Boards;

    int lastTake = 0;

    public bool ColorToMove = true;

    public CastleState castleState = new CastleState(0, 0, 0, 0); //is not 0 cant castle. if 1 and unmaking move reset to 0 else dont

    public int ZorbristHash;
    public int PreMoveZorbristHash;

    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
    */

    //Tag is index value is piece value. first index is useless
    private int[] TagToValue = new int[]{ 0, 100, 300, 300, 500, 900, 50000, 100, 300, 300, 500, 900, 50000 };

    public List<int> blackEnPassants = new List<int>();
    public List<int> whiteEnPassants = new List<int>();

    public void InitilizeBitboard()
    {
        WhiteKing = Constants.WHITE_KING_START;
        WhitePawns = Constants.WHITE_PAWNS_START;
        WhiteRooks = Constants.WHITE_ROOKS_START;
        WhiteBishops = Constants.WHITE_BISHOPS_START;
        WhiteKnights = Constants.WHITE_KNIGHTS_START;
        WhiteQueen = Constants.WHITE_QUEEN_START;

        BlackKing = Constants.BLACK_KING_START;
        BlackPawns = Constants.BLACK_PAWNS_START;
        BlackRooks = Constants.BLACK_ROOKS_START;
        BlackBishops = Constants.BLACK_BISHOPS_START;
        BlackKnights = Constants.BLACK_KNIGHTS_START;
        BlackQueen = Constants.BLACK_QUEEN_START;

        WhitePieces = WhiteKing | WhitePawns | WhiteRooks | WhiteBishops | WhiteKnights | WhiteQueen;
        BlackPieces = BlackKing | BlackPawns | BlackRooks | BlackBishops | BlackKnights | BlackQueen;
        SquaresOccupied = WhitePieces | BlackPieces;

        Boards = new ulong[] { WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueen, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueen, BlackKing };
    }

    public Bitboard Duplicate()
    {
        Bitboard bb = new Bitboard();
        bb.WhiteKing = WhiteKing;
        bb.WhitePawns = WhitePawns;
        bb.WhiteRooks = WhiteRooks;
        bb.WhiteBishops = WhiteBishops;
        bb.WhiteKnights = WhiteKnights;
        bb.WhiteQueen = WhiteQueen;
        bb.BlackKing = BlackKing;
        bb.BlackPawns = BlackPawns;
        bb.BlackRooks = BlackRooks;
        bb.BlackBishops = BlackBishops;
        bb.BlackKnights = BlackKnights;
        bb.BlackQueen = BlackQueen;
        bb.WhitePieces = WhitePieces;
        bb.BlackPieces = BlackPieces;
        bb.SquaresOccupied = SquaresOccupied;
        bb.Boards = DuplicateBoardArr();
        bb.castleState = castleState;
        bb.ColorToMove = ColorToMove;
        bb.lastTake = lastTake;

        bb.W_R_Q_Pos = W_R_Q_Pos;
        bb.W_R_K_Pos = W_R_K_Pos;
        bb.B_R_Q_Pos = B_R_Q_Pos;
        bb.B_R_K_Pos = B_R_K_Pos;

        bb.ZorbristHash = ZorbristHash;
        bb.PreMoveZorbristHash = PreMoveZorbristHash;

        bb.blackEnPassants = blackEnPassants;
        bb.whiteEnPassants = whiteEnPassants;

        return bb;
    }

    public ulong[] DuplicateBoardArr()
    {
        ulong[] newBoards = new ulong[12];
        for (int i = 0; i < newBoards.Length; i++)
        {
            newBoards[i] = Boards[i];
        }
        return newBoards;
    }

    public void MoveTest(bool color)
    {
        
        //List<Move> moves = GenerateMoves(color, false);
        List<Move> moves;
        if (color)
        {
            moves = GenerateMovesWhitePawns(false);
        }
        else
        {
            moves = GenerateMovesBlackPawns(false);
        }

        if (moves.Count == 0)
        {
            Debug.LogError("No Moves");
        }
        
        int num = Random.Range(0, moves.Count);
        MakeMove(moves[num]);
        //UnmakeMove(moves[num]);
    }

    public List<Move> GenerateMoves(bool color, bool check)
    {
        List<Move> possibles = new List<Move>();

        if (color) //white
        {
            possibles.AddRange(GenerateMovesPawns(color));
            possibles.AddRange(GenerateMovesKnight(true, check));
            possibles.AddRange(GenerateMovesBishop(true, check));
            possibles.AddRange(GenerateMovesRook(true, check));
            possibles.AddRange(GenerateMovesQueen(true, check));
            possibles.AddRange(GenerateMovesKing(true, check));
        }
        else //black
        {
            possibles.AddRange(GenerateMovesPawns(color));
            possibles.AddRange(GenerateMovesKnight(false, check));
            possibles.AddRange(GenerateMovesBishop(false, check));
            possibles.AddRange(GenerateMovesRook(false, check));
            possibles.AddRange(GenerateMovesQueen(false, check));
            possibles.AddRange(GenerateMovesKing(false, check));
        }

        //DebugMoveList(possibles);

        return possibles;
    }

    public List<Move> GenerateMovesKnight(bool color, bool check)
    {
        List<Move> possibles = new List<Move>();

        ulong eligableSquares = color ? ~WhitePieces : ~BlackPieces;
        ulong remainingKnights = color ? WhiteKnights : BlackKnights;

        int tag = color ? 2 : 8;

        while (remainingKnights != 0)
        {
            int startSquare = BitOps.BitScanForward(remainingKnights);
            ulong moves = Movement.GetKnightAttacks(startSquare) & eligableSquares;
            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);
                Move newMove = new Move(startSquare, target, tag, 0, 0); //2 = White Knight Tag
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(color))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }
                

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingKnights ^= (ulong)Mathf.Pow(2, startSquare);
        }

        return possibles;
    }

    public List<Move> GenerateMovesPawns(bool color) //OK SO THE PROBLEM LIES IN THAT WHEN THE FIRST PAWN ATTACKS A SQUARE NO OTHER PAWNS ARE ABLE TO ATTACK THAT SAME SQUARE I THINK IN THE SAME MOVE GENERATION DUE TO MAKE MOVE AND UNAMEK MOVE I THINK
    {
        //List<Move> notPossbles = new List<Move>();
        //List<Move> all = new List<Move>();



        List<Move> possibles = new List<Move>();
        ulong eligableSquares = ~SquaresOccupied;
        ulong remainingPawns = color ? WhitePawns : BlackPawns;
        ulong enemyPieces = color ? BlackPieces : WhitePieces;

        int tag = color ? 1 : 7;

        ulong enPassantTargets = EnPassantTargetsToUlong(!color);

        while (remainingPawns != 0)
        {
            int pos = BitOps.BitScanForward(remainingPawns);

            

            ulong moves = Movement.GetSinglePawnPush((ulong)Mathf.Pow(2, pos), eligableSquares, enemyPieces, color); //Movement.GetSinglePawnAttacks((ulong)Mathf.Pow(2, pos), enemyPieces, color);//
            ulong enPassantMoves = Movement.GetSinglePawnEnPassant((ulong)Mathf.Pow(2, pos), enPassantTargets, color) & eligableSquares;
            //Debug.Log("Pawn Moves = " + moves);
            //moves |= Movement.GetSinglePawnAttacks((ulong)Mathf.Pow(2, pos), enemyPieces, color);

            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves); 

                int promotion = 0;

                if (color && target >= 56)
                {
                    promotion = 1;
                }
                else if (!color && target <= 7)
                {
                    promotion = 2;
                }

                Move newMove = new Move(pos, target, tag, 0, 0, promotion); //1 = White Pawn
                newMove.capture = GetMoveScore(newMove);



                MakeMove(newMove);
                if (CheckLegal(color))
                {
                    possibles.Add(newMove);
                }
                /*else THIS IS JUST FOR DEBUGGING ALL POSSIBLE MOVES
                {
                    notPossbles.Add(newMove);
                    DebugBoard(newMove);
                }*/
                UnmakeMove(newMove);
             

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            while (enPassantMoves != 0)
            {
                int pawnToKillAdj = color ? -8 : 8; //if we attack with a white pawn we need to kill the black pawn which is 8 units down from where we are actually moving our pawn and vice versa

                int target = BitOps.BitScanForward(enPassantMoves);

                int promotion = 0;//ALWAYS FOR ENPASSENT

                Move newMove = new Move(pos, target, tag, 0, 0, promotion, target + pawnToKillAdj);
                newMove.capture = 0;

                MakeMove(newMove);
                if (CheckLegal(color))
                {
                    possibles.Add(newMove);
                }
                UnmakeMove(newMove);

                enPassantMoves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingPawns ^= (ulong)Mathf.Pow(2, pos);
        }

        return possibles;
    }

    public List<Move> GenerateMovesWhitePawns(bool check)
    {
        List<Move> possibles = new List<Move>();
        ulong eligableSquares = ~SquaresOccupied;
        ulong remainingPawns = WhitePawns;

        while (remainingPawns != 0)
        {
            int pos = BitOps.BitScanForward(remainingPawns);

            ulong moves = Movement.GetSinglePawnPush((ulong)Mathf.Pow(2, pos), eligableSquares, BlackPieces, true);

            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);

                int promotion = 0;

                if (target >= 56)
                {
                    promotion = 1;
                }

                Move newMove = new Move(pos, target, 1, 0, 0, promotion); //1 = White Pawn
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(true))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingPawns ^= (ulong)Mathf.Pow(2, pos);
        }

        return possibles;
    }

    public List<Move> GenerateMovesBlackPawns(bool check)
    {
        List<Move> possibles = new List<Move>();
        ulong eligableSquares = ~SquaresOccupied;
        ulong remainingPawns = BlackPawns;

        while (remainingPawns != 0)
        {
            int pos = BitOps.BitScanForward(remainingPawns);

            ulong moves = Movement.GetSinglePawnPush((ulong)Mathf.Pow(2, pos), eligableSquares, WhitePieces, false);

            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);

                int promotion = 0;

                if (target <= 7)
                {
                    promotion = 2;
                }

                Move newMove = new Move(pos, target, 7, 0, 0, promotion); //7 = BlackPawn
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(false))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingPawns ^= (ulong)Mathf.Pow(2, pos);
        }

        return possibles;
    }

    public List<Move> GenerateMovesBishop(bool color, bool check)
    {
        List<Move> possibles = new List<Move>();
        ulong eligableSquares = color ? ~WhitePieces : ~BlackPieces;
        ulong remainingBishops = color ? WhiteBishops : BlackBishops;

        int tag = color ? 3 : 9;

        while (remainingBishops != 0)
        {
            int startSquare = BitOps.BitScanForward(remainingBishops);

            ulong moves = Movement.GetBishopAttacks(SquaresOccupied, startSquare) & eligableSquares;
            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);

                Move newMove = new Move(startSquare, target, tag, 0, 0);
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(color))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingBishops ^= (ulong)Mathf.Pow(2, startSquare);
        }

        return possibles;
    }

    public List<Move> GenerateMovesRook(bool color, bool check)
    {
        List<Move> possibles = new List<Move>();
        ulong eligableSquares = color ? ~WhitePieces : ~BlackPieces;
        ulong remainingRooks = color ? WhiteRooks : BlackRooks;

        int tag = color ? 4 : 10;

        while (remainingRooks != 0)
        {           
            int startSquare = BitOps.BitScanForward(remainingRooks);

            int castleAdj = 0;

            if (color)
            {
                if (startSquare == W_R_K_Pos)
                {
                    castleAdj = 2;
                }
                else
                {
                    castleAdj = 1;
                }
                //castleAdj = (remainingRooks ^ (ulong)Mathf.Pow(2, startSquare)) != 0 ? 1 : 2; // 1 == White Queen Side, 2 = White King Side, 3 = Black Queen Side, 4 = Black King Side
            }
            else
            {
                if (startSquare == B_R_K_Pos)
                {
                    castleAdj = 4;
                }
                else
                {
                    castleAdj = 3;
                }
                //castleAdj = (remainingRooks ^ (ulong)Mathf.Pow(2, startSquare)) != 0 ? 3 : 4;
            }
             

            ulong moves = Movement.GetRookAttacks(SquaresOccupied, startSquare) & eligableSquares;
            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);

                

                Move newMove = new Move(startSquare, target, tag, castleAdj, 0);
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(color))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }

                moves ^= (ulong)Mathf.Pow(2, target);

            }

            remainingRooks ^= (ulong)Mathf.Pow(2, startSquare);
        }      
        return possibles;
    }

    public List<Move> GenerateMovesQueen(bool color, bool check)
    {

        List<Move> possibles = new List<Move>();
        ulong eligableSquares = color ? ~WhitePieces : ~BlackPieces;
        ulong remainingQueens = color ? WhiteQueen : BlackQueen;

        int tag = color ? 5 : 11;

        while (remainingQueens != 0)
        {
            int startSquare = BitOps.BitScanForward(remainingQueens);

            ulong moves = Movement.GetQueenAttacks(SquaresOccupied, startSquare) & eligableSquares;
            while (moves != 0)
            {
                int target = BitOps.BitScanForward(moves);

                Move newMove = new Move(startSquare, target, tag, 0, 0);
                newMove.capture = GetMoveScore(newMove);

                if (!check)
                {
                    MakeMove(newMove);
                    if (CheckLegal(color))
                    {
                        
                        possibles.Add(newMove);
                    }
                    UnmakeMove(newMove);
                }
                else
                {
                    possibles.Add(newMove);
                }

                moves ^= (ulong)Mathf.Pow(2, target);
            }

            remainingQueens ^= (ulong)Mathf.Pow(2, startSquare);
        }

        return possibles;
    }

    public List<Move> GenerateMovesKing(bool color, bool check)
    {
        List<Move> possibles = new List<Move>();
        ulong eligableSquares = color ? ~WhitePieces : ~BlackPieces;
        ulong remainingKing = color ? WhiteKing : BlackKing;

        int tag = color ? 6 : 12;
        int castleAdj = color ? 5 : 6;

        int startSquare = BitOps.BitScanForward(remainingKing);

        ulong moves = Movement.GetKingAttacks(startSquare) & eligableSquares;
        while (moves != 0)
        {
            int target = BitOps.BitScanForward(moves);

            Move newMove = new Move(startSquare, target, tag, castleAdj, 0); //5 = Cancel White Castle, 6 = Cancel Black Castle
            newMove.capture = GetMoveScore(newMove);

            if (!check)
            {
                MakeMove(newMove);
                if (CheckLegal(color))
                {
                    
                    possibles.Add(newMove);
                }
                UnmakeMove(newMove);
            }
            else
            {
                possibles.Add(newMove);
            }

            moves ^= (ulong)Mathf.Pow(2, target);
        }

        //CASTLING LOGIC

        if (color)
        {
            if (castleState.WK == 0)
            {
                if (SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 5)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 6))) //nothing blocking king side castle white
                {
                    if (CheckLegal(color))
                    {
                        if (!SquareAttacked(color, 5))
                        {
                            Move m = new Move(4, 6, tag, 7, 0);
                            MakeMove(m);
                            if (CheckLegal(color))
                            {
                                possibles.Add(m); //7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            }
                            UnmakeMove(m);
                        }
                        
                    }
                    
                }
            }
            if (castleState.WQ == 0)
            {
                if (SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 1)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 2)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 3)))
                {
                    if (CheckLegal(color))
                    {
                        if (!SquareAttacked(color, 3))
                        {
                            Move m = new Move(4, 2, tag, 8, 0);//7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            MakeMove(m);
                            if (CheckLegal(color))
                            {
                                possibles.Add(m); //7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            }
                            UnmakeMove(m);
                        }
                        
                    }
                }
            }
        }
        else
        {
            if (castleState.BK == 0)
            {
                if (SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 61)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 62))) //nothing blocking king side castle white
                {
                    if (CheckLegal(color))
                    {
                        if (!SquareAttacked(color, 61))
                        {
                            Move m = new Move(60, 62, tag, 9, 0);//7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            MakeMove(m);
                            if (CheckLegal(color))
                            {
                                possibles.Add(m); //7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            }
                            UnmakeMove(m);
                        }
                        
                    }
                }
            }
            if (castleState.BQ == 0)
            {
                if (SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 57)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 58)) && SquaresOccupied != (SquaresOccupied | (ulong)Mathf.Pow(2, 59)))
                {
                    if (CheckLegal(color))
                    {
                        if (!SquareAttacked(color, 59))
                        {
                            Move m = new Move(60, 58, tag, 10, 0);//7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            MakeMove(m);
                            if (CheckLegal(color))
                            {
                                possibles.Add(m); //7, 8, 9, 10 = made castle on WK, WQ, BK, BQ
                            }
                            UnmakeMove(m);
                        }
                        
                    }
                }
            }
        }

        return possibles;
    }

    public void MakeMove(Move move)
    {
        CastleAdjustmentOnMake(move.castleAdj);

        if (move.castleAdj >= 7)
        {
            DoCastle(move.castleAdj);
        }

        lastTake = -1;

        ulong target = (ulong)Mathf.Pow(2, move.targetSquare);
        ulong start = (ulong)Mathf.Pow(2, move.startSquare);

        if (move.enPassantTarget == 0)
        {
            for (int i = 0; i < 12; i++)
            {
                if ((Boards[i] & ~target) != Boards[i])
                {
                    lastTake = i;
                }
                Boards[i] &= ~target;
            }
        }
        

        SelfBoardMove(start, target, move.tag, move.promotion, false);

        //Set En Passant
        if (move.tag == 1 || move.tag == 7)
        {
            SetEnPassant(move, true);
        }

        //Kill En Passant
        if (move.enPassantTarget != 0) //this has a ton(not much anymore) of debug junk rn so... fix that i guess
        {
            Boards[0] &= ~(ulong)Mathf.Pow(2, move.enPassantTarget);
            Boards[6] &= ~(ulong)Mathf.Pow(2, move.enPassantTarget);
        }

        SetBitBoardsFast();
        
        
        WhitePieces = WhiteKing | WhitePawns | WhiteRooks | WhiteBishops | WhiteKnights | WhiteQueen;
        BlackPieces = BlackKing | BlackPawns | BlackRooks | BlackBishops | BlackKnights | BlackQueen;
        SquaresOccupied = WhitePieces | BlackPieces;
    }

    public void UnmakeMove(Move move)
    {
        CastleAdjustmentOnUnmake(move.castleAdj);

        if (move.castleAdj >= 7)
        {
            UnDoCastle(move.castleAdj);
        }

        ulong start = (ulong)Mathf.Pow(2, move.targetSquare);
        ulong target = (ulong)Mathf.Pow(2, move.startSquare);

        SelfBoardMove(start, target, move.tag, move.promotion, true);

        //Remove En Passant
        if (move.tag == 1 || move.tag == 7)
        {
            SetEnPassant(move, false);
        }

        

        if (lastTake != -1 && move.enPassantTarget == 0)
        {
            ReplacePiece(lastTake, move.targetSquare);
        }
        lastTake = -1;

        //Replace En Passant
        if (move.enPassantTarget != 0)
        {
            if (ColorToMove)
            {
                Boards[6] |= (ulong)Mathf.Pow(2, move.enPassantTarget);
            }
            else
            {
                Boards[0] |= (ulong)Mathf.Pow(2, move.enPassantTarget);
            }
        }

        SetBitBoardsFast();

        //Debug.Log("Black Pawn Pop - " + BitOps.PopulationCount(Boards[6]));
        
        WhitePieces = WhiteKing | WhitePawns | WhiteRooks | WhiteBishops | WhiteKnights | WhiteQueen;
        BlackPieces = BlackKing | BlackPawns | BlackRooks | BlackBishops | BlackKnights | BlackQueen;
        SquaresOccupied = WhitePieces | BlackPieces;
    }

    public void CastleAdjustmentOnMake(int adj)
    {
        switch (adj)
        {
            case 0:
                return;
            case 1:
                castleState.WQ++;
                return;
            case 2:
                castleState.WK++;
                return;
            case 3:
                castleState.BQ++;
                return;
            case 4:
                castleState.BK++;
                return;
            case 5:
                castleState.WQ++;
                castleState.WK++;
                return;
            case 6:
                castleState.BQ++;
                castleState.BK++;
                return;
        }
    }
    public void CastleAdjustmentOnUnmake(int adj)
    {
        switch (adj)
        {
            case 0:
                return;
            case 1:
                castleState.WQ--;
                return;
            case 2:
                castleState.WK--;
                return;
            case 3:
                castleState.BQ--;
                return;
            case 4:
                castleState.BK--;
                return;
            case 5:
                castleState.WQ--;
                castleState.WK--;
                return;
            case 6:
                castleState.BQ--;
                castleState.BK--;
                return;
        }
    }

    public void DoCastle(int val)
    {
        switch (val)
        {
            case 7:
                WhiteRooks |= (ulong)Mathf.Pow(2, 5);
                WhiteRooks ^= (ulong)Mathf.Pow(2, 7);
                Boards[3] |= (ulong)Mathf.Pow(2, 5);
                Boards[3] ^= (ulong)Mathf.Pow(2, 7);
                castleState.WQ++;
                castleState.WK++;
                return;
            case 8:
                WhiteRooks |= (ulong)Mathf.Pow(2, 3);
                WhiteRooks ^= (ulong)Mathf.Pow(2, 0);
                Boards[3] |= (ulong)Mathf.Pow(2, 3);
                Boards[3] ^= (ulong)Mathf.Pow(2, 0);
                castleState.WQ++;
                castleState.WK++;
                return;
            case 9:
                BlackRooks |= (ulong)Mathf.Pow(2, 61);
                BlackRooks ^= (ulong)Mathf.Pow(2, 63);
                Boards[9] |= (ulong)Mathf.Pow(2, 61);
                Boards[9] ^= (ulong)Mathf.Pow(2, 63);
                castleState.BQ++;
                castleState.BK++;
                return;
            case 10:
                BlackRooks |= (ulong)Mathf.Pow(2, 59);
                BlackRooks ^= (ulong)Mathf.Pow(2, 56);
                Boards[9] |= (ulong)Mathf.Pow(2, 59);
                Boards[9] ^= (ulong)Mathf.Pow(2, 56);
                castleState.BQ++;
                castleState.BK++;
                return;
        }

    }
    public void UnDoCastle(int val)
    {
        switch (val)
        {
            case 7:
                WhiteRooks ^= (ulong)Mathf.Pow(2, 5);
                WhiteRooks |= (ulong)Mathf.Pow(2, 7);
                Boards[3] ^= (ulong)Mathf.Pow(2, 5);
                Boards[3] |= (ulong)Mathf.Pow(2, 7);
                castleState.WQ--;
                castleState.WK--;
                return;
            case 8:
                WhiteRooks ^= (ulong)Mathf.Pow(2, 3);
                WhiteRooks |= (ulong)Mathf.Pow(2, 0);
                Boards[3] ^= (ulong)Mathf.Pow(2, 3);
                Boards[3] |= (ulong)Mathf.Pow(2, 0);
                castleState.WQ--;
                castleState.WK--;
                return;
            case 9:
                BlackRooks ^= (ulong)Mathf.Pow(2, 61);
                BlackRooks |= (ulong)Mathf.Pow(2, 63);
                Boards[9] ^= (ulong)Mathf.Pow(2, 61);
                Boards[9] |= (ulong)Mathf.Pow(2, 63);
                castleState.BQ--;
                castleState.BK--;
                return;
            case 10:
                BlackRooks ^= (ulong)Mathf.Pow(2, 59);
                BlackRooks |= (ulong)Mathf.Pow(2, 56);
                Boards[9] ^= (ulong)Mathf.Pow(2, 59);
                Boards[9] |= (ulong)Mathf.Pow(2, 56);
                castleState.BQ--;
                castleState.BK--;
                return;
        }

    }

    public void SelfBoardMove(ulong start, ulong target, int tag, int promotion, bool unMake = false)
    {
        switch (tag)
        {
            case 2: //White Knight
                //WhiteKnights ^= start;
                //WhiteKnights |= target;
                Boards[1] ^= start;
                Boards[1] |= target;
                return;
            case 8: //Black Knight
                //BlackKnights ^= start;
                //BlackKnights |= target;
                Boards[7] ^= start;
                Boards[7] |= target;
                return;


            case 1: //White Pawn
                if (promotion == 1)
                {
                    if (unMake)
                    {
                        //WhitePawns |= target;
                        //WhiteQueen ^= start;
                        Boards[0] |= target;
                        Boards[4] ^= start;
                        return;
                    }
                    //WhitePawns ^= start;
                    //WhiteQueen |= target;
                    Boards[0] ^= start;
                    Boards[4] |= target;
                    //Debug.Log(BitOps.PopulationCount(Boards[4]));
                    //Debug.Log(BitOps.PopulationCount(Boards[0]));
                    return;
                }
                else
                {
                    //WhitePawns ^= start;
                    //WhitePawns |= target;
                    Boards[0] ^= start;
                    Boards[0] |= target;
                    //Debug.Log("Did Normal Pawn Move");
                    return;
                }
                
            case 7: //Black Pawn
                if (promotion == 2)
                {
                    if (unMake)
                    {
                        //BlackPawns |= target;
                        //BlackQueen ^= start;
                        Boards[6] |= target;
                        Boards[10] ^= start;
                        return;
                    }
                    //BlackPawns ^= start;
                    //BlackQueen |= target;
                    Boards[6] ^= start;
                    Boards[10] |= target;
                    return;
                }
                else
                {
                    //BlackPawns ^= start;
                    //BlackPawns |= target;
                    Boards[6] ^= start;
                    Boards[6] |= target;
                    return;
                }
                


            case 3: //White Bishop
                //WhiteBishops ^= start;
                //WhiteBishops |= target;
                Boards[2] ^= start;
                Boards[2] |= target;
                return;
            case 9: //Black Bishop
                //BlackBishops ^= start;
                //BlackBishops |= target;
                Boards[8] ^= start;
                Boards[8] |= target;
                return;


            case 4: //White Rooks
                //WhiteRooks ^= start;
                //WhiteRooks |= target;
                UpdateRookPositions((int)start, (int)target);
                Boards[3] ^= start;
                Boards[3] |= target;
                return;
            case 10: //Black Rooks
                //BlackRooks ^= start;
                //BlackRooks |= target;
                UpdateRookPositions((int)start, (int)target);
                Boards[9] ^= start;
                Boards[9] |= target;
                return;


            case 5: //White Queen
                //WhiteQueen ^= start;
                //WhiteQueen |= target;
                Boards[4] ^= start;
                Boards[4] |= target;
                return;
            case 11: //Black Queen
                //BlackQueen ^= start;
                //BlackQueen |= target;
                Boards[10] ^= start;
                Boards[10] |= target;
                return;


            case 6: //White King
                //WhiteKing ^= start;
                //WhiteKing |= target;
                Boards[5] ^= start;
                Boards[5] |= target;
                return;
            case 12: //Black King
                //BlackKing ^= start;
                //BlackKing |= target;
                Boards[11] ^= start;
                Boards[11] |= target;
                return;
        }
    }


    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
    */
    public void SetBitBoards(int execption) //move.tag
    {
        //There is probabaly a way to do this wihtout using all these branches but i think that this is OK for now

        //SEE TAG CHART
        if (execption != 6)
        {
            WhiteKing = Boards[5];
        }
        
        if (execption != 1)
        {
            WhitePawns = Boards[0];
        }

        if (execption != 4)
        {
            WhiteRooks = Boards[3];
        }
        
        if (execption != 3)
        {
            WhiteBishops = Boards[2];
        }
        
        if (execption != 2)
        {
            WhiteKnights = Boards[1];
        }
        
        if (execption != 5)
        {
            WhiteQueen = Boards[4];
        }
        
        if (execption != 12)
        {
            BlackKing = Boards[11];
        }

        if (execption != 7)
        {
            BlackPawns = Boards[6];
        }
        
        if (execption != 10)
        {
            BlackRooks = Boards[9];
        }

        if (execption != 9)
        {
            BlackBishops = Boards[8];
        }
        
        if (execption != 8)
        {
            BlackKnights = Boards[7];
        }
        
        if (execption != 11)
        {
            BlackQueen = Boards[10];
        }
    }

    public void SetBitBoardsFast()
    {
        WhiteKing = Boards[5];
        WhitePawns = Boards[0];
        WhiteRooks = Boards[3];
        WhiteBishops = Boards[2];
        WhiteKnights = Boards[1];
        WhiteQueen = Boards[4];
        BlackKing = Boards[11];
        BlackPawns = Boards[6];
        BlackRooks = Boards[9];
        BlackBishops = Boards[8];
        BlackKnights = Boards[7];
        BlackQueen = Boards[10];
    }


    public bool CheckLegal(bool color) //Color to try move
    {
        int kingPos = color ? BitOps.BitScanForward(WhiteKing) : BitOps.BitScanForward(BlackKing);
        return !SquareAttacked(color, kingPos);
        /*List<Move> moves = GenerateMoves(!color, true);
        
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].targetSquare == kingPos)
            {
                return false;
            }
        }
        return true;*/
    }

    private bool SquareAttacked(bool color, int pos) 
    {
        if (color)
        {
            if (Movement.GetSinglePawnAttacks((ulong)Mathf.Pow(2, pos), BlackPawns, color) != 0)
            {
                return true;
            }
            if ((Movement.GetKnightAttacks(pos) & BlackKnights) != 0)
            {
                return true;
            }
            if ((Movement.GetKingAttacks(pos) & BlackKing) != 0)
            {
                return true;
            }
            if ((Movement.GetRookAttacks(SquaresOccupied, pos) & BlackRooks) != 0)
            {
                return true;
            }
            if ((Movement.GetBishopAttacks(SquaresOccupied, pos) & BlackBishops) != 0)
            {
                return true;
            }
            if ((Movement.GetQueenAttacks(SquaresOccupied, pos) & BlackQueen) != 0)
            {
                return true;
            }
        }
        else
        {
            if (Movement.GetSinglePawnAttacks((ulong)Mathf.Pow(2, pos), WhitePawns, color) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White Pawn Attack");
                return true;
            }
            if ((Movement.GetKnightAttacks(pos) & WhiteKnights) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White Knight Attack");
                return true;
            }
            if ((Movement.GetKingAttacks(pos) & WhiteKing) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White King Attack");
                return true;
            }
            if ((Movement.GetRookAttacks(SquaresOccupied, pos) & WhiteRooks) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White Rook Attack");
                return true;
            }
            if ((Movement.GetBishopAttacks(SquaresOccupied, pos) & WhiteBishops) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White Bishop Attack");
                return true;
            }
            if ((Movement.GetQueenAttacks(SquaresOccupied, pos) & WhiteQueen) != 0)
            {
                //Debug.LogError("Illegal Move Detected : White Queen Attack");
                return true;
            }
        }

        return false;
        
    }
    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
    */
    public void ReplacePiece(int tag, int square)
    {
        Boards[tag] |= (ulong)Mathf.Pow(2, square);
    }

    public float Evaluate()
    {
        int score = 0;

        score += BitOps.PopulationCount(WhitePawns) * 200;
        score -= BitOps.PopulationCount(BlackPawns) * 200;

        score += BitOps.PopulationCount(WhiteKnights) * 600;
        score -= BitOps.PopulationCount(BlackKnights) * 600;

        score += BitOps.PopulationCount(WhiteBishops) * 600;
        score -= BitOps.PopulationCount(BlackBishops) * 600;

        score += BitOps.PopulationCount(WhiteRooks) * 1000;
        score -= BitOps.PopulationCount(BlackRooks) * 1000;

        score += BitOps.PopulationCount(WhiteQueen) * 1800;
        score -= BitOps.PopulationCount(BlackQueen) * 1800;

        for (int i = 0; i < Boards.Length; i++)
        {
            score += (int)Evaluation.GetScoreOfBoard(Boards[i], i + 1);
        }

        score *= ColorToMove ? 1 : -1;
       
        return score;
    }

    public int GetMoveScore(Move m)
    {
        int valPiece = TagToValue[m.tag];
        int capVal = FindValOfPieceOnSquare(m.targetSquare);
        if (capVal != 0)
        {
            return valPiece - capVal + 1;
        }
        return 0;
    }

    public int FindValOfPieceOnSquare(int square) //TagToValue[i + 1] = opposite score
    {
        for (int i = 0; i < Boards.Length; i++)
        {
            if ((Boards[i] & (ulong)(Mathf.Pow(2, square))) != 0)
            {
                return TagToValue[i + 1];
            }
        }
        return 0;
    }

    public void UpdateRookPositions(int start, int end)
    {
        if (start == W_R_Q_Pos)
        {
            W_R_Q_Pos = end;
            return;
        }
        else if (start == W_R_K_Pos)
        {
            W_R_K_Pos = end;
            return;
        }
        else if (start == B_R_Q_Pos)
        {
            B_R_Q_Pos = end;
            return;
        }
        else if (start == B_R_K_Pos)
        {
            B_R_K_Pos = end;
            return;
        }
    }

    public void UpdateZorbristHash(Move m)
    {
        int hash1 = TranspositionTable.PieceSquareValues[m.startSquare, m.tag - 1];
        int hash2 = TranspositionTable.PieceSquareValues[m.targetSquare, m.tag - 1];

        int hash3 = -1;

        if (m.capture != 0)
        {
            ulong position = (ulong)Mathf.Pow(2, m.targetSquare);

            ulong[] tempBoards = DuplicateBoardArr();
            for (int i = 0; i < tempBoards.Length; i++)
            {
                if ((tempBoards[i] ^ position) != Boards[i])
                {
                    hash3 = TranspositionTable.PieceSquareValues[m.targetSquare, i];
                }
            }
        }

        ZorbristHash ^= hash1;
        ZorbristHash ^= hash2;
        if (hash3 != 0)
        {
            ZorbristHash ^= hash3;
        }
    }

    public void SetEnPassant(Move m, bool add)
    {
        if (add)
        {
            if (Mathf.Abs(m.startSquare - m.targetSquare) > 8)
            {
                if (m.tag == 1)
                {
                    whiteEnPassants.Add(m.targetSquare);
                }
                else
                {
                    blackEnPassants.Add(m.targetSquare);
                }
            }
        }
        else
        {
            if (Mathf.Abs(m.startSquare - m.targetSquare) > 8)
            {
                if (m.tag == 1)
                {
                    whiteEnPassants.Remove(m.targetSquare);
                }
                else
                {
                    blackEnPassants.Remove(m.targetSquare);
                }
            }
        } 
    }

    public ulong EnPassantTargetsToUlong(bool colorOfTargets)
    {
        ulong bb = 0;

        if (colorOfTargets)
        {
            for (int i = 0; i < whiteEnPassants.Count; i++)
            {
                bb |= (ulong)Mathf.Pow(2, whiteEnPassants[i] - 8);
            }
        }
        else
        {
            for (int i = 0; i < blackEnPassants.Count; i++)
            {
                bb |= (ulong)Mathf.Pow(2, blackEnPassants[i] + 8);
            }
        }

        return bb;
    }

    public void ResetEnPassents(bool colorMoving)
    {
        if (colorMoving)
        {
            whiteEnPassants.Clear();
        }
        else
        {
            blackEnPassants.Clear();
        }
    }

    //-------- GAME MANAGER INTERACTION -----------//
    //_____________________________________________//

    public char[] CreateCharArr()
    {
        char[] charBoard = new char[64];

        //Kings
        charBoard[BitOps.BitScanForward(WhiteKing)] = "K"[0];
        charBoard[BitOps.BitScanForward(BlackKing)] = "k"[0];

        //Queens
        //if (WhiteQueen != 0) { charBoard[BitOps.BitScanForward(WhiteQueen)] = "Q"[0]; }
        //if (BlackQueen != 0) { charBoard[BitOps.BitScanForward(BlackQueen)] = "q"[0]; }
        ulong tempQueens = WhiteQueen;
        while (tempQueens != 0)
        {
            int pos = BitOps.BitScanForward(tempQueens);
            charBoard[pos] = "Q"[0];
            tempQueens ^= (ulong)Mathf.Pow(2, pos);
        }
        tempQueens = BlackQueen;
        while (tempQueens != 0)
        {
            int pos = BitOps.BitScanForward(tempQueens);
            charBoard[pos] = "q"[0];
            tempQueens ^= (ulong)Mathf.Pow(2, pos);
        }

        //Rooks
        ulong tempRooks = WhiteRooks;
        while (tempRooks != 0)
        {
            int pos = BitOps.BitScanForward(tempRooks);
            charBoard[pos] = "R"[0];
            tempRooks ^= (ulong)Mathf.Pow(2, pos);
        }
        tempRooks = BlackRooks;
        while (tempRooks != 0)
        {
            int pos = BitOps.BitScanForward(tempRooks);
            charBoard[pos] = "r"[0];
            tempRooks ^= (ulong)Mathf.Pow(2, pos);
        }

        //Bishops
        ulong tempBishops = WhiteBishops;
        while (tempBishops != 0)
        {
            int pos = BitOps.BitScanForward(tempBishops);
            charBoard[pos] = "B"[0];
            tempBishops ^= (ulong)Mathf.Pow(2, pos);
        }
        tempBishops = BlackBishops;
        while (tempBishops != 0)
        {
            int pos = BitOps.BitScanForward(tempBishops);
            charBoard[pos] = "b"[0];
            tempBishops ^= (ulong)Mathf.Pow(2, pos);
        }

        //Knights
        ulong tempKnights = WhiteKnights;
        while (tempKnights != 0)
        {
            int pos = BitOps.BitScanForward(tempKnights);
            charBoard[pos] = "N"[0];
            tempKnights ^= (ulong)Mathf.Pow(2, pos);
        }
        tempKnights = BlackKnights;
        while (tempKnights != 0)
        {
            int pos = BitOps.BitScanForward(tempKnights);
            charBoard[pos] = "n"[0];
            tempKnights ^= (ulong)Mathf.Pow(2, pos);
        }

        //Pawns
        ulong tempPawns = WhitePawns;
        while (tempPawns != 0)
        {
            int pos = BitOps.BitScanForward(tempPawns);
            charBoard[pos] = "P"[0];
            tempPawns ^= (ulong)Mathf.Pow(2, pos);
        }
        tempPawns = BlackPawns;
        while (tempPawns != 0)
        {
            int pos = BitOps.BitScanForward(tempPawns);
            charBoard[pos] = "p"[0];
            tempPawns ^= (ulong)Mathf.Pow(2, pos);
        }

        return charBoard;
    }

    public void DebugMoveList(List<Move> moves)
    {
        //Debug.LogError("Start Moves List");
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].castleAdj >= 7)
            {
                Debug.LogError("Move #" + (i + 1) + " | Start = " + moves[i].startSquare + " | Target = " + moves[i].targetSquare);
            }
            else
            {
                Debug.Log("Move #" + (i + 1) + " | Start = " + moves[i].startSquare + " | Target = " + moves[i].targetSquare);
            }
            
        }
        //Debug.LogError("End Moves List");
    }

    public void AddPieceToBoard(int boardIndex, int targetPos)
    {
        Boards[boardIndex] |= (ulong)Mathf.Pow(2, targetPos);
    }

    public void Clear()
    {
        WhiteKing = 0;
        WhitePawns = 0;
        WhiteRooks = 0;
        WhiteBishops = 0;
        WhiteKnights = 0;
        WhiteQueen = 0;

        BlackKing = 0;
        BlackPawns = 0;
        BlackRooks = 0;
        BlackBishops = 0;
        BlackKnights = 0;
        BlackQueen = 0;

        WhitePieces = 0;
        BlackPieces = 0;
        SquaresOccupied = 0;

        castleState = new CastleState(0, 0, 0, 0);

        Boards = new ulong[] { WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueen, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueen, BlackKing };
    }

    public void SetState()
    {
        WhitePieces = WhiteKing | WhitePawns | WhiteRooks | WhiteBishops | WhiteKnights | WhiteQueen;
        BlackPieces = BlackKing | BlackPawns | BlackRooks | BlackBishops | BlackKnights | BlackQueen;
        SquaresOccupied = WhitePieces | BlackPieces;

        Boards = new ulong[] { WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueen, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueen, BlackKing };
    }

    public void DebugBoard(Move m)
    {

        string str = "";
        str += "Board Info:";

        //Move Info
        if (m.startSquare != -1)
        {
            str += "\n\tMove Info:";
            str += "\n\t\tStart Square = " + m.startSquare;
            str += "\n\t\tTarget Square = " + m.targetSquare;
        }

        //Piece Info
        str += "\n\tPieces:";
        str += "\n\t\tWhite Pawns = " + WhitePawns;
        str += "\n\t\tBlack Pawns = " + BlackPawns;
        str += "\n\t\tWhite Bishops = " + WhiteBishops;
        str += "\n\t\tBlack Bishops = " + BlackBishops;
        str += "\n\t\tWhite Knights = " + WhiteKnights;
        str += "\n\t\tBlack Knights = " + BlackKnights;
        str += "\n\t\tWhite Rooks = " + WhiteRooks;
        str += "\n\t\tBlack Rooks = " + BlackRooks;
        str += "\n\t\tWhite Queen = " + WhiteQueen;
        str += "\n\t\tBlack Queen = " + BlackQueen;
        str += "\n\t\tWhite King = " + WhiteKing;
        str += "\n\t\tBlack King = " + BlackKing;

        Debug.Log(str);

        //Debug.Log("Board Info:")

        //Debug.Log("White Pawns = " + WhitePawns)
        //Debug.Log("Squares Occupied = " + SquaresOccupied);
    }

    public int Accumulate()
    {
        int sum = 0;
        sum += (int)WhitePawns;
        sum += (int)BlackPawns;
        sum += (int)WhiteKnights;
        sum += (int)BlackKnights;
        sum += (int)WhiteBishops;
        sum += (int)BlackBishops;
        sum += (int)WhiteQueen;
        sum += (int)BlackQueen;
        sum += (int)WhiteRooks;
        sum += (int)BlackRooks;
        sum += (int)WhiteKing;
        sum += (int)BlackKing;

        for (int i = 0; i < Boards.Length; i++)
        {
            sum += (int)Boards[i];
        }

        return sum;
    }
    
}

/*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
     
    */

public struct Move // caputre val is determined in GetMoveScore
{
    public int startSquare;
    public int targetSquare;
    public int tag;
    public int castleAdj;
    public int capture;
    public int promotion;
    public int enPassantTarget;
    public Move(int _startSquare, int _targetSquare, int _tag, int _castleAdj, int _capture, int _promotion = 0, int _enPassantTarget = 0)
    {
        startSquare = _startSquare;
        targetSquare = _targetSquare;
        tag = _tag;
        castleAdj = _castleAdj;
        capture = _capture;
        promotion = _promotion;
        enPassantTarget = _enPassantTarget;
    }
}

public struct CastleState
{
    public int WQ;
    public int WK;
    public int BQ;
    public int BK;
    public CastleState(int _WQ, int _WK, int _BQ, int _BK)
    {
        WQ = _WQ;
        WK = _WK;
        BQ = _BQ;
        BK = _BK;
    }
}

