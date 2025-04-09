using UnityEngine;
using UnityEngine.UI;

public class PromotionUI : MonoBehaviour
{
    public static PromotionUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject panel; // The panel with 4 buttons

    // The pawn we want to promote
    private GameObject pawnToPromote;

    private void Awake()
    {
        // Basic singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide the panel initially
        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// Called by generalmoving to show the choice panel.
    /// </summary>
    public void ShowPromotionPanel(GameObject pawn)
    {
        pawnToPromote = pawn;
        if (panel != null) panel.SetActive(true);
    }

    private void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    // Hook these to the OnClick events of your 4 buttons in the Inspector

    public void OnSelectQueen()
    {
        PromoteChoice("Queen");
    }

    public void OnSelectRook()
    {
        PromoteChoice("Rook");
    }

    public void OnSelectBishop()
    {
        PromoteChoice("Bishop");
    }

    public void OnSelectKnight()
    {
        PromoteChoice("Knight");
    }

    private void PromoteChoice(string pieceType)
    {
        // Find or reference your generalmoving instance:
        // If it's a singleton, do generalmoving.Instance, or:
        generalmoving gm = FindObjectOfType<generalmoving>();

        if (pawnToPromote != null && gm != null)
        {
            // Actually do the promotion
            gm.PerformPromotion(pawnToPromote, pieceType);
        }

        // Clear reference, hide panel
        pawnToPromote = null;
        HidePanel();
    }
}
