using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingMovement : MonoBehaviour
{
    public bool hasMoved = false;  // True once the king has moved (disallows further castling)

    /// <summary>
    /// Check if the King can move from its current square to the target square,
    /// including normal moves (1 square) and castling (2 squares horizontally).
    /// </summary>
    public bool IsValidMove(GameObject targetSquare)
    {
        // 1. Convert positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        int absX = Mathf.Abs(xDiff);
        int absZ = Mathf.Abs(zDiff);

        Debug.Log($"King current = {currentCoords}, target = {targetCoords}, xDiff = {absX}, zDiff = {absZ}");

        // 2. Normal King Move: at most 1 square in any direction
        if (absX <= 1 && absZ <= 1 && (absX + absZ > 0))
        {
            // If target is empty or occupied by opponent, it's valid
            if (IsSquareFreeOrCapturable(targetSquare))
            {
                Debug.Log("King can move normally.");
                return true;
            }
            else
            {
                Debug.Log("Target square occupied by same-color piece. Invalid.");
                return false;
            }
        }
        // 3. Castling Attempt: King tries to move 2 squares horizontally (zDiff = 0)
        else if (absX == 2 && zDiff == 0)
        {
            Debug.Log("Attempting castling move...");

            // If the King has already moved, can't castle
            if (hasMoved)
            {
                Debug.Log("King already moved. Can't castle.");
                return false;
            }

            bool isKingSide = (xDiff == 3);  // True if moving 2 squares to the right, false if 2 squares to the left
            Debug.Log($"King side: {isKingSide}");

            if (CheckCastlingConditions(currentCoords, isKingSide))
            {
                Debug.Log("Castling conditions met! Allowed castling move.");
                return true;
            }
            else
            {
                Debug.Log("Castling conditions not met.");
                return false;
            }
        }
        else
        {
            Debug.Log("King move is more than one square (and not castling). Invalid move.");
        }

        return false; // Default invalid
    }

    /// <summary>
    /// Checks structural conditions for castling:
    ///  - The corresponding Rook hasn't moved
    ///  - Path between King and Rook is clear
    ///  - (Optional) squares are not under attack, if you want full chess rules
    /// </summary>
    private bool CheckCastlingConditions(Vector2Int kingCoords, bool isKingSide)
    {
        // 1. Identify which corner has the rook
        // For standard 8x8:
        //  - White rooks at (0,0) and (7,0)
        //  - Black rooks at (0,7) and (7,7)
        // Adjust for your coordinate system
        int rookX = isKingSide ? 7 : 0;

        Vector2Int rookCoords = new Vector2Int(rookX, kingCoords.y);

        GameObject rookSquare = GetSquareAtCoordinates(rookCoords);
        if (rookSquare == null || rookSquare.transform.childCount == 0)
        {
            Debug.Log("No rook found in corner for castling.");
            return false;
        }

        GameObject rookObj = rookSquare.transform.GetChild(0).gameObject;
        RookMovement rookMov = rookObj.GetComponent<RookMovement>();
        if (rookMov == null)
        {
            Debug.Log("Piece in corner is not a rook.");
            return false;
        }

        // If the rook has moved or the king has moved, can't castle
        if (rookMov.hasMoved)
        {
            Debug.Log("Rook has already moved. Can't castle.");
            return false;
        }

        // 2. Check squares between King and Rook are empty
        if (!PathIsClearForCastling(kingCoords, rookCoords, isKingSide))
        {
            Debug.Log("Path is not clear between King and Rook.");
            return false;
        }

        // (Optional) 3. Check none of these squares are under attack if you want full classical castling rules

        // Passed structural checks
        return true;
    }

    /// <summary>
    /// Ensures each square between the king and rook is unoccupied.
    /// Example for White's standard row: king at x=4, rook at x=0 or x=7
    /// </summary>
    private bool PathIsClearForCastling(Vector2Int kingCoords, Vector2Int rookCoords, bool isKingSide)
    {
        // If isKingSide: check squares (x=5, row) and (x=6, row) are empty
        // If queen-side: check squares (x=1, row), (x=2, row), (x=3, row) are empty
        int startX = isKingSide ? Mathf.Min(kingCoords.x, rookCoords.x) + 1 : 1;
        Debug.Log($"startX: {startX}");
        int endX = isKingSide ? Mathf.Max(kingCoords.x, rookCoords.x) - 1 : 3;
        Debug.Log($"isKingSide {isKingSide}");

        if (!isKingSide)
        {
            // If the king is at x=4, rook at x=0,
            // we want to check x=1, x=2, x=3 
            startX = 1;
            endX = 3;
        }
        else
        {
            // King side (king at 4, rook at 7),
            // check x=5, x=6
            startX = 5;
            endX = 6;
        }

        int row = kingCoords.y;
        for (int x = startX; x <= endX; x++)
        {
            Vector2Int coords = new Vector2Int(x, row);
            GameObject sq = GetSquareAtCoordinates(coords);
            if (sq == null || sq.transform.childCount > 0)
            {
                Debug.Log($"Square at {coords} is occupied or missing. Path not clear.");
                return false;
            }
        }
        return true;
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
    /// Check if target square is empty or occupied by opponent piece (valid for normal king move).
    /// </summary>
    private bool IsSquareFreeOrCapturable(GameObject targetSquare)
    {
        if (targetSquare.transform.childCount == 0)
        {
            // It's empty
            return true;
        }
        else
        {
            // There's a piece, ensure it's an opponent
            GameObject occupant = targetSquare.transform.GetChild(0).gameObject;
            return occupant.tag != this.tag;
        }
    }

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



    // ------------------------
    // Utility methods:

    private GameObject GetSquareAtCoordinates(Vector2Int coords)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            Vector2Int squareCoords = GetBoardCoordinates(child.position);
            //Debug.Log($"Checking square: {child.name}, squareCoords: {squareCoords}, targetCoords: {coords}");
            if (squareCoords == coords)
            {
                Debug.Log($"Square found: {child.name}");
                return child.gameObject;
            }
        }
        Debug.Log($"No square found at {coords}");
        return null;
    }

    /// <summary>
    /// Example method to convert a world position to board coordinates.
    /// Adjust `squareSize` and `boardOrigin` to match your board.
    /// </summary>
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f;
        Vector3 boardOrigin = this.transform.parent.parent.position; // The ChessBoard's position

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize) + 1;
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }


}
