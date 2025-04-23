using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingMovement : MonoBehaviour
{
    /// <summary>
    /// Check if the King can move from its current square to the target square.
    /// </summary>
    public bool IsValidMove(GameObject targetSquare)
    {
        // Get the board coordinates for the King's current square and the target square.
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = Mathf.Abs(targetCoords.x - currentCoords.x);
        int zDiff = Mathf.Abs(targetCoords.y - currentCoords.y);

        //Debug.Log($"King current = {currentCoords}, target = {targetCoords}, xDiff = {xDiff}, zDiff = {zDiff}");

        // 1. The King can move at most 1 square horizontally, vertically, or diagonally.
        if (xDiff <= 1 && zDiff <= 1 && (xDiff + zDiff > 0))
        {
            // 2. Check if the target square is empty or occupied by opponent's piece.
            if (targetSquare.transform.childCount == 0)
            {
                //Debug.Log("King can move to empty square.");
                return true;
            }
            else
            {
                // If there's a piece, ensure it's not the same tag (i.e., not your own piece).
                GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
                if (occupyingPiece.tag != this.tag)
                {
                    //Debug.Log("King can capture the opposing piece.");
                    return true;
                }
                else
                {
                    //Debug.Log("Target square occupied by same-color piece. Invalid move.");
                }
            }
        }
        else
        {
            //Debug.Log("King move is more than one square away. Invalid move.");
        }

        return false; // Default invalid
    }

    /// <summary>
    /// Example method to convert a world position to board coordinates.
    /// Adjust `squareSize` and `boardOrigin` to match your board.
    /// </summary>
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f;
        Vector3 boardOrigin = this.transform.parent.parent.position; // The ChessBoard's position

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }
}
