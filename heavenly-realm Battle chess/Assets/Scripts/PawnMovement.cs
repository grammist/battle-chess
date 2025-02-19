using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMovement : MonoBehaviour
{
    public bool enPassantVulnerable = false;

    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f; // Adjust this if each square spans more than 1 world unit
        Vector3 boardOrigin = this.transform.parent.parent.position; // ChessBoard's position

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        Debug.Log($"World position: {worldPosition}, Board origin: {boardOrigin}, Calculated coordinates: ({x}, {z})");
        return new Vector2Int(x, z);
    }



    private GameObject GetSquareAtCoordinates(Vector2Int coords)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            Vector2Int squareCoords = GetBoardCoordinates(child.position);
            Debug.Log($"Checking square: {child.name}, squareCoords: {squareCoords}, targetCoords: {coords}");
            if (squareCoords == coords)
            {
                Debug.Log($"Square found: {child.name}");
                return child.gameObject;
            }
        }
        Debug.Log($"No square found at {coords}");
        return null;
    }



    public bool IsValidMove(GameObject targetSquare)
    {
        Debug.Log("IsValidMove function");
        //int direction = (this.tag == "White") ? -1 : 1;
        //int direction = (this.tag == "White") ? -1 : 1;
        int direction = 0;
        if (this.tag == "White")
        {
            // White moves “down” (decreasing z)
            direction = -1;
        }
        else if (this.tag == "Black")
        {
            // Black moves “up” (increasing z)
            direction = 1;
        }
        Debug.Log($"Direction: , {direction}");

        // Convert positions to board coordinates
        Vector2Int currentCoords = GetBoardCoordinates(this.transform.parent.position);
        Vector2Int targetCoords = GetBoardCoordinates(targetSquare.transform.position);

        int xDiff = targetCoords.x - currentCoords.x;
        int zDiff = targetCoords.y - currentCoords.y;

        Debug.Log($"Pawn current coords: {currentCoords}, Target coords: {targetCoords}");
        Debug.Log($"xDiff: {xDiff}, zDiff: {zDiff}, direction: {direction}");
        Debug.Log($"{this.tag}, Pawn Direction: {direction}, Expected zDiff for one-step forward: {direction}");


        // Check for standard forward move
        if (xDiff == 0 && zDiff == direction)
        {
            if (targetSquare.transform.childCount == 0)
                return true;
        }

        // Check for two-square forward move
        // Check for two-square forward move (only on the first move)
        if (xDiff == 0 && zDiff == 2 * direction)
        {
            int startingRow = (this.tag == "White") ? 0 : -5;
            //int startingRow = (this.tag == "White") ? 1 : 6;
            Debug.Log($"Checking two-square move: Current row = {currentCoords.y}, Starting row = {startingRow}");

            if (currentCoords.y == startingRow && targetSquare.transform.childCount == 0)
            {
                Vector2Int midCoords = new Vector2Int(currentCoords.x, currentCoords.y + direction);
                GameObject midSquare = GetSquareAtCoordinates(midCoords);

                if (midSquare != null)
                {
                    Debug.Log($"Mid square found: {midSquare.name}, Child count: {midSquare.transform.childCount}");
                    if (midSquare.transform.childCount == 0)
                    {
                        Debug.Log("Two-square move is valid!");
                        enPassantVulnerable = true;  // Enable En Passant capture
                        Debug.Log($"En passent is vulnerable");
                        return true;
                    }
                }
                else
                {
                    Debug.Log("Mid square not found!");
                }
            }
        }


        // Check for diagonal capture
        if (Mathf.Abs(xDiff) == 1 && zDiff == direction)
        {
            if (targetSquare.transform.childCount > 0) // Check if there is a piece to capture
            {
                if (targetSquare.transform.GetChild(0).tag != this.tag)
                {
                    return true; // Normal capture
                }
            }
            else
            {
                // Check for En Passant capture
                return CheckEnPassantCapture(currentCoords, targetCoords, direction);
            }
        }

        return false;
    }

    private bool CheckEnPassantCapture(Vector2Int currentCoords, Vector2Int targetCoords, int direction)
    {
        // The "passed" pawn is behind the target square by 1 in z
        Vector2Int behindCoords = new Vector2Int(targetCoords.x, targetCoords.y - direction);

        GameObject behindSquare = GetSquareAtCoordinates(behindCoords);
        if (behindSquare != null && behindSquare.transform.childCount > 0)
        {
            GameObject behindPawn = behindSquare.transform.GetChild(0).gameObject;
            PawnMovement behindPawnMovement = behindPawn.GetComponent<PawnMovement>();

            if (behindPawnMovement != null && behindPawnMovement.enPassantVulnerable)
            {
                Debug.Log("En Passant capture is valid.");
                return true;
            }
        }
        return false;
    }




    private GameObject GetSquareAtPosition(Vector3 position)
    {
        foreach (Transform child in this.transform.parent.parent)
        {
            Debug.Log($"Checking square at position: {child.position}");
            if (Vector3.Distance(child.position, position) < 0.1f)
            {
                Debug.Log($"Square found at position {position}");
                return child.gameObject;
            }
        }
        Debug.Log($"No square found at position {position}");
        return null;
    }
}
