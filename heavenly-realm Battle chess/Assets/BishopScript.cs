using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BishopMovement : MonoBehaviour
{
    /// <summary>
    /// Checks if the bishop can move from its current square to the target square.
    /// </summary>
    public bool IsValidMove(GameObject targetSquare)
    {
        // Convert positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        //Debug.Log($"Bishop currentCoords: {currentCoords}, targetCoords: {targetCoords}, " +
                  //$"xDiff: {xDiff}, zDiff: {zDiff}");

        // A bishop must move diagonally: |xDiff| == |zDiff|
        if (Mathf.Abs(xDiff) != Mathf.Abs(zDiff))
        {
            //Debug.Log("Bishop move is not diagonal!");
            return false;
        }

        // Collect all squares between the current and target positions for a "no-leap" check.
        List<GameObject> squaresBetween = GetSquaresBetween(currentCoords, targetCoords);

        // Check if any square in between is occupied.
        foreach (GameObject square in squaresBetween)
        {
            if (square.transform.childCount > 0)
            {
                //Debug.Log("A piece is blocking the bishop's path.");
                return false;
            }
        }

        // Target square can be empty or occupied by opponent piece.
        if (targetSquare.transform.childCount > 0)
        {
            GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
            if (occupyingPiece.tag == this.tag)
            {
                //Debug.Log("Target square occupied by your own piece. Invalid move.");
                return false;
            }
            else
            {
                //Debug.Log("Bishop can capture the opposing piece.");
            }
        }
        else
        {
            //Debug.Log("Bishop can move to the empty square.");
        }

        return true;
    }

    /// <summary>
    /// Returns a list of squares between two board coordinates (exclusive of start and end).
    /// Used to ensure there are no pieces blocking the bishop's path.
    /// </summary>
    private List<GameObject> GetSquaresBetween(Vector2Int start, Vector2Int end)
    {
        List<GameObject> squares = new List<GameObject>();

        // Determine the direction of movement along both axes
        int xStep = (end.x > start.x) ? 1 : -1;
        int zStep = (end.y > start.y) ? 1 : -1;

        // The number of steps to move diagonally
        int steps = Mathf.Abs(end.x - start.x);

        // Collect squares in between (exclude the final square)
        // We start from the square adjacent to the start and move to one before the end
        for (int i = 1; i < steps; i++)
        {
            int x = start.x + xStep * i;
            int z = start.y + zStep * i;
            GameObject square = GetSquareAtCoordinates(new Vector2Int(x, z));

            if (square != null)
            {
                squares.Add(square);
            }
        }

        return squares;
    }

    /// <summary>
    /// Example method to convert a world position to board coordinates.
    /// Adjust `squareSize` and `boardOrigin` for your specific board setup.
    /// </summary>
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 1.0f; // Your board's tile size
        Vector3 boardOrigin = this.transform.parent.parent.position; // The ChessBoard's position

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Example method to find a square `GameObject` by its board coordinates.
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
