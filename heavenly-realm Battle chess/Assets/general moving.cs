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

    private readonly Dictionary<string, Transform> mainTiles = new Dictionary<string, Transform>();


    public static generalmoving Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }


    void Start()
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

    }


    // Update is called once per frame
    void Update()
    {
        if (isCastlingRookMoving) return;

        if (!battle)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
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

        Vector2Int start = GetBoardCoordinates(chessObject.transform.position);
        Vector2Int end = GetBoardCoordinates(targetSquare.transform.position);

        // 调试输出（可选）
        Debug.Log($"移动验证：{chessObject.name} ({start}) -> {targetSquare.name} ({end})");

        // 王车易位特殊处理
        if (chessObject.GetComponent<KingMovement>() != null)
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

        // 其他棋子类型验证
        if (chessObject.GetComponentInChildren<PawnMovement>() is PawnMovement pawn && pawn.IsValidMove(targetSquare)) 
            return true;
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



    private bool CanCastle(GameObject kingObj, bool kingside)
    {
        if (kingObj == null) return false;

        bool isWhite = kingObj.CompareTag("White");
        Vector2Int start = GetBoardCoordinates(kingObj.transform.position);

        // 检查王和车的移动状态
        bool kingMoved = isWhite ? whiteKingHasMoved : blackKingHasMoved;
        bool rookMoved = isWhite 
            ? (kingside ? whiteRookKingsideHasMoved : whiteRookQueensideHasMoved)
            : (kingside ? blackRookKingsideHasMoved : blackRookQueensideHasMoved);

        if (kingMoved || rookMoved)
        {
            Debug.Log($"易位失败：{(isWhite ? "白" : "黑")}方 {(kingside ? "短" : "长")}易位王/车已移动");
            return false;
        }

        // 检查路径
        int step = kingside ? 1 : -1;
        int maxStep = kingside ? 2 : 3; // 短易位检查右侧2格，长易位检查左侧3格

        for (int i = 1; i <= maxStep; i++)
        {
            int checkX = start.x + i * step;
            if (checkX < 0 || checkX >= 8)
            {
                Debug.Log($"易位路径越界: {checkX}");
                return false;
            }

            GameObject grid = gridBoard[checkX, start.y];
            if (grid == null)
            {
                Debug.LogError($"无法找到格子: {checkX},{start.y}");
                return false;
            }

            if (grid.transform.childCount > 0)
            {
                Debug.Log($"易位路径被阻挡在 {grid.name}（阻挡物：{grid.transform.GetChild(0).name}）");
                return false;
            }
        }

        Debug.Log($"{(isWhite ? "白" : "黑")}方 {(kingside ? "短" : "长")}易位路径畅通");
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

        if (lastTargetParent != null && lastTargetParent.childCount == 2)
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
        }

        // ✅ Castling logic
        if (curObject != null && curObject.name.Contains("King"))
        {
            bool isWhite = curObject.CompareTag("White");
            Vector2Int kingCoord = GetBoardCoordinates(curObject.transform.position);
            int targetY = isWhite ? 0 : 7;

            // Kingside castling
            if (kingCoord.x == 6)
            {
                string rookName = isWhite ? "Rook1" : "Rook01";
                GameObject rook = GameObject.Find(rookName);
                GameObject rookTarget = gridBoard[5, targetY]; // F1 / F8

                if (rook != null && rookTarget != null)
                {
                    isCastlingRookMoving = true;
                    StartCoroutine(MoveToTarget(
                        rook,
                        rookTarget.transform,
                        rookTarget.transform.position,
                        moveSpeed,
                        () => {
                            rook.transform.SetParent(rookTarget.transform);
                            isCastlingRookMoving = false;
                            if (isWhite) whiteRookKingsideHasMoved = true;
                            else blackRookKingsideHasMoved = true;
                        }
                    ));
                }
            }

            // Queenside castling
            else if (kingCoord.x == 2)
            {
                string rookName = isWhite ? "Rook" : "Rook0";
                GameObject rook = GameObject.Find(rookName);
                GameObject rookTarget = gridBoard[3, targetY]; // D1 / D8

                if (rook != null && rookTarget != null)
                {
                    isCastlingRookMoving = true;
                    StartCoroutine(MoveToTarget(
                        rook,
                        rookTarget.transform,
                        rookTarget.transform.position,
                        moveSpeed,
                        () => {
                            rook.transform.SetParent(rookTarget.transform);
                            isCastlingRookMoving = false;
                            if (isWhite) whiteRookQueensideHasMoved = true;
                            else blackRookQueensideHasMoved = true;
                        }
                    ));
                }
            }

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

        GameManager.NextState();

        if (curObject != null)
        {
            curObject.GetComponent<HoverChangeColor>()?.unClick();
        }

        AfterMoveCheck(curObject);

        curObject = null;
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

        if (movedPiece.name.Contains("Pawn"))
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

    private IEnumerator HandlePromotionAfterBattle(GameObject pawn)
    {
        // Wait until the mini-game is over
        while (battle)
        {
            yield return null;
        }

        // Then show promotion
        FindObjectOfType<PromotionManager>().ShowPromotionUI(pawn);
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