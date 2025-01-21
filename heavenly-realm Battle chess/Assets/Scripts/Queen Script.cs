using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenMovement : MonoBehaviour
{
    /// <summary>
    /// Checks if the queen can move from its current square to the target square.
    /// </summary>
    public bool IsValidMove(GameObject targetSquare)
    {
        // 1. Convert current and target positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        Debug.Log($"Queen currentCoords: {currentCoords}, targetCoords: {targetCoords}, xDiff: {xDiff}, zDiff: {zDiff}");

        // 2. Check movement direction to see if it's valid for a queen
        //    The queen moves like a rook OR bishop: horizontal, vertical, or diagonal
        bool isHorizontal = (zDiff == 0 && xDiff != 0);
        bool isVertical = (xDiff == 0 && zDiff != 0);
        bool isDiagonal = (Mathf.Abs(xDiff) == Mathf.Abs(zDiff));

        // If it's neither horizontal, vertical, nor diagonal, it's invalid
        if (!isHorizontal && !isVertical && !isDiagonal)
        {
            Debug.Log("Invalid queen move: not in a straight or diagonal line.");
            return false;
        }

        // 3. Get all squares along the path from current to target (excluding the starting and target squares)
        List<GameObject> squaresBetween = GetSquaresBetween(currentCoords, targetCoords);

        // 4. Ensure the path is clear
        foreach (GameObject square in squaresBetween)
        {
            if (square.transform.childCount > 0)
            {
                Debug.Log("A piece is blocking the queen's path.");
                return false;
            }
        }

        // 5. Check the target square
        if (targetSquare.transform.childCount > 0)
        {
            // If occupied, ensure it's an opponent piece
            GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
            if (occupyingPiece.tag == this.tag)
            {
                Debug.Log("Target square occupied by same-color piece. Invalid move.");
                return false;
            }
            else
            {
                Debug.Log("Queen can capture the opponent piece.");
            }
        }
        else
        {
            Debug.Log("Queen can move to the empty square.");
        }

        return true; // Passes all checks
    }

    /// <summary>
    /// Retrieves all squares between the current and target positions (exclusive).
    /// If the line is diagonal, we move diagonally; if horizontal/vertical, we move in a straight line.
    /// </summary>
    private List<GameObject> GetSquaresBetween(Vector2Int start, Vector2Int end)
    {
        List<GameObject> squares = new List<GameObject>();

        int xDiff = end.x - start.x;
        int zDiff = end.y - start.y;

        int xDirection = (xDiff == 0) ? 0 : (xDiff > 0 ? 1 : -1);
        int zDirection = (zDiff == 0) ? 0 : (zDiff > 0 ? 1 : -1);

        // Number of steps we need to take (for diagonal, horizontal, or vertical)
        int steps = Mathf.Max(Mathf.Abs(xDiff), Mathf.Abs(zDiff));

        // Traverse from the square next to start until just before end
        for (int i = 1; i < steps; i++)
        {
            int nextX = start.x + xDirection * i;
            int nextZ = start.y + zDirection * i;
            GameObject square = GetSquareAtCoordinates(new Vector2Int(nextX, nextZ));
            if (square != null)
            {
                squares.Add(square);
            }
        }

        return squares;
    }

    /// <summary>
    /// Example method to convert a world position to board coordinates.
    /// Adjust squareSize and boardOrigin to match your board setup.
    /// </summary>
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f;
        Vector3 boardOrigin = this.transform.parent.parent.position; // The ChessBoard's transform

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Example method to find a square by its board coordinates.
    /// Assumes each square is a child of the board parent.
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
