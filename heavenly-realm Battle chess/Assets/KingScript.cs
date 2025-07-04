using System.Collections.Generic;
using UnityEngine;

public class KingMovement : MonoBehaviour
{
    private generalmoving boardManager;
    private Vector2Int kingPos;

    void Start()
    {
        boardManager = FindObjectOfType<generalmoving>();
    }

    public bool IsValidMove(GameObject targetSquare)
    {
        Vector2Int current = boardManager.GetBoardCoordinates(transform.position);
        Vector2Int target = boardManager.GetBoardCoordinates(targetSquare.transform.position);

        int dx = Mathf.Abs(target.x - current.x);
        int dy = Mathf.Abs(target.y - current.y);

        // King moves 1 square in any direction
        return (dx <= 1 && dy <= 1 && (dx + dy) > 0);
    }

    public bool IsInCheck()
    {
        Vector2Int kingPos = boardManager.GetBoardCoordinates(transform.position);
        string opponentTag = CompareTag("White") ? "Black" : "White";

        // Check all pieces on the board
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                //GameObject square = boardManager.getGridBoard[x, y];
                GameObject square = boardManager.GetSquareAt(x, y);
                if (square.transform.childCount == 0) continue;

                GameObject piece = square.transform.GetChild(0).gameObject;
                if (!piece.CompareTag(opponentTag)) continue;

                if (boardManager.IsValidMove(piece, transform.parent.gameObject))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasLegalMoves()
    {
        Vector2Int current = boardManager.GetBoardCoordinates(transform.position);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = current.x + dx;
                int ny = current.y + dy;

                if (nx < 0 || ny < 0 || nx >= 8 || ny >= 8) continue;

                //GameObject target = boardManager.gridBoard[nx, ny];
                GameObject target = boardManager.GetSquareAt(nx, ny);

                if (target.transform.childCount > 0 &&
                    target.transform.GetChild(0).CompareTag(gameObject.tag))
                {
                    continue; // can't capture own piece
                }

                if (IsValidMove(target))
                {
                    // simulate move
                    Transform originalParent = transform.parent;
                    Transform captured = target.transform.childCount > 0 ? target.transform.GetChild(0) : null;

                    transform.SetParent(target.transform);
                    if (captured != null) captured.gameObject.SetActive(false);

                    bool stillInCheck = IsInCheck();

                    // revert move
                    transform.SetParent(originalParent);
                    if (captured != null) captured.gameObject.SetActive(true);

                    if (!stillInCheck)
                        return true;
                }
            }
        }

        return false;
    }
}