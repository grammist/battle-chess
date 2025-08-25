using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PromotionManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject promotionPanel;
    public Button queenButton;
    public Button rookButton;
    public Button bishopButton;
    public Button knightButton;

    [Header("Piece Prefabs (Board models only)")]
    public GameObject whiteQueenModel;
    public GameObject whiteRookModel;
    public GameObject whiteBishopModel;
    public GameObject whiteKnightModel;
    public GameObject blackQueenModel;
    public GameObject blackRookModel;
    public GameObject blackBishopModel;
    public GameObject blackKnightModel;

    [Header("Main Board Root (ASSIGN ME)")]
    [SerializeField] private Transform mainBoardRoot;   // <- single source of truth

    [Header("Desired WORLD Scales")]
    public Vector3 queenScale = new Vector3(0.005f, 0.015f, 0.005f);
    public Vector3 rookScale = new Vector3(0.015f, 0.060f, 0.015f);
    public Vector3 bishopScale = new Vector3(0.025f, 0.075f, 0.025f);
    public Vector3 knightScale = new Vector3(0.35f, 1.40f, 0.35f);

    [Header("Placement")]
    [Tooltip("Vertical lift from tile center (in local units)")]
    public float pieceYOffset = 0.5f;

    private GameObject promotingPawn;


    private void Start()
    {
        if (!queenButton || !rookButton || !bishopButton || !knightButton)
        {
            Debug.LogError("PromotionManager: One or more button references are missing.");
            return;
        }
        promotionPanel?.SetActive(false);

        queenButton.onClick.AddListener(() => Promote("Queen"));
        rookButton.onClick.AddListener(() => Promote("Rook"));
        bishopButton.onClick.AddListener(() => Promote("Bishop"));
        knightButton.onClick.AddListener(() => Promote("Knight"));
    }

    public void ShowPromotionUI(GameObject pawn)
    {
        promotingPawn = pawn;
        promotionPanel?.SetActive(true);
    }

    public void Promote(string pieceType)
    {
        if (!promotingPawn) { Debug.LogError("PromotionManager: promotingPawn is null."); return; }
        if (!mainBoardRoot)
        {
            Debug.LogError("PromotionManager: mainBoardRoot is not assigned. Drag your MAIN board root here.");
            return;
        }

        // side/tag
        bool isWhite = promotingPawn.CompareTag("White");
        string pawnTag = promotingPawn.tag;

        // ---------- RESOLVE MAIN-BOARD TILE BY NAME ----------
        string tileName = promotingPawn.transform.parent ? promotingPawn.transform.parent.name : null;
        if (string.IsNullOrEmpty(tileName))
        {
            Debug.LogError("PromotionManager: Pawn has no parent tile name to resolve.");
            return;
        }

        Transform parentTile = FindDeepChildByName(mainBoardRoot, tileName);
        if (!parentTile)
        {
            Debug.LogError($"PromotionManager: Could not find tile '{tileName}' under mainBoardRoot '{mainBoardRoot.name}'.");
            return;
        }

        // ---------- PICK PREFAB + TARGET WORLD SCALE ----------
        GameObject prefab = null;
        Vector3 targetWorldScale = Vector3.one;
        switch (pieceType)
        {
            case "Queen": prefab = isWhite ? whiteQueenModel : blackQueenModel; targetWorldScale = queenScale; break;
            case "Rook": prefab = isWhite ? whiteRookModel : blackRookModel; targetWorldScale = rookScale; break;
            case "Bishop": prefab = isWhite ? whiteBishopModel : blackBishopModel; targetWorldScale = bishopScale; break;
            case "Knight": prefab = isWhite ? whiteKnightModel : blackKnightModel; targetWorldScale = knightScale; break;
            default: Debug.LogError("PromotionManager: Unknown pieceType " + pieceType); return;
        }
        if (!prefab) { Debug.LogError("PromotionManager: Prefab missing for " + pieceType); return; }

        // Warn if the prefab smells like UI
        if (!LooksLikeBoardMesh(prefab) || LooksLikeUIPrefab(prefab))
        {
            Debug.LogWarning($"PromotionManager: The {pieceType} prefab might be a UI asset. HasMesh={LooksLikeBoardMesh(prefab)} HasUI={LooksLikeUIPrefab(prefab)}");
        }

        // remove pawn
        Destroy(promotingPawn);
        promotingPawn = null;

        // ---------- SCALE-NEUTRALIZER UNDER THE MAIN TILE ----------
        Transform neutral = parentTile.Find("ScaleNeutralizer");
        if (!neutral)
        {
            neutral = new GameObject("ScaleNeutralizer").transform;
            neutral.SetParent(parentTile, false);
            neutral.localPosition = Vector3.zero;
            neutral.localRotation = Quaternion.identity;

            // counter the parent's lossy scale so children can use local = world
            Vector3 pl = parentTile.lossyScale;
            neutral.localScale = new Vector3(
                1f / (Mathf.Approximately(pl.x, 0f) ? 1f : pl.x),
                1f / (Mathf.Approximately(pl.y, 0f) ? 1f : pl.y),
                1f / (Mathf.Approximately(pl.z, 0f) ? 1f : pl.z)
            );
        }

        // ---------- INSTANTIATE + IMMEDIATE SAFE PARENT ----------
        GameObject newPiece = Instantiate(prefab);
        // Parent FIRST so nothing grabs it under UI
        newPiece.transform.SetParent(neutral, false);
        newPiece.transform.localPosition = new Vector3(0f, pieceYOffset, 0f);
        newPiece.transform.localRotation = Quaternion.identity;
        newPiece.tag = pawnTag;

        // If somehow it still landed under a Canvas, fix & strip UI bits
        var inCanvas = newPiece.GetComponentInParent<Canvas>();
        if (inCanvas)
        {
            Debug.LogWarning($"PromotionManager: {pieceType} was under Canvas '{inCanvas.name}'. Reparenting to board.");
            newPiece.transform.SetParent(neutral, true);
            var rt = newPiece.GetComponent<RectTransform>(); if (rt) Destroy(rt);
            var cr = newPiece.GetComponent<CanvasRenderer>(); if (cr) Destroy(cr);
        }

        // optional: board-only layer
        int boardLayer = LayerMask.NameToLayer("BoardPieces");
        if (boardLayer >= 0)
        {
            SetLayerRecursively(newPiece, boardLayer);
            parentTile.gameObject.layer = boardLayer;
            neutral.gameObject.layer = boardLayer;
        }

        // ---------- SCALE THE VISUAL NODE ----------
        Transform modelNamed = newPiece.transform.Find("Model") ?? newPiece.transform.Find("model");
        Transform scaleTarget =
            modelNamed ??
            newPiece.GetComponentInChildren<SkinnedMeshRenderer>(true)?.transform ??
            newPiece.GetComponentInChildren<MeshRenderer>(true)?.transform ??
            newPiece.transform;

        scaleTarget.localScale = targetWorldScale;
        StartCoroutine(ReapplyScaleNextFrame(scaleTarget, targetWorldScale));


        promotionPanel?.SetActive(false);

        Debug.Log($"Promoted to {pieceType} on '{parentTile.name}' | path={GetFullPath(parentTile)} | localPos={newPiece.transform.localPosition} | worldPos={newPiece.transform.position} | worldScale={scaleTarget.lossyScale}");
    }

    // ---------- helpers ----------
    private static Transform FindDeepChildByName(Transform root, string name)
    {
        if (!root) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static string GetFullPath(Transform t)
    {
        var path = t.name;
        var p = t.parent;
        while (p != null) { path = p.name + "/" + path; p = p.parent; }
        return path;
    }

    private System.Collections.IEnumerator ReapplyScaleNextFrame(Transform t, Vector3 desiredLocal)
    {
        yield return null;
        if (t) t.localScale = desiredLocal;
    }

    private static bool LooksLikeUIPrefab(GameObject go)
    {
        return go.GetComponentInChildren<RectTransform>(true) != null
            || go.GetComponentInChildren<CanvasRenderer>(true) != null
            || go.GetComponentInChildren<Canvas>(true) != null;
    }

    private static bool LooksLikeBoardMesh(GameObject go)
    {
        return go.GetComponentInChildren<MeshRenderer>(true) != null
            || go.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        var ts = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < ts.Length; i++)
            ts[i].gameObject.layer = layer;
    }
}
