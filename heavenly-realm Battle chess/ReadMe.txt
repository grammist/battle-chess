Updated files and new lines

Updated samplescene with gameover UI.

KingScript.cs

  public bool IsInCheck()
    {
        Debug.Log("Checking if king is in check…");

        // 1) Find this king’s square
        Vector2Int myCoords = GetBoardCoordinates(this.transform.parent.position);
        GameObject mySquare = FindBoardSquare(myCoords);
        if (mySquare == null)
        {
            Debug.LogError($"King square not found at {myCoords}");
            return false;
        }

        // 2) Grab your generalmoving so you can ask “can they move here?”
        var gm = FindObjectOfType<generalmoving>();
        if (gm == null)
        {
            Debug.LogError("generalmoving instance not found!");
            return false;
        }

        // 3) Loop all enemy pieces
        Transform board = gm.getboardTransfrom();
        foreach (Transform square in board)
        {
            if (square.childCount == 0) continue;
            var piece = square.GetChild(0).gameObject;
            if (piece.tag == this.tag) continue;              // skip allies
            if (gm.IsValidMove(piece, mySquare))              // can they attack the king?
            {
                Debug.Log($"King is in check by {piece.name} at {GetBoardCoordinates(piece.transform.parent.position)}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the board-tile GameObject at a given coordinate by asking generalmoving’s boardTransform getter.
    /// </summary>
    private GameObject FindBoardSquare(Vector2Int coords)
    {
        var gm = FindObjectOfType<generalmoving>();
        Transform board = gm.getboardTransfrom();
        foreach (Transform sq in board)
        {
            Vector2Int sqCoords = GetBoardCoordinates(sq.position);
            if (sqCoords == coords) return sq.gameObject;
        }
        return null;
    }

/// <summary>
    /// True if the king has at least one legal king-move (ignoring block/capture by other pieces).
    /// </summary>
    public bool HasLegalMoves()
    {
        // Current square coords
        Vector2Int cur = GetBoardCoordinates(this.transform.parent.position);

        // Loop all 8 neighbors
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                Vector2Int test = new Vector2Int(cur.x + dx, cur.y + dz);
                GameObject square = FindBoardSquare(test);
                if (square == null) continue;

                // 1) Can the king *legally* move there at all?
                if (!IsValidMove(square)) continue;

                // 2) Would the king be *in check* if it actually went there?
                if (!WouldBeInCheckAfterMove(square))
                    return true;
            }
        }

        // no escape found
        return false;
    }

    /// <summary>
    /// Simulate moving the king to [targetSquare], check IsInCheck, then restore everything.
    /// </summary>
    private bool WouldBeInCheckAfterMove(GameObject targetSquare)
    {
        Transform originalParent = this.transform.parent;
        Vector3 originalPos = transform.localPosition;
        GameObject captured = null;

        // If there's an enemy there, stash it
        if (targetSquare.transform.childCount > 0)
            captured = targetSquare.transform.GetChild(0).gameObject;

        // 1) Remove any captured piece
        if (captured != null) captured.SetActive(false);

        // 2) Move this king
        transform.SetParent(targetSquare.transform, false);

        // 3) Check
        bool inCheck = IsInCheck();

        // 4) Undo
        transform.SetParent(originalParent, false);
        transform.localPosition = originalPos;
        if (captured != null) captured.SetActive(true);

        return inCheck;
    }

/// <summary>
    /// True if the king has at least one legal king-move (ignoring block/capture by other pieces).
    /// </summary>
    public bool HasLegalMoves()
    {
        // Current square coords
        Vector2Int cur = GetBoardCoordinates(this.transform.parent.position);

        // Loop all 8 neighbors
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                Vector2Int test = new Vector2Int(cur.x + dx, cur.y + dz);
                GameObject square = FindBoardSquare(test);
                if (square == null) continue;

                // 1) Can the king *legally* move there at all?
                if (!IsValidMove(square)) continue;

                // 2) Would the king be *in check* if it actually went there?
                if (!WouldBeInCheckAfterMove(square))
                    return true;
            }
        }

        // no escape found
        return false;
    }

    /// <summary>
    /// Simulate moving the king to [targetSquare], check IsInCheck, then restore everything.
    /// </summary>
    private bool WouldBeInCheckAfterMove(GameObject targetSquare)
    {
        Transform originalParent = this.transform.parent;
        Vector3 originalPos = transform.localPosition;
        GameObject captured = null;

        // If there's an enemy there, stash it
        if (targetSquare.transform.childCount > 0)
            captured = targetSquare.transform.GetChild(0).gameObject;

        // 1) Remove any captured piece
        if (captured != null) captured.SetActive(false);

        // 2) Move this king
        transform.SetParent(targetSquare.transform, false);

        // 3) Check
        bool inCheck = IsInCheck();

        // 4) Undo
        transform.SetParent(originalParent, false);
        transform.localPosition = originalPos;
        if (captured != null) captured.SetActive(true);

        return inCheck;
    }



generalmovement.cs added following:

using TMPro;

 [SerializeField] private GameObject checkmatePanel;      // drag in your panel
 [SerializeField] private TextMeshProUGUI checkmateText;  // drag in the TMP text

// keep this as an instance field:
    private bool isWhiteTurn = true;

void Start()
    {
        player = Camera.main.GetComponent<AudioSource>();


@@ -293,8 +304,30 @@ public class generalmoving : MonoBehaviour
        }

        curObject = null; // Clear current piece reference

	//new part
        string losingSide = isWhiteTurn ? "White" : "Black";
        if (IsCheckmate(losingSide))
        {
            ShowCheckmateUI(losingSide);
            return; // stop further turn switching
        }
	//end of new part

    }

/// <summary>
    /// Returns true only if the side whose king has 'kingTag' is in checkmate.
    /// </summary>
    private bool IsCheckmate(string kingTag)
    {
        // locate that king
        var king = FindObjectsOfType<KingMovement>()
                   .FirstOrDefault(k => k.CompareTag(kingTag));
        if (king == null) return false;
        // must be in check…
        if (!king.IsInCheck()) return false;
        // …and king must have no escape squares
        if (king.HasLegalKingMoves()) return false;
        // now try every other friendly piece to see if it can block or capture the attacker
        foreach (Transform sq in boardTransform)
        {
            if (sq.childCount == 0) continue;
            var piece = sq.GetChild(0).gameObject;
            if (!piece.CompareTag(kingTag)) continue;

            // try moving it to every square
            foreach (Transform dest in boardTransform)
            {
                if (dest.childCount > 0 && dest.GetChild(0).tag == kingTag) continue;
                if (!IsValidMove(piece, dest.gameObject)) continue;

                // simulate
                var origParent = piece.transform.parent;
                var captured = dest.childCount == 1 ? dest.GetChild(0).gameObject : null;
                piece.transform.SetParent(dest, false);
                if (captured) Destroy(captured);

                bool stillInCheck = king.IsInCheck();

                // undo
                piece.transform.SetParent(origParent, false);
                if (captured) Instantiate(captured, dest.position, Quaternion.identity)
                              .transform.SetParent(dest, false);

                if (!stillInCheck) return false;
            }
        }
        // no escapes, no blocks → checkmate!
        return true;
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

