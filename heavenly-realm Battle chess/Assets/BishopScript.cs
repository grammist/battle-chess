using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BishopMovement : MonoBehaviour
{
    public bool IsValidMove(GameObject targetSquare)
    {
        // Convert positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        // ✅ Prevent moving to the same square
        if (currentCoords == targetCoords)
        {
            //Debug.Log("Cannot move bishop to the same square.");
            return false;
        }

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

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

    private List<GameObject> GetSquaresBetween(Vector2Int start, Vector2Int end)
    {
        List<GameObject> squares = new List<GameObject>();

        int xStep = (end.x > start.x) ? 1 : -1;
        int zStep = (end.y > start.y) ? 1 : -1;
        int steps = Mathf.Abs(end.x - start.x);

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

    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 1.0f;
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
