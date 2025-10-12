using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class generalmoving : MonoBehaviour
{
    private float moveSpeed = 10.0f;

    private bool allowClick = true;

    public GameObject chess;
    public GameObject targetSquare;
    public GameObject curObject;
    public static GameObject curGrid;

    private Transform lastTargetParent;

    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    [SerializeField] public AudioClip seM;
    private AudioSource player;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool save = false;

    public GameObject capturedPiece;
    public GameObject uiPanel;
    private bool battle = false;

    private GameObject retrieve; 
    private GameObject inBattlePiece;

    private GameObject b1;
    private GameObject b2;

    private bool whiteKingHasMoved = false;
    private bool whiteRookKingsideHasMoved = false;  // Rook1 @ H1
    private bool whiteRookQueensideHasMoved = false; // Rook @ A1

    private bool blackKingHasMoved = false;
    private bool blackRookKingsideHasMoved = false;  // Rook01 @ H8
    private bool blackRookQueensideHasMoved = false; // Rook0 @ A8

    private GameObject[,] gridBoard = new GameObject[8, 8];
    private List<GameObject> castlingExtras = new List<GameObject>();
    private bool isCastlingRookMoving = false; 



    [SerializeField] private GameObject whiteQueenPrefab;
    [SerializeField] private GameObject blackQueenPrefab;
    [SerializeField] private GameObject whiteRookPrefab;
    [SerializeField] private GameObject blackRookPrefab;
    [SerializeField] private GameObject whiteBishopPrefab;
    [SerializeField] private GameObject blackBishopPrefab;
    [SerializeField] private GameObject whiteKnightPrefab;
    [SerializeField] private GameObject blackKnightPrefab;

    [SerializeField] private GameObject checkmatePanel;      // drag in your panel
    [SerializeField] private TextMeshProUGUI checkmateText;  // drag in the TMP text

    // keep this as an instance field:
    private bool isWhiteTurn = true;


    [SerializeField] private GameObject inCheckPanel;
    [SerializeField] private TextMeshProUGUI inCheckText;

    // Root that contains ONLY the main board tiles A1..H8 (not the counter board)
    [SerializeField] private Transform mainBoardRoot;

    // === MAIN BOARD TILES CONTAINER ===
    // Assign the object whose DIRECT children are A1..H8 (main board only!)
    [SerializeField] private Transform mainTilesContainer;

    // Castling state flag: prevents "move rook again" in OnMoveCompleted.
    private bool alreadyCastledThisMove = false;


    // ----- En Passant state -----
    private int plyCount = 0; // increments after every completed move (half-move)

    // Valid only for the opponent's *very next* move (expirePly == current plyCount)
    private struct EnPassantInfo
    {
        public Transform targetTile;     // the square the capturing pawn moves TO (the "passed" square)
        public GameObject victimPawn;    // the pawn that just double-stepped and can be captured
        public int expirePly;            // ply on which the opponent may capture
    }
    private EnPassantInfo enPassant;

    // per-move flags
    private bool enPassantThisMove = false;
    private GameObject enPassantVictimThisMove = null;

    // last move bookkeeping so we can detect double-steps
    private GameObject lastMovePiece = null;
    private Transform lastFromTile = null;
    private Transform lastToTile = null;

    // Track last move start/end so we can detect a two-square pawn move
    private Transform lastMoveStartParent;
    private Transform lastMoveEndParent;
    private GameObject lastMovedPiece;


    private readonly Dictionary<string, Transform> mainTiles = new Dictionary<string, Transform>();


    public static generalmoving Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }


    /*void Start()
    {

        if (mainBoardRoot == null)
        {
            Debug.LogError("generalmoving: mainBoardRoot is not assigned in the Inspector. Drag your MAIN board root here.");
            return;
        }

        // Build the gridBoard ONLY from tiles under the main board
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                string gridName = $"{(char)('A' + x)}{y + 1}";
                Transform tile = FindDeepChildByName(mainBoardRoot, gridName);
                if (tile == null)
                {
                    Debug.LogError($"generalmoving: Could not find tile '{gridName}' under mainBoardRoot '{mainBoardRoot.name}'.");
                }
                else
                {
                    gridBoard[x, y] = tile.gameObject;
                }
            }
        }

        // 初始化棋盘格子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                string gridName = $"{(char)('A' + x)}{y + 1}";
                gridBoard[x, y] = GameObject.Find(gridName);
            }
        }

        // 自动检测初始车位置（新增关键逻辑）
        whiteRookKingsideHasMoved = GameObject.Find("H1").transform.childCount == 0;
        whiteRookQueensideHasMoved = GameObject.Find("A1").transform.childCount == 0;
        blackRookKingsideHasMoved = GameObject.Find("H8").transform.childCount == 0;
        blackRookQueensideHasMoved = GameObject.Find("A8").transform.childCount == 0;

        // 其他原有初始化逻辑...
        uiPanel.SetActive(false);
        player = Camera.main.GetComponent<AudioSource>();

        if (inCheckPanel != null)
            inCheckPanel.SetActive(false);

    }*/

    void Start()
    {
        if (mainBoardRoot == null)
        {
            Debug.LogError("generalmoving: mainBoardRoot is not assigned in the Inspector. Drag your MAIN board root here.");
            return;
        }

        // Build tile index from MAIN board only, and fill gridBoard[]
        BuildMainTileIndex();

        // Detect if initial rooks are present on their corner tiles (main board only)
        whiteRookKingsideHasMoved = !HasRookOnTile("H1", "White");
        whiteRookQueensideHasMoved = !HasRookOnTile("A1", "White");
        blackRookKingsideHasMoved = !HasRookOnTile("H8", "Black");
        blackRookQueensideHasMoved = !HasRookOnTile("A8", "Black");

        uiPanel.SetActive(false);
        player = Camera.main.GetComponent<AudioSource>();

        if (inCheckPanel != null)
            inCheckPanel.SetActive(false);
    }



    // Update is called once per frame
    void Update()
    {
        if (isCastlingRookMoving) return;

        if (!battle)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            /*if (Physics.Raycast(ray, out hit))
            {
                Transform hitTransform = hit.transform;

                if (hitTransform != null && Input.GetMouseButtonDown(0) && allowClick && curObject != null && hitTransform.childCount < 2)
                {
                    // ✅ 特殊情况：castling 附加目标格
                    if (castlingExtras.Contains(hitTransform.gameObject))
                    {
                        bool isWhite = curObject.CompareTag("White");
                        bool kingside = hitTransform.name == (isWhite ? "H1" : "H8");

                        string targetGridName = isWhite
                            ? (kingside ? "G1" : "C1")
                            : (kingside ? "G8" : "C8");

                        GameObject castlingTarget = GameObject.Find(targetGridName);
                        if (castlingTarget != null && castlingTarget.transform.childCount == 0)
                        {
                            allowClick = false;
                            Move(curObject, castlingTarget);
                            return; // ❗️防止落入默认处理
                        }
                    }

                    curGrid = hitTransform.gameObject;
                    lastTargetParent = hitTransform;

                    if (hitTransform.childCount == 0)
                    {
                        if (IsValidMove(curObject, hitTransform.gameObject))
                        {
                            allowClick = false;
                            Move(curObject, hitTransform.gameObject);
                        }
                        else
                        {
                            allowClick = true;
                            curGrid = null;
                        }
                    }
                    else if (hitTransform.GetChild(0).tag != curObject.tag)
                    {
                        if (IsValidMove(curObject, hitTransform.gameObject))
                        {
                            allowClick = false;
                            inBattlePiece = curObject;
                            retrieve = inBattlePiece.transform.parent.gameObject;
                            Move(curObject, hitTransform.gameObject);
                        }
                        else
                        {
                            allowClick = true;
                            curGrid = null;
                        }
                    }
                }
            }*/

            if (Physics.Raycast(ray, out hit))
            {
                Transform tile = ResolveTile(hit.transform);
                if (!tile) return;

                if (Input.GetMouseButtonDown(0) && allowClick && curObject != null)
                {
                    curGrid = tile.gameObject;
                    lastTargetParent = tile;

                    Transform occ = GetOccupant(tile);
                    if (occ == null)
                    {
                        if (IsValidMove(curObject, tile.gameObject))
                        {
                            allowClick = false;
                            Move(curObject, tile.gameObject);
                        }
                    }
                    else if (!occ.CompareTag(curObject.tag))
                    {
                        if (IsValidMove(curObject, tile.gameObject))
                        {
                            allowClick = false;
                            inBattlePiece = curObject;
                            retrieve = inBattlePiece.transform.parent.gameObject;
                            Move(curObject, tile.gameObject);
                        }
                    }
                }
            }
        }
    }


    // Find a child anywhere under 'root' by name
    private static Transform FindDeepChildByName(Transform root, string name)
    {
        if (root == null) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }


    public bool IsValidMove(GameObject chessObject, GameObject targetSquare)
    {
        if (chessObject == null || targetSquare == null)
            return false;

        //Vector2Int start = GetBoardCoordinates(chessObject.transform.position);
        //Vector2Int end = GetBoardCoordinates(targetSquare.transform.position);

        // 调试输出（可选）
        //Debug.Log($"移动验证：{chessObject.name} ({start}) -> {targetSquare.name} ({end})");

        // 王车易位特殊处理
        /*if (chessObject.GetComponent<KingMovement>() != null)
        {
            bool isWhite = chessObject.CompareTag("White");
            
            // 短易位目标：G1/G8
            if (end == new Vector2Int(6, isWhite ? 0 : 7))
                return CanCastle(chessObject, true);
            
            // 长易位目标：C1/C8
            if (end == new Vector2Int(2, isWhite ? 0 : 7))
                return CanCastle(chessObject, false);

            // 普通王移动
            return chessObject.GetComponent<KingMovement>().IsValidMove(targetSquare);
        }

        // 王车易位特殊处理
        if (chessObject.GetComponent<KingMovement>() != null)
        {
            bool isWhite = chessObject.CompareTag("White");
            Vector2Int start = GetBoardCoordinates(chessObject.transform.position);
            Vector2Int end = GetBoardCoordinates(targetSquare.transform.position);

            LogCastle($"King {(isWhite ? "White" : "Black")} trying {start} -> {end}");

            // Kingside target: G1 / G8  (file x = 6)
            if (end == new Vector2Int(6, isWhite ? 0 : 7))
            {
                LogCastle($"Attempting KINGSIDE castle {(isWhite ? "White" : "Black")} (E -> G)");
                bool ok = CanCastle(chessObject, true);
                LogCastle($"CanCastle(kingside) => {ok}");
                return ok;
            }

            // Queenside target: C1 / C8 (file x = 2)
            if (end == new Vector2Int(2, isWhite ? 0 : 7))
            {
                LogCastle($"Attempting QUEENSIDE castle {(isWhite ? "White" : "Black")} (E -> C)");
                bool ok = CanCastle(chessObject, false);
                LogCastle($"CanCastle(queenside) => {ok}");
                return ok;
            }

            // 普通王移动
            return chessObject.GetComponent<KingMovement>().IsValidMove(targetSquare);
        }*/

        if (chessObject == null || targetSquare == null) return false;

        // --- KING special: use tile names, not coordinates ---
        if (chessObject.GetComponent<KingMovement>() != null)
        {
            bool isWhite = chessObject.CompareTag("White");
            string tName = targetSquare.name;

            // Kingside / Queenside castling destinations
            bool isKS = tName == (isWhite ? "G1" : "G8");
            bool isQS = tName == (isWhite ? "C1" : "C8");
            if (isKS || isQS)
                return CanCastle(chessObject, isKS);

            // otherwise normal king step
            return chessObject.GetComponent<KingMovement>().IsValidMove(targetSquare);
        }

        // ----- PAWN special: EN PASSANT first -----
        if (chessObject.GetComponent<PawnMovement>() != null)
        {
            // (a) en passant diagonal onto empty square?
            if (IsEnPassantAttempt(chessObject, targetSquare)) return true;

            // (b) otherwise normal pawn rules
            if (chessObject.GetComponent<PawnMovement>().IsValidMove(targetSquare)) return true;

            return false;
        }


        // 其他棋子类型验证
        //if (chessObject.GetComponentInChildren<PawnMovement>() is PawnMovement pawn && pawn.IsValidMove(targetSquare)) 
        //return true;
        // --- Pawn (include en passant) ---
        // ----- PAWN (en passant first, then normal) -----
        if (chessObject.GetComponent<PawnMovement>() != null)
        {
            // Use the new EP system
            if (IsEnPassantAttempt(chessObject, targetSquare))
                return true;

            // Otherwise normal pawn rules
            return chessObject.GetComponent<PawnMovement>().IsValidMove(targetSquare);
        }

        if (chessObject.GetComponent<KnightMovement>() is KnightMovement knight && knight.IsValidMove(targetSquare)) 
            return true;
        if (chessObject.GetComponent<RookMovement>() is RookMovement rook && rook.IsValidMove(targetSquare)) 
            return true;
        if (chessObject.GetComponent<BishopMovement>() is BishopMovement bishop && bishop.IsValidMove(targetSquare)) 
            return true;
        if (chessObject.GetComponent<QueenMovement>() is QueenMovement queen && queen.IsValidMove(targetSquare)) 
            return true;

        // 默认返回
        return false;
    }


    public List<GameObject> GetExtraCastlingHighlights(GameObject king)
    {
        List<GameObject> highlights = new List<GameObject>();

        if (!king.name.Contains("King")) return highlights;

        bool isWhite = king.CompareTag("White");
        Vector2Int pos = GetBoardCoordinates(king.transform.position);

        if ((isWhite && !whiteKingHasMoved) || (!isWhite && !blackKingHasMoved))
        {
            // Kingside
            if ((isWhite && !whiteRookKingsideHasMoved) || (!isWhite && !blackRookKingsideHasMoved))
            {
                GameObject rookSquare = GameObject.Find(isWhite ? "H1" : "H8");
                GameObject castlingTarget = GameObject.Find(isWhite ? "G1" : "G8");

                if (rookSquare != null && castlingTarget != null &&
                    rookSquare.transform.childCount > 0 &&
                    castlingTarget.transform.childCount == 0)
                {
                    highlights.Add(rookSquare);
                    highlights.Add(castlingTarget);
                }
            }

            // Queenside
            if ((isWhite && !whiteRookQueensideHasMoved) || (!isWhite && !blackRookQueensideHasMoved))
            {
                GameObject rookSquare = GameObject.Find(isWhite ? "A1" : "A8");
                GameObject castlingTarget = GameObject.Find(isWhite ? "C1" : "C8");

                if (rookSquare != null && castlingTarget != null &&
                    rookSquare.transform.childCount > 0 &&
                    castlingTarget.transform.childCount == 0)
                {
                    highlights.Add(rookSquare);
                    highlights.Add(castlingTarget);
                }
            }
        }

        return highlights;
    }



    /*private bool CanCastle(GameObject kingObj, bool kingside)
    {
        if (kingObj == null) return false;

        bool isWhite = kingObj.CompareTag("White");

        // Has the king or the corresponding rook moved?
        if (isWhite ? whiteKingHasMoved : blackKingHasMoved) return false;

        bool rookMoved = isWhite
            ? (kingside ? whiteRookKingsideHasMoved : whiteRookQueensideHasMoved)
            : (kingside ? blackRookKingsideHasMoved : blackRookQueensideHasMoved);
        if (rookMoved) return false;

        string rank = isWhite ? "1" : "8";
        string rookTileName = (kingside ? "H" : "A") + rank;
        string kingStartName = "E" + rank;
        string[] betweenNames = kingside
            ? new[] { "F" + rank, "G" + rank }
            : new[] { "D" + rank, "C" + rank, "B" + rank };
        string[] passNames = kingside
            ? new[] { "E" + rank, "F" + rank, "G" + rank }
            : new[] { "E" + rank, "D" + rank, "C" + rank };

        // Rook is present and same color?
        var rookTile = GetMainTile(rookTileName);
        if (rookTile == null || rookTile.childCount == 0) return false;
        var rook = rookTile.GetChild(0).gameObject;
        if (!rook.CompareTag(isWhite ? "White" : "Black") || !rook.name.Contains("Rook")) return false;

        // Squares between king and rook must be empty
        foreach (var n in betweenNames)
        {
            var t = GetMainTile(n);
            if (t == null || t.childCount > 0) return false;
        }

        // The king may not be in check; the squares it passes through may not be attacked
        string opponent = isWhite ? "Black" : "White";
        foreach (var n in passNames)
        {
            var s = GetMainTile(n);
            if (s == null) return false;
            if (IsSquareAttacked(opponent, s.gameObject)) return false;
        }

        return true;
    }*/

    private bool CanCastle(GameObject kingObj, bool kingside)
    {
        if (kingObj == null) return false;

        bool isWhite = kingObj.CompareTag("White");
        string side = isWhite ? "White" : "Black";
        string wing = kingside ? "Kingside" : "Queenside";
        LogCastle($"=== CanCastle start: {side} {wing} ===");

        // King / rook moved flags
        bool kingMoved = isWhite ? whiteKingHasMoved : blackKingHasMoved;
        bool rookMoved = isWhite
            ? (kingside ? whiteRookKingsideHasMoved : whiteRookQueensideHasMoved)
            : (kingside ? blackRookKingsideHasMoved : blackRookQueensideHasMoved);

        LogCastle($"Flags: kingMoved={kingMoved}, rookMoved={rookMoved}");
        if (kingMoved || rookMoved) { LogCastle("Fail: king or rook already moved."); return false; }

        string rank = isWhite ? "1" : "8";
        string rookTileName = (kingside ? "H" : "A") + rank;
        string[] between = kingside ? new[] { "F" + rank, "G" + rank } : new[] { "D" + rank, "C" + rank, "B" + rank };
        string[] passSquares = kingside ? new[] { "E" + rank, "F" + rank, "G" + rank } : new[] { "E" + rank, "D" + rank, "C" + rank };

        // Rook presence
        var rookTile = GetMainTile(rookTileName);
        if (rookTile == null) { LogCastleErr($"Fail: rook tile {rookTileName} not found (main board)."); return false; }
        if (rookTile.childCount == 0) { LogCastle($"Fail: no rook on {rookTileName}."); return false; }

        var rook = rookTile.GetChild(0).gameObject;
        if (!rook.CompareTag(side) || !rook.name.Contains("Rook"))
        {
            LogCastle($"Fail: object on {rookTileName} is not same-color rook ({rook.name}, tag={rook.tag}).");
            return false;
        }
        LogCastle($"Rook OK on {rookTileName}: {rook.name}");

        // Empty between squares
        foreach (var n in between)
        {
            var t = GetMainTile(n);
            if (t == null) { LogCastleErr($"Fail: between square {n} not found under main board."); return false; }
            if (t.childCount > 0) { LogCastle($"Fail: path blocked at {n} by {t.GetChild(0).name}."); return false; }
            LogCastle($"Between {n}: empty");
        }

        // Squares not attacked
        string opponent = isWhite ? "Black" : "White";
        foreach (var n in passSquares)
        {
            var s = GetMainTile(n);
            if (s == null) { LogCastleErr($"Fail: pass square {n} missing."); return false; }

            bool attacked = IsSquareAttacked(opponent, s.gameObject);
            LogCastle($"Pass {n}: attackedBy{opponent}={attacked}");
            if (attacked) { LogCastle($"Fail: {n} is attacked."); return false; }
        }

        LogCastle($"Success: {side} {wing} available.");
        return true;
    }



    private void OnEnable()
    {
        HoverChangeColor.OnObjectClicked += HandleObjectClicked;
        //HoverChangeColor.OnObjectUnClicked += HandleObjectUnClicked;
        TimeoutPenalty.OnTimeOut += HandleTimeOut;
    }

    private void OnDisable()
    {
        HoverChangeColor.OnObjectClicked -= HandleObjectClicked;
        //HoverChangeColor.OnObjectUnClicked -= HandleObjectUnClicked;
        TimeoutPenalty.OnTimeOut -= HandleTimeOut;
    }

    private void HandleObjectClicked(GameObject clickedObject)
    {
        curObject = clickedObject;
        castlingExtras.Clear(); // 🔄 每次点击前清空

        // 如果是国王，准备 castling 高亮
        if (curObject.name.Contains("King"))
        {
            bool isWhite = curObject.CompareTag("White");

            string rookKingside = isWhite ? "Rook1" : "Rook01";
            string rookQueenside = isWhite ? "Rook" : "Rook0";

            GameObject r1 = GameObject.Find(rookKingside);
            GameObject r2 = GameObject.Find(rookQueenside);

            if (r1 && r1.transform.parent != null)
                castlingExtras.Add(r1.transform.parent.gameObject);
            if (r2 && r2.transform.parent != null)
                castlingExtras.Add(r2.transform.parent.gameObject);
        }
    }

    private void HandleTimeOut()
    {
        if(curObject != null) {
            curObject.GetComponent<HoverChangeColor>().unClick();
        }
        curObject = null;
        //ResetGridHighlight();
        curGrid = null;
    }

    private void HandleObjectUnClicked(GameObject clickedObject)
    {
        curObject = null;
    }

    void Move(GameObject chessObject, GameObject targetSquareObject)
    {
        if (chessObject == null || targetSquareObject == null) return;

        // ensure target is the tile, not the mesh
        var tile = ResolveTile(targetSquareObject.transform);
        if (!tile) return;
        targetSquareObject = tile.gameObject;


        // --- SPECIAL: castling -> move king+rook together ---
        if (chessObject.name.Contains("King"))
    {
        bool isWhite  = chessObject.CompareTag("White");
        string tName  = targetSquareObject.name;
        bool kingside = tName == (isWhite ? "G1" : "G8");
        bool queenside= tName == (isWhite ? "C1" : "C8");

        if (kingside || queenside)
        {
            // Validate first
            if (!CanCastle(chessObject, kingside)) return;

            Transform kingTarget = GetMainTile(tName);
            if (kingTarget == null) return;

            // Kick off simultaneous animation
            StartCastleMove(chessObject, kingTarget, isWhite, kingside);
            return; // IMPORTANT: don't run the single-piece move path
        }
    }

        // Remember last move info
        lastMovePiece = chessObject;
        lastFromTile = chessObject.transform.parent;
        lastToTile = targetSquareObject.transform;


        // reset per-move EP flags
        //enPassantThisMove = false;
        //enPassantVictimThisMove = null;

        curGrid = targetSquareObject;

        Vector3 targetPos = new Vector3(
            targetSquareObject.transform.position.x,
            chessObject.transform.position.y,
            targetSquareObject.transform.position.z
        );

        StartCoroutine(MoveToTarget(
            chessObject,
            targetSquareObject.transform,
            targetPos,
            moveSpeed,
            OnMoveCompleted
        ));
    }

    private IEnumerator MoveBothForCastlingCoroutine(
    GameObject king, Transform kingTarget,
    GameObject rook, Transform rookTarget,
    bool isWhite, bool kingside)
    {
        allowClick = false;
        alreadyCastledThisMove = true;

        // Record move info for bookkeeping (used by your existing flow)
        lastMovePiece = king;
        lastFromTile = king.transform.parent;
        lastToTile = kingTarget;

        // Move BOTH pieces together
        while (Vector3.Distance(king.transform.position, kingTarget.position) > 0.01f ||
               Vector3.Distance(rook.transform.position, rookTarget.position) > 0.01f)
        {
            king.transform.position = Vector3.MoveTowards(
                king.transform.position, kingTarget.position, moveSpeed * Time.deltaTime);
            rook.transform.position = Vector3.MoveTowards(
                rook.transform.position, rookTarget.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap & parent to target tiles
        king.transform.SetParent(kingTarget);
        rook.transform.SetParent(rookTarget);

        // Update flags
        if (isWhite)
        {
            whiteKingHasMoved = true;
            if (kingside) whiteRookKingsideHasMoved = true; else whiteRookQueensideHasMoved = true;
        }
        else
        {
            blackKingHasMoved = true;
            if (kingside) blackRookKingsideHasMoved = true; else blackRookQueensideHasMoved = true;
        }

        // Use your existing end-of-move pipeline
        curObject = king;
        lastTargetParent = kingTarget;

        // ✅ Re-enable input BEFORE calling OnMoveCompleted (normal path sets this in MoveToTarget)
        allowClick = true;

        OnMoveCompleted();  // ← Will run, but we’ll guard inside to not move rook again.

        alreadyCastledThisMove = false; // reset for future moves
    }

    private void StartCastleMove(GameObject king, Transform kingTarget, bool isWhite, bool kingside)
    {
        string rank = isWhite ? "1" : "8";
        string rookStart = (kingside ? "H" : "A") + rank;
        string rookTarget = (kingside ? "F" : "D") + rank;

        Transform rookTile = GetMainTile(rookStart);
        Transform rookDest = GetMainTile(rookTarget);
        if (rookTile == null || rookDest == null || rookTile.childCount == 0) return;

        GameObject rookObj = rookTile.GetChild(0).gameObject;
        StartCoroutine(MoveBothForCastlingCoroutine(king, kingTarget, rookObj, rookDest, isWhite, kingside));
    }



    public IEnumerator MoveToTarget(GameObject chessObject, Transform targetParent, Vector3 targetPosition, float moveSpeed, Action onMoveComplete)
    {
        while (Vector3.Distance(chessObject.transform.position, targetPosition) > 0.01f)
        {
            chessObject.transform.position = Vector3.MoveTowards(chessObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        chessObject.transform.SetParent(targetParent);

        allowClick = true;

        onMoveComplete?.Invoke();
    }

    

    private void OnMoveCompleted()
    {


        player.PlayOneShot(seM);

        // ---------- (A) If this move was en passant, remove the victim pawn now ----------
        /*if (enPassantThisMove && enPassantVictimThisMove != null)
        {
            Destroy(enPassantVictimThisMove);
            enPassantThisMove = false;
            enPassantVictimThisMove = null;
            // EP opportunity is consumed once taken
            enPassant = default;
        }*/

        // ---------- Robust En Passant removal ----------
        /*if (lastMovePiece != null &&
            lastToTile != null &&
            lastMovePiece.GetComponent<PawnMovement>() != null &&
            enPassant.targetTile != null &&
            enPassant.victimPawn != null &&
            enPassant.expirePly == plyCount &&                // EP is valid only this reply
            lastToTile == enPassant.targetTile &&             // pawn landed on EP target
            lastToTile.childCount == 1)                       // landing square is empty except the mover
        {
            // Remove the pawn that was passed
            Destroy(enPassant.victimPawn);
            // Clear EP window after use
            enPassant = default;
        }*/

        // --- En Passant: run mini-game instead of auto-deleting the pawn ---
        if (enPassantThisMove && enPassantVictimThisMove != null)
        {
            // set up the same state your normal capture path uses
            battle = true;

            inBattlePiece = curObject;                       // the moving pawn (attacker)
            retrieve = inBattlePiece.transform.parent   // where to return if attacker loses
                            ? inBattlePiece.transform.parent.gameObject : null;

            capturedPiece = enPassantVictimThisMove;         // the pawn that was passed (defender)
            b1 = capturedPiece;
            b2 = inBattlePiece;

            // camera & UI (same as your normal capture)
            Transform cam = Camera.main.transform;
            SaveCameraTransform(cam);
            StartCoroutine(SmoothTransition(
                OnCameraTransitionCompleted,
                curObject.transform.position + new Vector3(0, 0.1f, 2),
                Quaternion.Euler(-30, 180, 0),
                100
            ));

            // IMPORTANT: do NOT destroy anything here; your OnClick/OnClick0 handlers already do it
            // also don't return; keep your flow consistent with the normal capture path
        }



        /*if (lastTargetParent != null && lastTargetParent.childCount == 2)
        {
            battle = true;
            b1 = lastTargetParent.GetChild(0).gameObject;
            b2 = lastTargetParent.GetChild(1).gameObject;
            Transform child = lastTargetParent.GetChild(0);
            capturedPiece = child.gameObject;
            Transform cameraTransform = Camera.main.transform;
            SaveCameraTransform(cameraTransform);
            StartCoroutine(SmoothTransition(
                OnCameraTransitionCompleted,
                curObject.transform.position + new Vector3(0, 0.1f, 2),
                Quaternion.Euler(-30, 180, 0),
                100
            ));
        }*/

        if (lastTargetParent != null)
        {
            GameObject pA, pB;
            int pieceCount = GetPieceChildren(lastTargetParent, out pA, out pB);
            if (pieceCount >= 2)
            {
                battle = true;

                // attacker is curObject, defender is the other color
                GameObject defender = (pA != null && pA.tag != curObject.tag) ? pA : pB;
                b1 = defender;             // enemy (your UI expected this as "capturedPiece")
                b2 = curObject;            // attacker
                capturedPiece = defender;

                Transform cameraTransform = Camera.main.transform;
                SaveCameraTransform(cameraTransform);
                StartCoroutine(SmoothTransition(
                    OnCameraTransitionCompleted,
                    curObject.transform.position + new Vector3(0, 0.1f, 2),
                    Quaternion.Euler(-30, 180, 0),
                    100
                ));
            }
        }


        // ---------- (B) If a pawn just double-stepped, set a new EP window ----------
        bool createdNewEP = false;
        if (lastMovePiece != null && lastFromTile != null && lastToTile != null
            && lastMovePiece.GetComponent<PawnMovement>() != null
            && TryParseTileName(lastFromTile.name, out var fromC)
            && TryParseTileName(lastToTile.name, out var toC))
        {
            int dy = toC.y - fromC.y;
            bool isWhite = lastMovePiece.CompareTag("White");
            bool doubleStep = (isWhite && dy == +2) || (!isWhite && dy == -2);

            if (doubleStep)
            {
                // middle square the pawn passed over
                var mid = new Vector2Int(fromC.x, fromC.y + (isWhite ? +1 : -1));
                var midTile = TileAtCoord(mid);

                enPassant = new EnPassantInfo
                {
                    targetTile = midTile,
                    victimPawn = lastMovePiece,
                    expirePly = plyCount + 1  // opponent's immediate reply only
                };
                createdNewEP = true;
                // Debug.Log($"[EP] Created: target={midTile?.name}, victim={lastMovePiece.name}, expires at ply {enPassant.expirePly}");
            }
        }

        // ---------- (C) If there was an EP right and the opponent didn't take it, clear it now ----------
        if (!createdNewEP && enPassant.targetTile != null && enPassant.expirePly <= plyCount)
        {
            enPassant = default;
            // Debug.Log("[EP] Cleared (expired).");
        }

        // ✅ Castling logic
        if (curObject != null && curObject.name.Contains("King") && !alreadyCastledThisMove)
        {
            /*bool isWhite = curObject.CompareTag("White");
            Vector2Int kingCoord = GetBoardCoordinates(curObject.transform.position);
            int targetY = isWhite ? 0 : 7;

            // Kingside castling (king on file G)
            if (kingCoord.x == 6)
            {
                MoveRookForCastle(isWhite, true);
            }
            // Queenside castling (king on file C)
            else if (kingCoord.x == 2)
            {
                MoveRookForCastle(isWhite, false);
            }

            // Mark king as moved
            if (isWhite) whiteKingHasMoved = true;
            else blackKingHasMoved = true;*/

            bool isWhite = curObject.CompareTag("White");
            Vector2Int kingCoord = GetBoardCoordinates(curObject.transform.position);
            LogCastle($"King ended at board x={kingCoord.x}, y={kingCoord.y}");

            if (kingCoord.x == 6) // G-file
            {
                LogCastle("Trigger rook move: Kingside");
                MoveRookForCastle(isWhite, true);
            }
            else if (kingCoord.x == 2) // C-file
            {
                LogCastle("Trigger rook move: Queenside");
                MoveRookForCastle(isWhite, false);
            }

            if (isWhite) whiteKingHasMoved = true;
            else blackKingHasMoved = true;

            // 标记王已移动
            if (isWhite) whiteKingHasMoved = true;
            else blackKingHasMoved = true;

            string losingSide = isWhiteTurn ? "White" : "Black";
            if (IsCheckmate(losingSide))
            {
                ShowCheckmateUI(losingSide);
                return; // stop further turn switching
            }

            string nextSide = isWhiteTurn ? "Black" : "White";
            KingMovement nextKing = FindObjectsOfType<KingMovement>().FirstOrDefault(k => k.CompareTag(nextSide));

            if (nextKing != null && nextKing.IsInCheck())
            {
                ShowInCheckUI(nextSide);
            }

        }

        // --- PROMOTION EARLY EXIT ---
        // If a pawn ended on last rank, open promotion UI now and DON'T advance the turn yet.
        if (curObject != null && curObject.name.Contains("Pawn") && ReachedPromotionRank(curObject))
        {
            StartCoroutine(HandlePromotionAfterBattle(curObject)); // waits if a capture mini-game is running
            return; // <- stop here; we'll advance the turn after promotion completes
        }


        GameManager.NextState();
        plyCount++; // <— increment *after* a move completes


        if (curObject != null)
        {
            curObject.GetComponent<HoverChangeColor>()?.unClick();
        }

        AfterMoveCheck(curObject);

        curObject = null;

        Debug.Log($"[TURN] Next side ready. allowClick={allowClick}, curObject={(curObject ? curObject.name : "null")}");
    }

    private void ShowCheckmateUI(string losingSide)
    {
        if (checkmatePanel != null && checkmateText != null)
        {
            checkmateText.text = $"Checkmate! {losingSide} loses!";
            checkmatePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Checkmate UI references are missing!");
        }
    }


    /// <summary>
    /// Returns true only if the side whose king has 'kingTag' is in checkmate.
    /// </summary>
    private bool IsCheckmate(string kingTag)
    {
        Debug.Log($"[generalmoving] Testing checkmate for {kingTag}…");

        // 1) is in check?
        KingMovement king = FindObjectsOfType<KingMovement>()
            .FirstOrDefault(k => k.CompareTag(kingTag));
        if (king == null)
        {
            Debug.LogError("[generalmoving] No king found for tag " + kingTag);
            return false;
        }

        if (!king.IsInCheck())
        {
            Debug.Log($"[generalmoving] {kingTag} king is NOT in check → not checkmate.");
            return false;
        }
        Debug.Log($"[generalmoving] {kingTag} king IS in check.");

        // 2) can king move out?
        if (king.HasLegalMoves())
        {
            Debug.Log($"[generalmoving] {kingTag} king has at least one legal escape move → not checkmate.");
            return false;
        }
        Debug.Log($"[generalmoving] {kingTag} king has NO legal escape moves.");

        // 3) can any ally block or capture?
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = gridBoard[x, y];
                if (square.transform.childCount == 0) continue;

                GameObject piece = square.transform.GetChild(0).gameObject;
                if (!piece.CompareTag(kingTag)) continue;

                // Try moving this piece to every square on the board
                for (int tx = 0; tx < 8; tx++)
                {
                    for (int ty = 0; ty < 8; ty++)
                    {
                        GameObject target = gridBoard[tx, ty];
                        /*if (IsValidMove(piece, target))
                        {
                            Debug.Log($"[generalmoving] Ally {piece.name} can move to {target.name} → not checkmate.");
                            return false;
                        }*/

                        if (IsValidMove(piece, target))
                        {
                            Transform originalParent = piece.transform.parent;
                            GameObject capturedPiece = null;

                            // Simulate move
                            if (target.transform.childCount > 0)
                            {
                                capturedPiece = target.transform.GetChild(0).gameObject;
                                capturedPiece.SetActive(false);
                            }

                            piece.transform.SetParent(target.transform);
                            piece.transform.localPosition = Vector3.zero;

                            bool stillInCheck = king.IsInCheck();

                            // Undo move
                            piece.transform.SetParent(originalParent);
                            piece.transform.localPosition = Vector3.zero;
                            if (capturedPiece != null) capturedPiece.SetActive(true);

                            if (!stillInCheck)
                            {
                                Debug.Log($"[generalmoving] Ally {piece.name} can move to {target.name} and stop the check → not checkmate.");
                                return false;
                            }
                        }

                    }
                }
            }
        }


        Debug.Log($"[generalmoving] {kingTag} is truly checkmated!");
        return true;
    }

    private int GetPieceChildren(Transform tile, out GameObject p1, out GameObject p2)
    {
        p1 = p2 = null; int count = 0;
        for (int i = 0; i < tile.childCount; i++)
        {
            var c = tile.GetChild(i);
            if (!IsPiece(c)) continue;
            if (count == 0) p1 = c.gameObject;
            else if (count == 1) p2 = c.gameObject;
            count++;
        }
        return count;
    }


    private GameObject GetPromotionPrefab(bool isWhite, string pieceType)
    {
        switch (pieceType)
        {
            case "Rook":
                return isWhite ? whiteRookPrefab : blackRookPrefab;
            case "Bishop":
                return isWhite ? whiteBishopPrefab : blackBishopPrefab;
            case "Knight":
                return isWhite ? whiteKnightPrefab : blackKnightPrefab;
            default:
                return isWhite ? whiteQueenPrefab : blackQueenPrefab;
        }
    }

    public Vector2Int GetBoardCoordinates(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x + 7)); // adjust based on your leftmost tile
        int y = Mathf.RoundToInt((worldPos.z + 14) / 2); // maps -14 to 0, -12 to 1, ..., 0 to 7

        return new Vector2Int(x, y);
    }


    public void PerformPromotion(GameObject pawnObject, string pieceType)
    {
        Debug.Log($"Promoting pawn to {pieceType}!");

        Transform parentSquare = pawnObject.transform.parent;
        bool isWhite = pawnObject.CompareTag("White");

        Destroy(pawnObject);

        GameObject newPiecePrefab = GetPromotionPrefab(isWhite, pieceType);
        if (newPiecePrefab == null)
        {
            Debug.LogError($"No prefab found for {pieceType}");
            return;
        }

        GameObject newPiece = Instantiate(newPiecePrefab, parentSquare.position, Quaternion.identity);
        newPiece.transform.SetParent(parentSquare);

        newPiece.transform.localScale = newPiecePrefab.transform.localScale;
        float yOffset = 0.5f;
        Vector3 finalPos = new Vector3(
            parentSquare.position.x,
            parentSquare.position.y + yOffset,
            parentSquare.position.z
        );
        newPiece.transform.position = finalPos;

        Debug.Log($"Pawn promoted to {pieceType}!");
    }



void HighlightGrid(GameObject grid, Color color)
    {
        Renderer renderer = grid.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Store the original color if it's not already stored
            if (!originalColors.ContainsKey(grid))
            {
                originalColors[grid] = renderer.material.color;         
            }

            // Change the color of the grid
            renderer.material.color = color;
        }
    }

    private void ShowInCheckUI(string side)
    {
        if (inCheckPanel != null && inCheckText != null)
        {
            inCheckText.text = $"⚠ {side} is in check!";
            inCheckPanel.SetActive(true);
            StartCoroutine(HideInCheckAfterSeconds(2.5f));  // Optional auto-hide
        }
    }

    private void AfterMoveCheck(GameObject movedPiece)
    {

        if (movedPiece == null) return;

        /*if (movedPiece.name.Contains("Pawn"))
        {
            Vector2Int boardPos = generalmoving.Instance.GetBoardCoordinatesFromWorld(movedPiece.transform.position);
            Debug.Log("Pawn Y: " + boardPos.y + ", Expected: " + (movedPiece.CompareTag("White") ? -14 : 0));



            if ((movedPiece.CompareTag("White") && boardPos.y == -14) ||
                (movedPiece.CompareTag("Black") && boardPos.y == 0))
            {
                StartCoroutine(HandlePromotionAfterBattle(movedPiece));
                //FindObjectOfType<PromotionManager>().ShowPromotionUI(movedPiece);
                return; // pause flow until promotion completes
            }
        } */

        if (movedPiece != null && movedPiece.name.Contains("Pawn"))
        {
            if (movedPiece.transform.parent != null &&
                TryParseTileName(movedPiece.transform.parent.name, out var coord))
            {
                // White promotes on rank 8 (coord.y == 7); Black on rank 1 (coord.y == 0)
                bool reached = movedPiece.CompareTag("White") ? coord.y == 7 : coord.y == 0;
                if (reached)
                {
                    // If a battle is in progress (capture on last rank), wait for it to finish
                    StartCoroutine(HandlePromotionAfterBattle(movedPiece));
                    return;
                }
            }
        }


        string movingSide = movedPiece.CompareTag("White") ? "White" : "Black";
        string opponentSide = movedPiece.CompareTag("White") ? "Black" : "White";
        // Checkmate check
        if (IsCheckmate(opponentSide))
        {
            ShowCheckmateUI(opponentSide);
            return; // stop here if checkmate
        }

        // Check (not checkmate)
        KingMovement opponentKing = FindObjectsOfType<KingMovement>()
            .FirstOrDefault(k => k.CompareTag(opponentSide));

        if (opponentKing != null && opponentKing.IsInCheck())
        {
            ShowInCheckUI(opponentSide);
        }



    }

    private bool IsTileName(string n)
    {
        if (string.IsNullOrEmpty(n) || n.Length != 2) return false;
        char f = n[0], r = n[1];
        return (f >= 'A' && f <= 'H') && (r >= '1' && r <= '8');
    }

    private Transform ResolveTile(Transform t)
    {
        while (t != null && !IsTileName(t.name)) t = t.parent;
        return t; // null if not a board click
    }

    private bool IsPiece(Transform t)
        => t != null && (t.CompareTag("White") || t.CompareTag("Black"));

    private Transform GetOccupant(Transform tile)
    {
        for (int i = 0; i < tile.childCount; i++)
            if (IsPiece(tile.GetChild(i))) return tile.GetChild(i);
        return null;
    }


    /*private IEnumerator HandlePromotionAfterBattle(GameObject pawn)
    {
        // Wait until the mini-game is over
        while (battle)
        {
            yield return null;
        }

        // Then show promotion
        FindObjectOfType<PromotionManager>().ShowPromotionUI(pawn);
    }*/

    private IEnumerator HandlePromotionAfterBattle(GameObject pawn)
    {
        // Wait for capture mini-game to finish (if any)
        while (battle) yield return null;

        // Pawn might have died during the battle
        if (pawn == null) yield break;
        if (!pawn || pawn.transform == null) yield break;

        // Still a pawn and still on last rank?
        if (!pawn.name.Contains("Pawn")) yield break;
        if (!ReachedPromotionRank(pawn)) yield break;

        // Show the UI
        var pm = FindObjectOfType<PromotionManager>();
        if (pm != null) pm.ShowPromotionUI(pawn);
    }

    public void OnPromotionFinished(GameObject newPiece)
    {
        // Optional: run your existing post-move checks on the new piece
        AfterMoveCheck(newPiece);

        // Now advance the turn and bump ply
        GameManager.NextState();
        plyCount++;
    }



    private IEnumerator HideInCheckAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (inCheckPanel != null)
            inCheckPanel.SetActive(false);
    }


    void ResetGridHighlight()
    {
        /*if (curGrid != null)
        {
            Renderer renderer = curGrid.GetComponent<Renderer>();
            if (renderer != null && originalColors.ContainsKey(curGrid))
            {
                if (renderer.material.color != Color.blue){
                    // Reset to the original color
                    renderer.material.color = originalColors[curGrid];
                    Debug.Log(0);
                } else {
                    renderer.material.color = Color.cyan;
                    Debug.Log(1);
                }                
            }

            curGrid = null; // Clear the current grid
        }*/
    }

public void SaveCameraTransform(Transform cameraTransform)
    {
        save = true;
        savedPosition = cameraTransform.position;
        savedRotation = cameraTransform.rotation;
    }

    public void RestoreCameraTransform()
    {
        save = false;
    }

    private IEnumerator SmoothTransition(Action callback, Vector3 targetPosition, Quaternion targetRotation, float fov)
    {
        float transitionTime = 1.5f;
        // Store the initial values of the camera
        Vector3 initialPosition = Camera.main.transform.position;
        Quaternion initialRotation = Camera.main.transform.rotation;
        float initialFOV = Camera.main.fieldOfView;

        // Timer to keep track of the transition progress
        float timeElapsed = 0f;

        // Smoothly transition over time
        while (timeElapsed < transitionTime)
        {
            timeElapsed += Time.deltaTime;

            // Interpolate the position, rotation, and FOV of the camera
            Camera.main.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeElapsed / transitionTime);
            Camera.main.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, timeElapsed / transitionTime);
            Camera.main.fieldOfView = Mathf.Lerp(initialFOV, fov, timeElapsed / transitionTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the final values are set (in case transition time is over)
        Camera.main.transform.position = targetPosition;
        Camera.main.transform.rotation = targetRotation;

        // Invoke the callback function after the camera transition is complete
        callback?.Invoke();
    }

    private void OnCameraTransitionCompleted()
    {
        uiPanel.SetActive(true);       
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ShowObjectImage(b2, b1);
        }
    }

    private void OnCameraTransitionCompleted0()
    {
        
    }

    public void OnClick(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        Destroy(capturedPiece);
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    public void OnClick0(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        //Debug.Log(curObject.name);
        Destroy(inBattlePiece);
        inBattlePiece = null;
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    public void OnClick1(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        Move(inBattlePiece, retrieve);
        GameManager.NextState();
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    public GameObject GetSquareAt(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return null;
        return gridBoard[x, y];
    }

    public Vector2Int GetBoardCoordinatesFromWorld(Vector3 worldPosition)
{
    int x = Mathf.RoundToInt(worldPosition.x);
    int y = Mathf.RoundToInt(worldPosition.z);
    return new Vector2Int(x, y);
}


    // Build dictionary and gridBoard[] from MAIN board root only
    private void BuildMainTileIndex()
    {
        mainTiles.Clear();

        foreach (Transform t in mainBoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (IsTileName(t.name))
                mainTiles[t.name] = t;
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                string name = $"{(char)('A' + x)}{y + 1}";
                if (mainTiles.TryGetValue(name, out var tr))
                    gridBoard[x, y] = tr.gameObject;
                else
                    Debug.LogError($"generalmoving: Could not find tile '{name}' under mainBoardRoot '{mainBoardRoot.name}'.");
            }
        }
    }

    public Transform GetMainTile(string tileName)
    {
        if (mainTiles.TryGetValue(tileName, out var t)) return t;

        // Fallback (one-time deep lookup under MAIN board)
        Transform found = FindDeepChildByName(mainBoardRoot, tileName);
        if (found != null) { mainTiles[tileName] = found; return found; }

        Debug.LogError($"[generalmoving] Tile '{tileName}' not found under mainBoardRoot '{mainBoardRoot?.name ?? "NULL"}'.");
        return null;
    }

    private bool HasRookOnTile(string tileName, string tag)
    {
        var tr = GetMainTile(tileName);
        if (tr == null || tr.childCount == 0) return false;
        var p = tr.GetChild(0);
        return p.CompareTag(tag) && p.name.Contains("Rook");
    }

    // Generic “is this square attacked by side X?”
    public bool IsSquareAttacked(string attackerTag, GameObject targetSquare)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject sq = GetSquareAt(x, y);
                if (sq == null || sq.transform.childCount == 0) continue;

                GameObject piece = sq.transform.GetChild(0).gameObject;
                if (!piece.CompareTag(attackerTag)) continue;
                if (piece.name.Contains("King")) continue; // ignore king to avoid recursion

                // Reuse piece validators:
                if (piece.TryGetComponent<PawnMovement>(out var pawn) && pawn.IsValidMove(targetSquare)) return true;
                if (piece.TryGetComponent<KnightMovement>(out var kn) && kn.IsValidMove(targetSquare)) return true;
                if (piece.TryGetComponent<BishopMovement>(out var bi) && bi.IsValidMove(targetSquare)) return true;
                if (piece.TryGetComponent<RookMovement>(out var ro) && ro.IsValidMove(targetSquare)) return true;
                if (piece.TryGetComponent<QueenMovement>(out var qu) && qu.IsValidMove(targetSquare)) return true;
            }
        }
        return false;
    }

    private void MoveRookForCastle(bool isWhite, bool kingside)
    {
        string rank = isWhite ? "1" : "8";
        string rookTileName = (kingside ? "H" : "A") + rank;
        string rookTargetName = (kingside ? "F" : "D") + rank;

        var rookTile = GetMainTile(rookTileName);
        var rookTarget = GetMainTile(rookTargetName);
        if (rookTile == null || rookTarget == null) return;
        if (rookTile.childCount == 0) return;

        var rook = rookTile.GetChild(0).gameObject;

        isCastlingRookMoving = true;
        StartCoroutine(MoveToTarget(
            rook,
            rookTarget,
            rookTarget.position,
            moveSpeed,
            () =>
            {
                rook.transform.SetParent(rookTarget);
                isCastlingRookMoving = false;

                if (isWhite)
                {
                    if (kingside) whiteRookKingsideHasMoved = true;
                    else whiteRookQueensideHasMoved = true;
                }
                else
                {
                    if (kingside) blackRookKingsideHasMoved = true;
                    else blackRookQueensideHasMoved = true;
                }
            }
        ));
    }

    [SerializeField] private bool debugCastling = true;

    private void LogCastle(string msg)
    {
        if (debugCastling) Debug.Log($"[CASTLE] {msg}");
    }
    private void LogCastleErr(string msg)
    {
        if (debugCastling) Debug.LogError($"[CASTLE] {msg}");
    }

    [ContextMenu("Debug/Print Castling Flags")]
    private void DebugCastlingState()
    {
        LogCastle($"White: KingMoved={whiteKingHasMoved}, RkMoved={whiteRookKingsideHasMoved}, RqMoved={whiteRookQueensideHasMoved}");
        LogCastle($"Black: KingMoved={blackKingHasMoved}, RkMoved={blackRookKingsideHasMoved}, RqMoved={blackRookQueensideHasMoved}");
    }

    [ContextMenu("Debug/Show Key Squares Under Attack")]
    private void DebugKeySquares()
    {
        string[] squares = { "E1", "F1", "G1", "C1", "D1", "E8", "F8", "G8", "C8", "D8" };
        foreach (var s in squares)
        {
            var t = GetMainTile(s);
            if (t == null) { LogCastleErr($"{s} missing"); continue; }
            bool byBlack = IsSquareAttacked("Black", t.gameObject);
            bool byWhite = IsSquareAttacked("White", t.gameObject);
            LogCastle($"{s}: attackedByWhite={byWhite}, attackedByBlack={byBlack}, occupied={(t.childCount > 0 ? t.GetChild(0).name : "empty")}");
        }
    }

    // true if 'pawn' is attempting a legal en passant capture onto 'targetSquare'
    private bool IsEnPassantAttempt(GameObject pawn, GameObject targetSquare)
    {
        if (enPassant.targetTile == null) return false;                  // no EP available
        if (enPassant.expirePly != plyCount) return false;               // only this ply
        if (targetSquare.transform != enPassant.targetTile) return false;// must move to EP target
        if (targetSquare.transform.childCount != 0) return false;        // EP target is empty by definition

        // Check diagonal one-step in the correct forward direction by reading tile names.
        if (!TryParseTileName(pawn.transform.parent.name, out var from)) return false;
        if (!TryParseTileName(targetSquare.name, out var to)) return false;

        int dx = Mathf.Abs(to.x - from.x);
        int dy = to.y - from.y;
        bool isWhite = pawn.CompareTag("White");

        if (dx != 1) return false;
        if (isWhite && dy != +1) return false;     // white moves "up" the ranks: 2->3->4 ...
        if (!isWhite && dy != -1) return false;    // black moves "down": 7->6->5 ...

        // Looks good → mark pending capture so OnMoveCompleted can remove the victim
        enPassantThisMove = true;
        enPassantVictimThisMove = enPassant.victimPawn;
        return true;
    }

    // Parse "A1".."H8" -> (file 0..7, rank 0..7)
    private static bool TryParseTileName(string tileName, out Vector2Int coord)
    {
        coord = new Vector2Int(-1, -1);
        if (string.IsNullOrEmpty(tileName) || tileName.Length < 2) return false;

        char fileC = char.ToUpperInvariant(tileName[0]);
        if (fileC < 'A' || fileC > 'H') return false;

        // support tiles named like "E1", "E01", or "E1 (Clone)"
        string digits = new string(tileName.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits)) return false;

        if (!int.TryParse(digits, out int rankNum)) return false; // 1..8
        if (rankNum < 1 || rankNum > 8) return false;

        int file = fileC - 'A';      // A->0 … H->7
        int rank = rankNum - 1;      // 1->0 … 8->7
        coord = new Vector2Int(file, rank);
        return true;
    }

    private static bool ReachedPromotionRank(GameObject pawn)
    {
        if (pawn == null || pawn.transform.parent == null) return false;
        if (!TryParseTileName(pawn.transform.parent.name, out var c)) return false;
        return pawn.CompareTag("White") ? (c.y == 7) : (c.y == 0);
    }

    // Safer coordinates: prefer parent tile name; fallback to nearest grid cell.
    public Vector2Int GetBoardCoordinatesFromPiece(Transform piece)
    {
        if (piece != null && piece.parent != null && TryParseTileName(piece.parent.name, out var c))
            return c;

        // Fallback: nearest of main gridBoard (in case parent is wrong temporarily)
        float best = float.PositiveInfinity;
        Vector2Int bestC = new Vector2Int(-1, -1);
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                var sq = gridBoard[x, y];
                if (sq == null) continue;
                float d = (sq.transform.position - piece.position).sqrMagnitude;
                if (d < best) { best = d; bestC = new Vector2Int(x, y); }
            }
        return bestC;
    }


    private static string TileNameFromCoord(Vector2Int c)
    => $"{(char)('A' + c.x)}{c.y + 1}";

    private Transform TileAtCoord(Vector2Int c)
    {
        if (c.x < 0 || c.x > 7 || c.y < 0 || c.y > 7) return null;
        var go = gridBoard[c.x, c.y];
        return go ? go.transform : null;
    }

}

// --------------------
// CHECKMATE SYSTEM DOCUMENTATION
// --------------------

/*
 * Checkmate System Overview
 * ------------------------
 * The checkmate system in this script is responsible for:
 *  - Detecting when a king is in check.
 *  - Determining if a checkmate has occurred (i.e., the king is in check and has no legal moves, and no allied piece can block or capture the threat).
 *  - Displaying UI feedback for check and checkmate states.
 *  - Integrating check/checkmate logic into the move flow.
 *
 * Key Functions:
 * --------------
 * 1. IsCheckmate(string kingTag)
 *    - Core function that determines if the king of the given side is in checkmate.
 *    - Steps:
 *        a) Finds the king by tag.
 *        b) Checks if the king is in check (via KingMovement.IsInCheck()).
 *        c) Checks if the king has any legal moves (via KingMovement.HasLegalMoves()).
 *        d) Checks if any allied piece can block the check or capture the attacking piece by simulating all possible moves.
 *    - Returns true if all above fail (i.e., checkmate), false otherwise.
 *
 * 2. AfterMoveCheck(GameObject movedPiece)
 *    - Called after each move.
 *    - Checks if the opponent is in checkmate (calls IsCheckmate).
 *    - If not, checks if the opponent is in check (calls KingMovement.IsInCheck()).
 *    - Triggers UI for checkmate or check as appropriate.
 *
 * 3. ShowCheckmateUI(string losingSide)
 *    - Displays the checkmate panel and message.
 *
 * 4. ShowInCheckUI(string side)
 *    - Displays the "in check" panel and message.
 *
 * 5. KingMovement.IsInCheck()
 *    - (Defined elsewhere) Determines if the king is currently under attack.
 *
 * 6. KingMovement.HasLegalMoves()
 *    - (Defined elsewhere) Determines if the king has any legal moves to escape check.
 *
 * UI Integration:
 * ---------------
 * - When checkmate is detected, ShowCheckmateUI is called to display the result.
 * - When a king is in check (but not checkmate), ShowInCheckUI is called.
 *
 * Move Flow Integration:
 * ----------------------
 * - After each move, AfterMoveCheck is called to evaluate the board state for check or checkmate.
 * - During king moves, IsCheckmate is also checked to handle special cases.
 *
 * Notes:
 * ------
 * - The checkmate logic simulates all possible moves for all allied pieces to ensure no escape is possible.
 * - The system relies on KingMovement for check detection and legal move generation.
 * - The UI panels for check and checkmate must be assigned in the inspector.
 */