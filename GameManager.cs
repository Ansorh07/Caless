using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Главный менеджер игры Caless.
/// Управляет всеми режимами: vs AI, локальный, Bluetooth, обучение.
/// Координирует движок, рендеринг, UI и ввод.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameMode { None, VsComputer, LocalMultiplayer, Bluetooth, Training }
    public enum PlayerSide { White, Black }
 
    [HideInInspector] public CalessEngine engine;
    [HideInInspector] public CalessAI ai;
    [HideInInspector] public BoardRenderer boardRenderer;
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public SettingsManager settings;
    [HideInInspector] public StatisticsManager statistics;
    [HideInInspector] public TrainingManager trainingManager;
    [HideInInspector] public BluetoothManager bluetoothManager;
 
    public GameMode currentMode = GameMode.None;
    public bool isPlayerTurn = true;
    public int moveCount = 0;
    public List<int> playerCapturedPieces = new List<int>();
    public List<int> aiCapturedPieces = new List<int>();
    public Sprite[] pieceSprites;
 
    private PlayerSide playerSide = PlayerSide.White;
    private CalessAI.Difficulty aiDifficulty = CalessAI.Difficulty.Easy;
    private Camera mainCam;
 
    private Vector2Int selectedSquare = new Vector2Int(-1, -1);
    private List<Move> selectedMoves = new List<Move>();
    private bool gameActive = false;
 
    // Специальные режимы
    private bool templeMode = false;
    private Vector2Int templePiece = new Vector2Int(-1, -1);
    private bool castlingMode = false;
    private bool pendingRevive = false;
    private Move lastMoveForRevive;

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;
 
        InitializeComponents();
        uiManager.ShowMainMenu();
        PieceSpritesHolder spriteHolder = gameObject.AddComponent<PieceSpritesHolder>();
    }
 
    private void InitializeComponents()
    {
        settings = gameObject.AddComponent<SettingsManager>();
        settings.Load();

        statistics = gameObject.AddComponent<StatisticsManager>();
        statistics.Load();
 
        boardRenderer = gameObject.AddComponent<BoardRenderer>();
        boardRenderer.SetSettings(settings);

        LocalizationManager.SetLanguageByIndex(settings.Language);
 
        uiManager = gameObject.AddComponent<UIManager>();
        uiManager.gameManager = this;
 
        trainingManager = gameObject.AddComponent<TrainingManager>();
        trainingManager.gameManager = this;
 
        bluetoothManager = gameObject.AddComponent<BluetoothManager>();
        bluetoothManager.gameManager = this;
 
        mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6.5f;
            mainCam.backgroundColor = new Color(0.06f, 0.05f, 0.04f);
            mainCam.transform.position = new Vector3(0, 0.5f, -10);
        }
 
    }
    // ==================== ЗАПУСК ИГР ====================
 
    public void StartGame(CalessAI.Difficulty difficulty, PlayerSide side)
    {
        currentMode = GameMode.VsComputer;
        aiDifficulty = difficulty;
        playerSide = side;
 
        engine = new CalessEngine();
        engine.InitializeBoard();
 
        ai = new CalessAI();
        ai.difficulty = difficulty;
 
        ResetGameState();
 
        boardRenderer.isBoardFlipped = (side == PlayerSide.Black);
        boardRenderer.Initialize(engine);
 
        isPlayerTurn = (side == PlayerSide.White);
        gameActive = true;
 
        uiManager.ShowGameUI();
 
        if (!isPlayerTurn)
            StartCoroutine(AITurn());
    }

    public void StartLocalMultiplayer()
    {
        currentMode = GameMode.LocalMultiplayer;
 
        engine = new CalessEngine();
        engine.InitializeBoard();
 
        ResetGameState();
 
        boardRenderer.isBoardFlipped = false;
        boardRenderer.Initialize(engine);
 
        isPlayerTurn = true;
        gameActive = true;
 
        uiManager.ShowGameUI();
    }
 
    public void StartBluetoothGame()
    {
        currentMode = GameMode.Bluetooth;
        uiManager.ShowBluetoothUI(OnBluetoothHost, OnBluetoothJoin, () => ShowMenu());
    }
 
    public void StartTraining()
    {
        currentMode = GameMode.Training;
        trainingManager.StartTraining();
    }
 
    public void ShowMenu()
    {
        gameActive = false;
        currentMode = GameMode.None;
        StopAllCoroutines();
        uiManager.ShowMainMenu();
    }
 
    public void RestartGame()
    {
        if (currentMode == GameMode.VsComputer)
            StartGame(aiDifficulty, playerSide);
        else if (currentMode == GameMode.LocalMultiplayer)
            StartLocalMultiplayer();
    }

    private void ResetGameState()
    {
        moveCount = 0;
        playerCapturedPieces.Clear();
        aiCapturedPieces.Clear();
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        templeMode = false;
        castlingMode = false;
        pendingRevive = false;
    }

    // ==================== ВВОД ====================
 
    void Update()
    {
        if (!gameActive) return;
 
        if (currentMode == GameMode.Training)
        {
            trainingManager.HandleInput();
            return;
        }
 
        if (!isPlayerTurn && currentMode != GameMode.LocalMultiplayer) return;
        if (boardRenderer.IsAnimating) return;
 
        HandleGameInput();
    }
 
    private void HandleGameInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (mainCam == null) 
        {
            mainCam = Camera.main;
            if (mainCam == null) return;  // ✅ Защита
        }

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2Int boardPos = boardRenderer.GetBoardPosition(mouseWorld);

        if (!CalessEngine.InBounds(boardPos.x, boardPos.y)) return;

        // ✅ Правильная обработка templeMode
        if (templeMode)
        {
            HandleTempleInput(boardPos);
            return;
        }
 
        if (selectedSquare.x >= 0)
        {
            HandleMoveInput(boardPos);
            return;
        }

        HandlePieceSelection(boardPos);
    }

    private void HandlePieceSelection(Vector2Int pos)
    {
        int piece = engine.board[pos.x, pos.y];
        if (piece == CalessEngine.EMPTY) return;
 
        bool pieceIsWhite = CalessEngine.IsWhite(piece);
        if (currentMode == GameMode.LocalMultiplayer)
        {
            if (pieceIsWhite != engine.whiteTurn) return;
        }

        else
        {
            bool playerIsWhite = (playerSide == PlayerSide.White);
            if (pieceIsWhite != playerIsWhite) return;
        }
 
        SelectPiece(pos);
    }
 
    private void SelectPiece(Vector2Int pos)
    {
        selectedSquare = pos;
        selectedMoves = engine.GetLegalMovesForPiece(pos.x, pos.y);
 
        if (castlingMode)
        {
            int type = CalessEngine.PieceType(engine.board[pos.x, pos.y]);
            if (type == CalessEngine.DRAGON)
            {
                List<Move> castleMoves = engine.GetCastlingMoves(engine.whiteTurn);
                selectedMoves.AddRange(castleMoves);
            }
        }
 
        boardRenderer.ClearHighlights();
        boardRenderer.ShowSelectedHighlight(pos.x, pos.y);
 
        // Check if ShowPossibleMoves is enabled in settings
        bool showMoves = (settings != null) ? settings.ShowPossibleMoves : true;
        
        if (showMoves)
        {
            foreach (Move m in selectedMoves)
            {
                if (m.isCastling)
                    boardRenderer.ShowCastleHint(m.to.x, m.to.y);
                else
                {
                    bool isCapture = m.captured != CalessEngine.EMPTY || m.isDragonRanged;
                    boardRenderer.ShowMoveHint(m.to.x, m.to.y, isCapture);
                }
            }
        }
        // Highlight Kal (royal piece in check)
        if (engine.IsInCheck(engine.whiteTurn))
        {
            Vector2Int royalPos = GetVulnerableRoyalPos(engine.whiteTurn);
            if (royalPos.x >= 0)
                boardRenderer.ShowKalHighlight(royalPos.x, royalPos.y);
        }
    }

    private void HandleMoveInput(Vector2Int targetPos)
    {
        Move? targetMove = null;
        foreach (Move m in selectedMoves)
        {
            if (m.to == targetPos)
            {
                targetMove = m;
                break;
            }
        }
 
        if (targetMove.HasValue)
        {
            ExecutePlayerMove(targetMove.Value);
            return;
        }

        int clicked = engine.board[targetPos.x, targetPos.y];
        if (clicked != CalessEngine.EMPTY)
        {
            bool clickedIsWhite = CalessEngine.IsWhite(clicked);
            bool turnIsWhite = (currentMode == GameMode.LocalMultiplayer) ?
                engine.whiteTurn : (playerSide == PlayerSide.White);
 
            if (clickedIsWhite == turnIsWhite)
            {
                SelectPiece(targetPos);
                return;
            }
        }
        DeselectPiece();
    }
 
    private void DeselectPiece()
    {
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        castlingMode = false;
        boardRenderer.ClearHighlights();
    }
 
    // ==================== ВЫПОЛНЕНИЕ ХОДА ====================
    private void ExecutePlayerMove(Move move)
    {
        boardRenderer.ClearHighlights();

        if (move.captured != CalessEngine.EMPTY)
        {
            if (currentMode == GameManager.GameMode.LocalMultiplayer)
            {
                if (engine.whiteTurn)
                    playerCapturedPieces.Add(move.captured);
                else
                    aiCapturedPieces.Add(move.captured);
            }
            else
            {
                playerCapturedPieces.Add(move.captured);
            }
        }

        engine.MakeMove(move);
        moveCount++;

        boardRenderer.SetLastMove(move.from, move.to);
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        castlingMode = false;

        bool animate = (settings != null && settings.AnimationsEnabled);

        if (animate)
        {
            if (move.isCastling)
            {
                // ✅ ФИКС РОКИРОВКИ!
                boardRenderer.AnimateCastling(move, () => {
                    boardRenderer.RefreshPieces();  // Обновляем доску!
                    OnMoveComplete(move);
                });
            }
            else
            {
                boardRenderer.AnimateMove(move, () => {
                    boardRenderer.RefreshPieces();  // Обновляем доску!
                    OnMoveComplete(move);
                });
            }
        }
        else
        {
            boardRenderer.RefreshPieces();
            OnMoveComplete(move);
        }
    }
    
    private void OnMoveComplete(Move move)
    {
        // Сначала проверяем оживление Козла — это часть текущего хода
        if (TryHandleGoatRevive(move, fromAI: false, onContinue: () => ContinueAfterMove(move)))
            return;

        ContinueAfterMove(move);
    }

    private void ContinueAfterMove(Move move)
    {
        if (CheckGameOver()) return;

        if (currentMode == GameManager.GameMode.Bluetooth && bluetoothManager.IsConnected)
            bluetoothManager.SendMove(move);

        if (currentMode == GameManager.GameMode.VsComputer)
        {
            isPlayerTurn = false;
            uiManager.UpdateGameInfo();
            StartCoroutine(AITurn());
        }
        else if (currentMode == GameManager.GameMode.LocalMultiplayer)
        {
            uiManager.UpdateGameInfo();
        }
    }
    // ==================== AI ====================
 
    private IEnumerator AITurn()
    {
        uiManager.ShowThinking(true);
        yield return new WaitForSeconds(0.3f);
 
        Move aiMove = ai.GetBestMove(engine);
 
        uiManager.ShowThinking(false);
 
        if (aiMove.captured != CalessEngine.EMPTY)
            aiCapturedPieces.Add(aiMove.captured);
 
        engine.MakeMove(aiMove);
        moveCount++;
 
        boardRenderer.SetLastMove(aiMove.from, aiMove.to);
 
        bool animate = (settings != null && settings.AnimationsEnabled);
        if (animate)
        {
            if (aiMove.isCastling)
                boardRenderer.AnimateCastling(aiMove, () => OnAIMoveComplete());
            else
                boardRenderer.AnimateMove(aiMove, () => OnAIMoveComplete());
        }
        else
        {
            boardRenderer.RefreshPieces();
            OnAIMoveComplete();
        }
    }

    private void OnAIMoveComplete()
    {
        boardRenderer.RefreshPieces();

        // Дать AI шанс оживить съеденную фигуру (Козлом рядом)
        Move lastAiMove = (engine.moveHistory.Count > 0)
            ? engine.moveHistory[engine.moveHistory.Count - 1].move
            : default;

        if (TryHandleGoatRevive(lastAiMove, fromAI: true, onContinue: () => {
                if (CheckGameOver()) return;
                isPlayerTurn = true;
                uiManager.UpdateGameInfo();
            }))
            return;

        if (CheckGameOver()) return;
 
        isPlayerTurn = true;
        uiManager.UpdateGameInfo();
    }

    // ==================== ПРОВЕРКА КОНЦА ИГРЫ ====================
 
    private bool CheckGameOver()
    {
        // Проверяем, погибли ли оба королевских фигуры (Дракон + Тигр)
        // Белые королевские фигуры мертвы → побеждают чёрные
        if (!engine.whiteDragonAlive && !engine.whiteTigerAlive)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Чёрные победили!";
            else // VsComputer
                winner = (playerSide == PlayerSide.Black) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("РАС!", winner);
            return true;
        }
 
        // Чёрные королевские фигуры мертвы → побеждают белые
        if (!engine.blackDragonAlive && !engine.blackTigerAlive)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Белые победили!";
            else // VsComputer
                winner = (playerSide == PlayerSide.White) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("РАС!", winner);
            return true;
        }
 
        // Кал (шах)
        bool currentInCheck = engine.IsInCheck(engine.whiteTurn);
        if (currentInCheck)
        {
            List<Move> legalMoves = engine.GetAllLegalMoves(engine.whiteTurn);
            if (legalMoves.Count == 0)
            {
                gameActive = false;
                string side = engine.whiteTurn ? "чёрные" : "белые";
                string winner;
                if (currentMode == GameMode.LocalMultiplayer)
                    winner = (engine.whiteTurn ? "Чёрные" : "Белые") + " победили!";
                else
                    winner = isPlayerTurn ? "Вы проиграли!" : "Вы выиграли!";
                uiManager.ShowGameOver("РАС!", winner);
                return true;
            }
        }
        else
        {
            // Пат
            List<Move> legalMoves = engine.GetAllLegalMoves(engine.whiteTurn);
            if (legalMoves.Count == 0)
            {
                gameActive = false;
                uiManager.ShowGameOver("Пат", "Ничья — нет доступных ходов");
                return true;
            }
        }
        return false;
    }

    // ==================== СПЕЦСПОСОБНОСТИ ====================
 
    public void ActivateTemple()
    {
        if (!gameActive || !isPlayerTurn) return;

        bool isWhite = (currentMode == GameMode.LocalMultiplayer) ?
            engine.whiteTurn : (playerSide == PlayerSide.White);
 
        if (isWhite && engine.whiteTempleUsed)
        {
            uiManager.ShowDialog("Храм", "Храм уже использован!", "OK", null);
            return;
        }
        if (!isWhite && engine.blackTempleUsed)
        {
            uiManager.ShowDialog("Храм", "Храм уже использован!", "OK", null);
            return;
        }

        // Храм работает ТОЛЬКО если ваша фигура стоит на клетке Храма
        if (!engine.CanUseTemple(isWhite))
        {
            uiManager.ShowDialog("Храм",
                "Сначала поставьте свою фигуру на клетку Храма!", "OK", null);
            return;
        }

        templeMode = !templeMode;
        if (templeMode)
        {
            templePiece = new Vector2Int(-1, -1);
            boardRenderer.ClearHighlights();
            uiManager.ShowDialog("Храм", "Выберите фигуру для телепортации", "OK", null);
        }
        else
        {
            boardRenderer.ClearHighlights();
        }
    }
 
    private void HandleTempleInput(Vector2Int pos)
    {
        bool isWhite = (currentMode == GameMode.LocalMultiplayer) ?
            engine.whiteTurn : (playerSide == PlayerSide.White);
 
        if (templePiece.x < 0)
        {
            // Выбираем фигуру для телепортации — ТОЛЬКО фигура на клетке Храма
            if (pos.x != CalessEngine.TEMPLE_SQUARE.x || pos.y != CalessEngine.TEMPLE_SQUARE.y)
                return;
            int piece = engine.board[pos.x, pos.y];
            if (piece == CalessEngine.EMPTY) return;
            if (CalessEngine.IsWhite(piece) != isWhite) return;
 
            templePiece = pos;
            boardRenderer.ClearHighlights();
            boardRenderer.ShowSelectedHighlight(pos.x, pos.y);
 
            // Подсветить все пустые клетки
            for (int r = 0; r < CalessEngine.BOARD_SIZE; r++)
                for (int c = 0; c < CalessEngine.BOARD_SIZE; c++)
                    if (engine.board[r, c] == CalessEngine.EMPTY)
                        boardRenderer.ShowTeleportHint(r, c);
        }
        else
        {
            // Выбираем место телепортации
            if (engine.board[pos.x, pos.y] != CalessEngine.EMPTY)
            {
                templeMode = false;
                boardRenderer.ClearHighlights();
                return;
            }
 
            Move teleportMove = new Move();
            teleportMove.from = templePiece;
            teleportMove.to = pos;
            teleportMove.piece = engine.board[templePiece.x, templePiece.y];
            teleportMove.captured = CalessEngine.EMPTY;
            teleportMove.isTeleport = true;
 
            engine.MakeMove(teleportMove);
            moveCount++;
 
            boardRenderer.SetLastMove(templePiece, pos);
            boardRenderer.ClearHighlights();
            boardRenderer.RefreshPieces();
 
            templeMode = false;
            templePiece = new Vector2Int(-1, -1);
 
            if (currentMode == GameMode.VsComputer)
            {
                isPlayerTurn = false;
                uiManager.UpdateGameInfo();
                StartCoroutine(AITurn());
            }
            else
            {
                uiManager.UpdateGameInfo();
            }
        }
    }
 
    public void ActivateCastling()
    {
        if (!gameActive || !isPlayerTurn) return;

        bool isWhite = (currentMode == GameMode.LocalMultiplayer) ? 
            engine.whiteTurn : (playerSide == PlayerSide.White);

        List<Move> castleMoves = engine.GetCastlingMoves(isWhite);
        if (castleMoves.Count == 0)
        {
            uiManager.ShowDialog("Сдвиг", "Рокировка недоступна!", "OK", null);
            return;
        }

        castlingMode = true;
        boardRenderer.ClearHighlights();

        // ✅ Подсвечиваем Дракона и возможные рокировки
        Vector2Int dragonPos = isWhite ? engine.whiteDragonPos : engine.blackDragonPos;
        boardRenderer.ShowSelectedHighlight(dragonPos.x, dragonPos.y);
        
        foreach (Move m in castleMoves)
        {
            boardRenderer.ShowCastleHint(m.to.x, m.to.y);  // Цель Дракона
        }

        selectedSquare = dragonPos;
        selectedMoves = castleMoves;
    }
    // ==================== КОЗЁЛ ====================

    /// <summary>
    /// Проверяет, может ли только что сходивший игрок оживить съеденную фигуру
    /// (Козлом, стоящим рядом). При наличии возможности показывает диалог,
    /// возвращает true и приостанавливает дальнейший поток. onContinue вызывается
    /// в любой ветке диалога после завершения оживления/отказа.
    /// Возвращает false, если оживление неприменимо — поток продолжается обычным образом.
    /// </summary>
    private bool TryHandleGoatRevive(Move move, bool fromAI, System.Action onContinue)
    {
        if (move.piece == CalessEngine.EMPTY) return false;
        if (move.captured == CalessEngine.EMPTY) return false;
        int capType = CalessEngine.PieceType(move.captured);
        if (capType == CalessEngine.DRAGON || capType == CalessEngine.TIGER) return false;

        // Игрок, сделавший ход (whiteTurn уже переключён в MakeMove)
        bool moverIsWhite = !engine.whiteTurn;

        if (!engine.CheckGoatRevive(move, moverIsWhite))
            return false;

        List<Vector2Int> revivePositions = engine.GetRevivePositions();
        if (revivePositions.Count == 0)
        {
            engine.pendingRevive = false;
            return false;
        }

        // AI оживляет автоматически — берём первую доступную клетку
        if (fromAI)
        {
            ApplyRevive(revivePositions[0]);
            onContinue?.Invoke();
            return true;
        }

        string pieceName = CalessEngine.PieceNames.ContainsKey(capType)
            ? CalessEngine.PieceNames[capType] : "фигура";

        uiManager.ShowReviveDialog(
            pieceName,
            onRevive: () => {
                // Авто-выбираем первую доступную клетку рядом с Козлом
                ApplyRevive(revivePositions[0]);
                boardRenderer.RefreshPieces();
                onContinue?.Invoke();
            },
            onSkip: () => {
                engine.pendingRevive = false;
                onContinue?.Invoke();
            });

        return true;
    }

    private void ApplyRevive(Vector2Int pos)
    {
        if (!engine.pendingRevive) return;

        Move reviveMove = new Move();
        reviveMove.from = pos;          // источник не используется
        reviveMove.to = pos;
        reviveMove.piece = engine.pendingRevivePiece;
        reviveMove.captured = CalessEngine.EMPTY;
        reviveMove.isRevive = true;
        reviveMove.revivedPiece = engine.pendingRevivePiece;

        // Оживление выполняется молча — ход остаётся за тем же игроком
        engine.MakeMove(reviveMove, silent: true);
        engine.pendingRevive = false;
    }

    // ==================== BLUETOOTH ====================

    private void OnBluetoothHost()
    {
        bluetoothManager.Initialize(OnBTMoveReceived, OnBTChatReceived, OnBTConnected, OnBTDisconnected);
        bluetoothManager.StartHost();
    }

    private void OnBluetoothJoin()
    {
        bluetoothManager.Initialize(OnBTMoveReceived, OnBTChatReceived, OnBTConnected, OnBTDisconnected);
        bluetoothManager.StartClient();
    }

    private void OnBTConnected()
    {
        engine = new CalessEngine();
        engine.InitializeBoard();
        ResetGameState();

        playerSide = bluetoothManager.IsHost ? PlayerSide.White : PlayerSide.Black;
        boardRenderer.isBoardFlipped = !bluetoothManager.IsHost;
        boardRenderer.Initialize(engine);

        isPlayerTurn = bluetoothManager.IsHost;
        gameActive = true;

        uiManager.ShowGameUI();
    }

    private void OnBTDisconnected()
    {
        gameActive = false;
        uiManager.ShowDialog("Соединение", "Соединение потеряно", "В меню", () => ShowMenu());
    }

    private void OnBTChatReceived(string message)
    {
        uiManager.ShowDialog("Чат", message, "OK", null);
    }
        
    private void OnBTMoveReceived(string moveData)
    {
        string[] parts = moveData.Split(',');
        if (parts.Length < 4) return;

        int fr = int.Parse(parts[0]);
        int fc = int.Parse(parts[1]);
        int tr = int.Parse(parts[2]);
        int tc = int.Parse(parts[3]);

        Move move = new Move();  // ✅ Объявление ПЕРЕД использованием
        move.from = new Vector2Int(fr, fc);
        move.to = new Vector2Int(tr, tc);
        move.piece = engine.board[fr, fc];
        move.captured = engine.board[tr, tc];

        // ✅ ФИКС Contains()
        if (moveData.Contains("CASTLE"))
        {
            move.isCastling = true;
            int castleIdx = System.Array.IndexOf(parts, "CASTLE");
            if (castleIdx >= 0 && castleIdx + 4 < parts.Length)
            {
                move.castlePieceFrom = new Vector2Int(int.Parse(parts[castleIdx + 1]), int.Parse(parts[castleIdx + 2]));
                move.castlePieceTo = new Vector2Int(int.Parse(parts[castleIdx + 3]), int.Parse(parts[castleIdx + 4]));
            }
        }
        
        if (moveData.Contains("TELEPORT")) move.isTeleport = true;
        if (moveData.Contains("RANGED")) move.isDragonRanged = true;

        if (move.captured != CalessEngine.EMPTY)
            aiCapturedPieces.Add(move.captured);

        engine.MakeMove(move);
        moveCount++;

        boardRenderer.SetLastMove(move.from, move.to);
        boardRenderer.RefreshPieces();

        if (CheckGameOver()) return;

        isPlayerTurn = true;
        uiManager.UpdateGameInfo();
    }
 
    // ==================== УТИЛИТЫ ====================
 
    private Vector2Int GetVulnerableRoyalPos(bool forWhite)
    {
        if (forWhite)
        {
            if (engine.whiteDragonAlive && !engine.whiteTigerAlive)
                return engine.whiteDragonPos;
            if (!engine.whiteDragonAlive && engine.whiteTigerAlive)
                return engine.whiteTigerPos;
        }
        else
        {
            if (engine.blackDragonAlive && !engine.blackTigerAlive)
                return engine.blackDragonPos;
            if (!engine.blackDragonAlive && engine.blackTigerAlive)
                return engine.blackTigerPos;
        }
        return new Vector2Int(-1, -1);
    }
}