using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookMovement : MonoBehaviour
{
    // This method checks if the target square is a valid move for the rook.
    public bool IsValidMove(GameObject targetSquare)
    {
        // Convert positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        Debug.Log($"Rook currentCoords: {currentCoords}, targetCoords: {targetCoords}, xDiff: {xDiff}, zDiff: {zDiff}");

        // Rook must move either horizontally or vertically.
        // If both xDiff and zDiff are non-zero, it's invalid.
        if (xDiff != 0 && zDiff != 0)
        {
            Debug.Log("Rook cannot move diagonally.");
            return false;
        }

        // Determine the direction of movement along x or z.
        // We'll collect all squares between the current and target positions for a "no-leap" check.
        List<GameObject> squaresBetween = GetSquaresBetween(currentCoords, targetCoords);

        // Check if any square in between is occupied.
        foreach (GameObject square in squaresBetween)
        {
            if (square.transform.childCount > 0)
            {
                Debug.Log("A piece is blocking the rook's path.");
                return false;
            }
        }

        // If the target square has an opposing piece, capturing is allowed;
        // if it's empty, moving is allowed; if it's your own piece, it's invalid.
        if (targetSquare.transform.childCount > 0)
        {
            GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
            if (occupyingPiece.tag == this.tag)
            {
                Debug.Log("Target square is occupied by your own piece. Invalid move.");
                return false;
            }
            else
            {
                Debug.Log("Rook can capture the opposing piece.");
            }
        }
        else
        {
            Debug.Log("Rook can move to the empty square.");
        }

        return true;
    }

    /// <summary>
    /// Returns a list of squares between two board coordinates, excluding the starting and target squares.
    /// </summary>
    private List<GameObject> GetSquaresBetween(Vector2Int start, Vector2Int end)
    {
        List<GameObject> squares = new List<GameObject>();

        // If on the same row, x changes; if on the same column, z changes.
        if (start.x == end.x) // Vertical move
        {
            // Move up or down along z
            int minZ = Mathf.Min(start.y, end.y);
            int maxZ = Mathf.Max(start.y, end.y);

            // Collect squares in between
            for (int z = minZ + 1; z < maxZ; z++)
            {
                GameObject square = GetSquareAtCoordinates(new Vector2Int(start.x, z));
                if (square != null)
                {
                    squares.Add(square);
                }
            }
        }
        else // Horizontal move
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);

            // Collect squares in between
            for (int x = minX + 1; x < maxX; x++)
            {
                GameObject square = GetSquareAtCoordinates(new Vector2Int(x, start.y));
                if (square != null)
                {
                    squares.Add(square);
                }
            }
        }

        return squares;
    }

    /// <summary>
    /// Example method to convert a world position to board coordinates.
    /// Adjust `squareSize` and `boardOrigin` to match your setup.
    /// </summary>
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f; // Adjust for your board tile size
        Vector3 boardOrigin = this.transform.parent.parent.position; // ChessBoard transform

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Example method to find a square GameObject by its board coordinates.
    /// Assumes your board squares are children of the board parent.
    /// </summary>
    private GameObject GetSquareAtCoordinates(Vector2Int coords)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            Vector2Int squareCoords = GetBoardCoordinates(child.position);
            if (squareCoords == coords)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
