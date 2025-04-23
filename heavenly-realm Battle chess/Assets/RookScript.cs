using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookMovement : MonoBehaviour
{
    public bool IsValidMove(GameObject targetSquare)
    {
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        // ✅ Prevent moving to the same square
        if (currentCoords == targetCoords)
        {
            //Debug.Log("Cannot move rook to the same square.");
            return false;
        }

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        // Rook must move either horizontally or vertically.
        if (xDiff != 0 && zDiff != 0)
        {
            //Debug.Log("Rook cannot move diagonally.");
            return false;
        }

        // Check for any blocking pieces
        List<GameObject> squaresBetween = GetSquaresBetween(currentCoords, targetCoords);

        foreach (GameObject square in squaresBetween)
        {
            if (square.transform.childCount > 0)
            {
                //Debug.Log("A piece is blocking the rook's path.");
                return false;
            }
        }

        // Target square may be empty or have enemy piece
        if (targetSquare.transform.childCount > 0)
        {
            GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
            if (occupyingPiece.tag == this.tag)
            {
                //Debug.Log("Target square is occupied by your own piece. Invalid move.");
                return false;
            }
            else
            {
                //Debug.Log("Rook can capture the opposing piece.");
            }
        }
        else
        {
            //Debug.Log("Rook can move to the empty square.");
        }

        return true;
    }

    private List<GameObject> GetSquaresBetween(Vector2Int start, Vector2Int end)
    {
        List<GameObject> squares = new List<GameObject>();

        if (start.x == end.x) // Vertical move
        {
            int minZ = Mathf.Min(start.y, end.y);
            int maxZ = Mathf.Max(start.y, end.y);

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

    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f;
        Vector3 boardOrigin = this.transform.parent.parent.position;

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }

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
