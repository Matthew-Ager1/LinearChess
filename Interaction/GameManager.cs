using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Custom Games")]
    public string PawnPocolypse_FEN;
    public string QueenRush_FEN;
    public string Stronghold_FEN;
    public string RoundTable_FEN;
    public string HellsGate_FEN;
    public string TheCalvery_FEN;
    public string BishopPrison_FEN;
    public List<string> EndGame_FENS;
    public GameObject CustomGameCanvas;
    [Header("Effects")]
    public ParticleSystem WhiteExitParticles;
    public ParticleSystem BlackExitParticles;
    public ParticleSystem EnterParticles;
    public ParticleSystem KillEnterParticles;

    [Header("Interaction")]
    public GameObject interactionSquare;
    public GameObject _DisplaySquare;
    public GameObject boardSquare;
    List<GameObject> DisplaySquares = new List<GameObject>();
    public InputField FENtextField;
    public Image engineColDisplay;
    public Slider evalBar;

    [Header("Design")]
    public Color selectedPieceColor;
    public Color targetSquareColor;
    public Color selectedSquareColor;
    public Color nullColor;
    public Color bsWhite;
    public Color bsBlack;

    [Header("Pieces")]
    public GameObject WhiteQueen;
    public GameObject BlackQueen;

    public GameObject WhitePawn;
    public GameObject BlackPawn;

    public GameObject WhiteBishop;
    public GameObject BlackBishop;

    public GameObject WhiteRook;
    public GameObject BlackRook;

    public GameObject WhiteKing;
    public GameObject BlackKing;

    public GameObject WhiteKnight;
    public GameObject BlackKnight;

    Bitboard bb = new Bitboard();

    List<GameObject> ActivePieces = new List<GameObject>();

    //Game Control
    bool colorToMove = true;
    int selectedPiece = -1;
    //End Game Control

    Camera cam;

    bool gameStarted = false;

    int searchDepth = 4;

    [Header("Misc")]

    public GameObject gameCanvas;

    public AudioSource whiteMove;
    public AudioSource blackMove;

    public Image turnDisplay;

    public Text NodesSearchedText;
    public Text EvalText;

    private List<GameObject> tempBoardDisplaySquresBlack = new List<GameObject>();
    private List<GameObject> tempBoardDisplaySquresWhite = new List<GameObject>();

    public string FEN;

    bool engineColor = true;
    bool firstMove = true;
    bool useEngine = true;

    float engineWaitTime = .5f;

    Bitboard prevEngineBB;

    int repeatCnt = 0;
    private void Awake()
    {
        cam = Camera.main;
        Movement.InitAttackPatterns();
        bb.InitilizeBitboard();
        CreatePieces();
        InitPhysicalSquares();
        Utility.InitHistoryTable();
        TranspositionTable.InitilizeHashTable();

        if (PlayerPrefs.GetString("Load_FEN", "") != "")
        {
            LoadFEN(PlayerPrefs.GetString("Load_FEN"));
            PlayerPrefs.SetString("Load_FEN", "");
        }

    }

    public void LoadFEN(string s)
    {
        bb = Utility.FEN_To_Bitboard(s);
        colorToMove = bb.ColorToMove;
        DestoryPieces();
        CreatePieces();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && useEngine)
        {
            UndoEngineMove();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(Utility.Bitboard_To_FEN(bb));
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            List<Move> checkMoves = bb.GenerateMovesPawns(false);
            bb.DebugMoveList(checkMoves);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(bb.Evaluate());
        }

        UpdateColors();
        if (gameStarted)
        {
            if (engineColor && firstMove && useEngine)
            {
                Invoke("Invokeable_MakeEngineMove", 1);
                firstMove = false;    
            }
               
            if (engineColor != colorToMove || !useEngine)
            {
                GameControl();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
        } 
    }

    public void UndoEngineMove()
    {
        bb = prevEngineBB;
        bb.ColorToMove = engineColor;
        colorToMove = engineColor;
        DestoryPieces();
        CreatePieces();
        ClearDisplaySquares();
    }

    public void Invokeable_MakeEngineMove()
    {
        MakeEngineMove();
    }
    public void MakeEngineMove(bool canRepeat = true)
    {
        prevEngineBB = bb.Duplicate();
        ChessEngine.maxDepth = searchDepth;

        MoveScore choice = ChessEngine.Negamax(bb, searchDepth, colorToMove, -Mathf.Infinity, Mathf.Infinity);
        //MoveScore choice = ChessEngine.NegaScoutItterativeDeepening(bb, searchDepth, colorToMove);

        bb.MakeMove(choice.move);

        DestoryPieces();
        CreatePieces();
        PlaySound();
        colorToMove = colorToMove ? false : true;
        bb.ColorToMove = colorToMove;


        //Display Features
        NodesSearchedText.text = ChessEngine.count.ToString();

        float evaluation = bb.Evaluate();

        EvalText.text = evaluation.ToString();

        ClearDisplaySquares();
        DisplayPieceMovement(choice.move.startSquare, new int[0]);
        DisplayChosenMove(choice.move.targetSquare);

        SetEvalBar(evaluation);

        DisplayMoveEffects(choice.move);
        //End Display Features

        if (ChessEngine.count < 30000 && canRepeat && searchDepth >= 5)
        {         
            ChessEngine.count = 0;
            UndoEngineMove();
            searchDepth++;
            MakeEngineMove(false);
            searchDepth--;
            
            return;
        }

        ChessEngine.count = 0;

        repeatCnt = 0;
    }

    public void GameControl()
    {
        //Piece Selection
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CamCast();
            if (hit.collider != null)
            {
                int x = (int)hit.collider.gameObject.transform.position.x;
                int y = (int)hit.collider.gameObject.transform.position.y;

                char[] board = bb.CreateCharArr();
                if (GetColor(board[x + y * 8]) == colorToMove)
                {
                    ClearDisplaySquares();
                    selectedPiece = x + y * 8;
                    DisplayPieceMovement(x + y * 8, ConvertMoveListToTargetList(GetMovesListForPiece(board[x + y * 8], x + y * 8)));


                    //DELEATE THIS ITS FOR DEBUG
                    DestoryPieces();
                    CreatePieces();
                }
                else
                {
                    ClearDisplaySquares();
                    selectedPiece = -1;
                }
            }
            else
            {
                ClearDisplaySquares();
                selectedPiece = -1;
            }
        }
        //End Piece Selection

        //Piece Movement
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit = CamCast();
            if (hit.collider != null)
            {
                int x = (int)hit.collider.gameObject.transform.position.x;
                int y = (int)hit.collider.gameObject.transform.position.y;

                char[] board = bb.CreateCharArr();
                if (selectedPiece > board.Length) //avoid error when pressing wrong piece
                {
                    return;
                }
                if (board[selectedPiece] != '\0')
                {
                    Move m = IsMoveLegal(selectedPiece, x + y * 8, board[selectedPiece]);
                    if (m.startSquare != -1)
                    {
                        m.capture = bb.GetMoveScore(m);
                        //Castle Logic
                        int castleAdj = GetCastleAdj(selectedPiece, x + y * 8, board[selectedPiece]);

                        //Pawn Promotion Logic and EnPassant
                        int promotion = 0;
                        int enPassantTarget = 0;
                        if (board[selectedPiece] == "P"[0])
                        {
                            if (x + y * 8 >= 56)
                            {
                                promotion = 1;
                            }

                            if (board[x + y * 8] == 0 && board[(x + y * 8) - 8] == "p"[0])
                            {
                                Debug.Log("Set En Passant Data");
                                enPassantTarget = (x + y * 8) - 8;
                            }
                        }
                        else if (board[selectedPiece] == "p"[0])
                        {
                            if (x + y * 8 <= 7)
                            {
                                promotion = 2;
                            }

                            if (board[x + y * 8] == 0 && board[(x + y * 8) + 8] == "P"[0])
                            {
                                enPassantTarget = (x + y * 8) + 8;
                            }
                        }

                        bb.MakeMove(new Move(selectedPiece, x + y * 8, CharToTag(board[selectedPiece]), castleAdj, 0, promotion, enPassantTarget));

                        bb.ResetEnPassents(!colorToMove);

                        DisplayChosenMove(x + y * 8);

                        DisplayMoveEffects(m);

                        PlaySound();

                        DestoryPieces();
                        CreatePieces();
                        colorToMove = colorToMove ? false : true;
                        bb.ColorToMove = colorToMove;

                        float evaluation = bb.Evaluate();

                        evaluation -= 100;

                        SetEvalBar(evaluation);

                        EvalText.text = evaluation.ToString();
                        
                        if (useEngine)
                        {
                            Invoke("Invokeable_MakeEngineMove", engineWaitTime);
                        }
                        
                    }
                }
                
            }
        }

    }

    public void CreatePieces()
    {
        char[] board = bb.CreateCharArr();
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != '\0')
            {
                int x = i;
                int y = 0;
                while (x > 7)
                {
                    x -= 8;
                    y++;
                }
                ActivePieces.Add(Instantiate(CharToObject(board[i]), new Vector3(x, y, 0), Quaternion.identity));
            }
        }
    }
    public void DestoryPieces()
    {
        for (int i = 0; i < ActivePieces.Count; i++)
        {
            Destroy(ActivePieces[i]);
        }
        ActivePieces.Clear();
    }

    /*Char Chart
     * B = 66 | b = 98
     * K = 75 | k = 107
     * N = 78 | n = 110
     * P = 80 | p = 112
     * Q = 81 | q = 113
     * R = 82 | r = 114
     */

    public GameObject CharToObject(char occ)
    {
        switch ((int)occ)
        {
            case 66:
                return WhiteBishop;
            case 98:
                return BlackBishop;
            case 75:
                return WhiteKing;
            case 107:
                return BlackKing;
            case 78:
                return WhiteKnight;
            case 110:
                return BlackKnight;
            case 80:
                return WhitePawn;
            case 112:
                return BlackPawn;
            case 81:
                return WhiteQueen;
            case 113:
                return BlackQueen;
            case 82:
                return WhiteRook;
            case 114:
                return BlackRook;
        }

        Debug.LogError("Could Not Find Piece = " + occ);
        return null;
    }

    /*Tag Chart:
     * B = 3   b = 9
     * K = 6   k = 12
     * N = 2   n = 8
     * P = 1   p = 7
     * Q = 5   q = 11
     * R = 4   r = 10
    */

    public int CharToTag(char occ)
    {
        switch ((int)occ)
        {
            case 66:
                return 3;
            case 98:
                return 9;
            case 75:
                return 6;
            case 107:
                return 12;
            case 78:
                return 2;
            case 110:
                return 8;
            case 80:
                return 1;
            case 112:
                return 7;
            case 81:
                return 5;
            case 113:
                return 11;
            case 82:
                return 4;
            case 114:
                return 10;
        }

        Debug.LogError("Could not find piece = " + occ);
        return -1;
    }

    public bool GetColor(char occ)
    {
        if (occ >= 65 && occ <= 90)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitPhysicalSquares()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Instantiate(interactionSquare, new Vector3(x, y, 0), Quaternion.identity);           
            }
        }

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                DisplaySquares.Add(Instantiate(_DisplaySquare, new Vector3(x, y, .1f), Quaternion.identity));
            }
        }

        bool black = false;
        for (int y = 0; y < 8; y++)
        {
            black = !black;
            for (int x = 0; x < 8; x++)
            {
                GameObject tempGm = Instantiate(boardSquare, new Vector3(x, y, .2f), Quaternion.identity);
                if (black)
                {
                    tempGm.GetComponent<SpriteRenderer>().color = bsBlack;
                    tempBoardDisplaySquresBlack.Add(tempGm);
                }
                else
                {
                    tempGm.GetComponent<SpriteRenderer>().color = bsWhite;
                    tempBoardDisplaySquresWhite.Add(tempGm);
                }
                black = !black;
            }
        }
    }

    public RaycastHit CamCast()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 1 << 12))
        {
            return hit;
        }
        return hit;
    }

    public Move IsMoveLegal(int start, int end, char occ)
    {
        List<Move> moves = new List<Move>();
        switch ((int)occ)
        {
            case 66:
                moves = bb.GenerateMovesBishop(true, false);
                break;
            case 98:
                moves = bb.GenerateMovesBishop(false, false);
                break;
            case 75:
                moves = bb.GenerateMovesKing(true, false);
                break;
            case 107:
                moves = bb.GenerateMovesKing(false, false);
                break;
            case 78:
                moves = bb.GenerateMovesKnight(true, false);
                break;
            case 110:
                moves = bb.GenerateMovesKnight(false, false);
                break;
            case 80:
                moves = bb.GenerateMovesPawns(true);
                break;
            case 112:
                moves = bb.GenerateMovesPawns(false);
                break;
            case 81:
                moves = bb.GenerateMovesQueen(true, false);
                break;
            case 113:
                moves = bb.GenerateMovesQueen(false, false);
                break;
            case 82:
                moves = bb.GenerateMovesRook(true, false);
                break;
            case 114:
                moves = bb.GenerateMovesRook(false, false);
                break;
        }

        if (moves.Count == 0)
        {
            Debug.LogError("No moves or Could not find piece = " + occ);
        }

        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].startSquare == start && moves[i].targetSquare == end)
            {
                return moves[i];
            }
        }
        return new Move(-1, -1, -1, -1, -1);
    }

    public int GetCastleAdj(int start, int end, char occ)
    {
        int castleAdj = 0;
        if (occ == "K"[0])
        {
            if (start - end == -2)
            {
                castleAdj = 7;
            }
            else if (start - end == 2)
            {
                castleAdj = 8;
            }
            else
            {
                castleAdj = 5;
            }
        }
        else if (occ == "k"[0])
        {
            if (start - end == -2)
            {
                castleAdj = 9;
            }
            else if (start - end == 2)
            {
                castleAdj = 10;
            }
            else
            {
                castleAdj = 6;
            }
        }
        else if (occ == "R"[0] && start == 7)
        {
            castleAdj = 2;
        }
        else if (occ == "R"[0] && start == 0)
        {
            castleAdj = 1;
        }
        else if (occ == "r"[0] && start == 63)
        {
            castleAdj = 4;
        }
        else if (occ == "r"[0] && start == 56)
        {
            castleAdj = 3;
        }

        return castleAdj;
    }

    public void SetDifficulty(int depth)
    {
        if (depth == 4)
        {
            engineWaitTime = 1;
        }
        searchDepth = depth;
        gameStarted = true;
        gameCanvas.SetActive(false);
    }

    public void PlaySound()
    {
        if (colorToMove)
        {
            whiteMove.Play();
            turnDisplay.color = Color.black;
        }
        else
        {
            blackMove.Play();
            turnDisplay.color = Color.white;
        }
    }

    public List<Move> GetMovesListForPiece(char piece, int start)
    {
        List<Move> moves = new List<Move>();
        switch ((int)piece)
        {
            case 66:
                moves = bb.GenerateMovesBishop(true, false);
                break;
            case 98:
                moves = bb.GenerateMovesBishop(false, false);
                break;
            case 75:
                moves = bb.GenerateMovesKing(true, false);
                break;
            case 107:
                moves = bb.GenerateMovesKing(false, false);
                break;
            case 78:
                moves = bb.GenerateMovesKnight(true, false);
                break;
            case 110:
                moves = bb.GenerateMovesKnight(false, false);
                break;
            case 80:
                moves = bb.GenerateMovesPawns(true);
                break;
            case 112:
                moves = bb.GenerateMovesPawns(false);
                break;
            case 81:
                moves = bb.GenerateMovesQueen(true, false);
                break;
            case 113:
                moves = bb.GenerateMovesQueen(false, false);
                break;
            case 82:
                moves = bb.GenerateMovesRook(true, false);
                break;
            case 114:
                moves = bb.GenerateMovesRook(false, false);
                break;
        }

        moves = CullMovesNotStartingFrom(start, moves);
        return moves;
    }

    public List<Move> CullMovesNotStartingFrom(int start, List<Move> moves)
    {
        List<Move> newMoves = new List<Move>();
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].startSquare == start)
            {
                newMoves.Add(moves[i]);
            }
        }
        return newMoves;
    }

    public int[] ConvertMoveListToTargetList(List<Move> m)
    {
        int[] legals = new int[m.Count];
        for (int i = 0; i < m.Count; i++)
        {
            legals[i] = m[i].targetSquare;
        }
        return legals;
    }

    public void DisplayPieceMovement(int target, int[] legals)
    {
        DisplaySquares[target].GetComponent<SpriteRenderer>().color = selectedPieceColor;
        for (int i = 0; i < legals.Length; i++)
        {
            DisplaySquares[legals[i]].GetComponent<SpriteRenderer>().color = targetSquareColor;
        }
    }

    public void ClearDisplaySquares()
    {
        foreach (GameObject sq in DisplaySquares)
        {
            sq.GetComponent<SpriteRenderer>().color = nullColor;
        }
    }

    public void DisplayChosenMove(int target)
    {
        foreach (GameObject sq in DisplaySquares)
        {
            if (sq.GetComponent<SpriteRenderer>().color == targetSquareColor)
            {
                sq.GetComponent<SpriteRenderer>().color = nullColor;
            }
        }

        DisplaySquares[target].GetComponent<SpriteRenderer>().color = selectedSquareColor;
    }

    public void UpdateColors()
    {
        for (int i = 0; i < tempBoardDisplaySquresBlack.Count; i++)
        {
            tempBoardDisplaySquresBlack[i].GetComponent<SpriteRenderer>().color = bsBlack;
        }
        for (int i = 0; i < tempBoardDisplaySquresWhite.Count; i++)
        {
            tempBoardDisplaySquresWhite[i].GetComponent<SpriteRenderer>().color = bsWhite;
        }
    }

    public void DisplayMoveEffects(Move m)
    {
        int startX = m.startSquare;
        int startY = 0;
        while (startX >= 8)
        {
            startX -= 8;
            startY++;
        }

        int finalX = m.targetSquare;
        int finalY = 0;
        while (finalX >= 8)
        {
            finalX -= 8;
            finalY++;
        }
        
        if (m.capture != 0)
        {
            Instantiate(KillEnterParticles, new Vector3(finalX, finalY, .01f), Quaternion.identity);
        }

    }

    public void ImportFEN()
    {
        FEN = FENtextField.text;
        PlayerPrefs.SetString("Load_FEN", FEN);
        SceneManager.LoadScene(0);
    }

    public void LoadLocalFEN(string _fen)
    {
        FEN = _fen;
        PlayerPrefs.SetString("Load_FEN", FEN);
        SceneManager.LoadScene(0);
    }

    public void SetColor(bool col)
    {
        engineColor = col;
        engineColDisplay.color = col ? Color.white : Color.black;
    }

    public void StartNoEngine()
    {
        useEngine = false;
        gameStarted = true;
        gameCanvas.SetActive(false);
    }

    public void LoadCustomGame(int game)
    {
        switch (game)
        {
            case 0:
                LoadLocalFEN(PawnPocolypse_FEN);
                return;
            case 1:
                LoadLocalFEN(QueenRush_FEN);
                return;
            case 2:
                LoadLocalFEN(Stronghold_FEN);
                return;
            case 3:
                LoadLocalFEN(EndGame_FENS[Random.Range(0, EndGame_FENS.Count)]);
                return;
            case 4:
                LoadLocalFEN(RoundTable_FEN);
                return;
            case 5:
                LoadLocalFEN(HellsGate_FEN);
                return;
            case 6:
                LoadLocalFEN(TheCalvery_FEN);
                return;
            case 7:
                LoadLocalFEN(BishopPrison_FEN);
                return;

        }
    }

    public void OpenCustomGamePannel()
    {
        CustomGameCanvas.SetActive(true);
    }

    public void CloseCustomGamePannel()
    {
        CustomGameCanvas.SetActive(false);
    }

    public void SetEvalBar(float eval)
    {
        eval /= 1000;
        eval += .5f;
        eval = 1 - eval;
        //eval = EvalToSliderVal(eval);
        evalBar.value = eval;
    }

    public float EvalToSliderVal(float eval)
    {
        float val = -((2 + Mathf.Pow(eval / 1000, 2)) / (Mathf.Pow(eval / 1000, 2) + 1)) + 2;
        val -= .5f;
        return val;
    }
}
