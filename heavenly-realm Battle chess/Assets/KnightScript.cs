using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightMovement : MonoBehaviour
{
    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f; // Adjust this if each square spans more than 1 world unit
        Vector3 boardOrigin = this.transform.parent.parent.position; // ChessBoard's position

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        //Debug.Log($"World position: {worldPosition}, Board origin: {boardOrigin}, Calculated coordinates: ({x}, {z})");
        return new Vector2Int(x, z);
    }

    private GameObject GetSquareAtCoordinates(Vector2Int coords)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            Vector2Int squareCoords = GetBoardCoordinates(child.position);
            //Debug.Log($"Checking square: {child.name}, squareCoords: {squareCoords}, targetCoords: {coords}");
            if (squareCoords == coords)
            {
                //Debug.Log($"Square found: {child.name}");
                return child.gameObject;
            }
        }
        //Debug.Log($"No square found at {coords}");
        return null;
    }

    public bool IsValidMove(GameObject targetSquare)
    {
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = Mathf.Abs(targetCoords.x - currentCoords.x);
        int zDiff = Mathf.Abs(targetCoords.y - currentCoords.y);

        //Debug.Log($"Knight current coords: {currentCoords}, target coords: {targetCoords}, xDiff: {xDiff}, zDiff: {zDiff}");

        // Check if the move matches the "L" shape (2 in one direction, 1 in the other)
        if ((xDiff == 2 && zDiff == 1) || (xDiff == 1 && zDiff == 2))
        {
            //Debug.Log("Valid L-shaped move for knight");

            // Check if the target square is empty or has an opponent piece
            if (targetSquare.transform.childCount == 0)
            {
                //Debug.Log("Target square is empty");
                return true;
            }
            else
            {
                GameObject occupyingPiece = targetSquare.transform.GetChild(0).gameObject;
                if (occupyingPiece.tag != this.tag)
                {
                    //Debug.Log("Target square contains opponent's piece");
                    return true;
                }
            }
        }

        //Debug.Log("Move is invalid for knight");
        return false;
    }

    private GameObject GetSquareAtPosition(Vector3 position)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            //Debug.Log($"Checking square at position: {child.position}");
            if (Vector3.Distance(child.position, position) < 0.1f)
            {
                //Debug.Log($"Square found at position {position}");
                return child.gameObject;
            }
        }
        //Debug.Log($"No square found at position {position}");
        return null;
    }
}
