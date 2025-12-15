using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Main Board Root (optional: only used if generalmoving.Instance is missing)")]
    [SerializeField] private Transform mainBoardRoot;

    [Header("Desired WORLD Scales")]
    public Vector3 queenScale = new Vector3(0.005f, 0.015f, 0.005f);
    public Vector3 rookScale = new Vector3(0.015f, 0.060f, 0.015f);
    public Vector3 bishopScale = new Vector3(0.025f, 0.075f, 0.025f);
    public Vector3 knightScale = new Vector3(0.35f, 1.40f, 0.35f);

    [Header("Placement")]
    [Tooltip("Vertical lift from tile center (in local units)")]
    public float pieceYOffset = 0.5f;

    private GameObject promotingPawn;

    [Header("Promotion Styling (optional)")]
    [SerializeField] private Vector3 blackQueenScale = new Vector3(0.005f, 0.015f, 0.005f);
    [SerializeField] private float blackQueenYOffset = 0.5f;                     // vertical lift after scaling


    private void Start()
    {
        if (!queenButton || !rookButton || !bishopButton || !knightButton)
        {
            Debug.LogError("PromotionManager: One or more button references are missing.");
            return;
        }

        if (promotionPanel) promotionPanel.SetActive(false);

        queenButton.onClick.AddListener(() => Promote("Queen"));
        rookButton.onClick.AddListener(() => Promote("Rook"));
        bishopButton.onClick.AddListener(() => Promote("Bishop"));
        knightButton.onClick.AddListener(() => Promote("Knight"));
    }

    public void ShowPromotionUI(GameObject pawn)
    {
        promotingPawn = pawn;
        if (promotionPanel) promotionPanel.SetActive(true);
    }

    public void Promote(string pieceType)
    {
        if (!promotingPawn)
        {
            Debug.LogError("PromotionManager: promotingPawn is null.");
            return;
        }

        // --- figure out side / tag ---
        bool isWhite = promotingPawn.CompareTag("White");
        string pawnTag = promotingPawn.tag;

        // --- climb to the tile object (A1..H8) ---
        Transform tile = promotingPawn.transform.parent;
        while (tile != null && !IsTileName(tile.name))
            tile = tile.parent;

        if (!tile)
        {
            Debug.LogError("PromotionManager: could not resolve tile from pawn's parents.");
            return;
        }

        // Prefer the MAIN board tile from generalmoving, if available
        if (generalmoving.Instance != null)
        {
            Transform mainTile = generalmoving.Instance.GetMainTile(tile.name);
            if (mainTile != null) tile = mainTile;
        }

        // --- pick prefab ---
        GameObject prefab = null;
        switch (pieceType)
        {
            case "Queen": prefab = isWhite ? whiteQueenModel : blackQueenModel; break;
            case "Rook": prefab = isWhite ? whiteRookModel : blackRookModel; break;
            case "Bishop": prefab = isWhite ? whiteBishopModel : blackBishopModel; break;
            case "Knight": prefab = isWhite ? whiteKnightModel : blackKnightModel; break;
            default:
                Debug.LogError("PromotionManager: unknown pieceType " + pieceType);
                return;
        }
        if (!prefab)
        {
            Debug.LogError("PromotionManager: missing prefab for " + pieceType);
            return;
        }
        Vector3 queenSize = new Vector3(0.25f, 0.25f, 0.25f);   


        // --- remember the pawn’s local transform on that tile ---
        Vector3 pawnLocalPos = promotingPawn.transform.localPosition;
        Quaternion pawnLocalRot = promotingPawn.transform.localRotation;
        Vector3 pawnLocalScale = promotingPawn.transform.localScale;

        // --- remove pawn ---
        Destroy(promotingPawn);
        promotingPawn = null;

        // --- instantiate under the same tile, using the pawn’s local transform ---
        GameObject newPiece = Instantiate(prefab, tile, false);

        newPiece.transform.localPosition = pawnLocalPos;
        newPiece.transform.localRotation = pawnLocalRot;
        newPiece.transform.localScale = pawnLocalScale;

        // if BLACK QUEEN, override scale/height
        if (!isWhite && pieceType == "Queen")
        {
            newPiece.transform.localScale = blackQueenScale; //
                                                             // place slightly above the tile center (world Y), in case the mesh needs lift
            newPiece.transform.position = new Vector3(
                tile.position.x,
                tile.position.y + blackQueenYOffset,
                tile.position.z
            );
        }

        newPiece.tag = pawnTag;
        newPiece.name = pieceType;   // or $"White {pieceType}" / $"Black {pieceType}"

        promotionPanel?.SetActive(false);
        generalmoving.Instance?.OnPromotionFinished(newPiece);

        Debug.Log($"[Promote] {pieceType} on {tile.name}, localPos={newPiece.transform.localPosition}, localScale={newPiece.transform.localScale}");
    }




    // --------- helpers ---------

    private float FindReferencePieceHeight(string tag)
    {
        // try to copy height from any existing board piece of the same side
        var all = FindObjectsOfType<HoverChangeColor>(true);
        foreach (var h in all)
        {
            if (h != null && h.CompareTag(tag))
            {
                var rr = h.GetComponentInChildren<Renderer>(true);
                if (rr != null) return rr.bounds.size.y;
            }
        }
        // sensible fallback if nothing is on the board yet
        return 1.6f; // tweak to match your set (try 1.4–1.8)
    }


    private static float SafeInv(float v) => Mathf.Approximately(v, 0f) ? 1f : 1f / v;

    /*private static bool IsTileName(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        // Expect something like "A1".."H8" (allow suffixes like " (Clone)")
        char file = '\0';
        int rank = -1;

        // first letter A..H
        for (int i = 0; i < n.Length; i++)
        {
            if (n[i] >= 'A' && n[i] <= 'H') { file = n[i]; break; }
        }
        if (file == '\0') return false;

        // first digit sequence 1..8
        for (int i = 0; i < n.Length; i++)
        {
            if (char.IsDigit(n[i]))
            {
                // parse one or two digits
                int j = i;
                while (j < n.Length && char.IsDigit(n[j])) j++;
                int.TryParse(n.Substring(i, j - i), out rank);
                break;
            }
        }
        return rank >= 1 && rank <= 8;
    }*/

    private bool IsTileName(string n)
    {
        if (string.IsNullOrEmpty(n) || n.Length < 2) return false;
        char f = n[0];
        if (f < 'A' || f > 'H') return false;
        return n.Any(char.IsDigit);   // there is some rank number in the name
    }


    private static Transform FindDeepChildByName(Transform root, string name)
    {
        if (!root) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        var ts = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < ts.Length; i++)
            ts[i].gameObject.layer = layer;
    }
}
