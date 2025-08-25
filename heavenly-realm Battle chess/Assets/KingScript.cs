using UnityEngine;

public class KingMovement : MonoBehaviour
{
    private generalmoving boardManager;

    private void Awake()
    {
        //boardManager = generalmoving.Instance;
        if (generalmoving.Instance != null)
        {
            boardManager = generalmoving.Instance;
        }
        else
        {
            boardManager = FindObjectOfType<generalmoving>();
        }
    }

    public bool IsValidMove(GameObject targetSquare)
    {
        Vector2Int current = boardManager.GetBoardCoordinates(transform.position);
        Vector2Int target = boardManager.GetBoardCoordinates(targetSquare.transform.position);

        int dx = Mathf.Abs(target.x - current.x);
        int dy = Mathf.Abs(target.y - current.y);

        return (dx <= 1 && dy <= 1 && (dx + dy) > 0);
    }

    public bool IsInCheck()
    {
        Vector2Int kingPos = boardManager.GetBoardCoordinates(transform.position);
        string opponentTag = CompareTag("White") ? "Black" : "White";

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = boardManager.GetSquareAt(x, y);
                if (square.transform.childCount == 0) continue;

                GameObject piece = square.transform.GetChild(0).gameObject;
                if (!piece.CompareTag(opponentTag)) continue;
                if (piece.name.Contains("King")) continue; // ignore enemy king to prevent circular call

                GameObject target = transform.parent.gameObject;

                if (piece.TryGetComponent<PawnMovement>(out var pawn) && pawn.IsValidMove(target)) return true;
                if (piece.TryGetComponent<KnightMovement>(out var knight) && knight.IsValidMove(target)) return true;
                if (piece.TryGetComponent<BishopMovement>(out var bishop) && bishop.IsValidMove(target)) return true;
                if (piece.TryGetComponent<RookMovement>(out var rook) && rook.IsValidMove(target)) return true;
                if (piece.TryGetComponent<QueenMovement>(out var queen) && queen.IsValidMove(target)) return true;
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

                GameObject target = boardManager.GetSquareAt(nx, ny);
                if (target == null) continue;

                if (target.transform.childCount > 0 &&
                    target.transform.GetChild(0).CompareTag(gameObject.tag))
                    continue;

                if (IsValidMove(target) && SimulateMoveAndCheck(gameObject, target))
                    return true;
            }
        }

        return false;
    }

    private bool SimulateMoveAndCheck(GameObject piece, GameObject targetSquare)
    {
        Transform originalParent = piece.transform.parent;
        Transform captured = targetSquare.transform.childCount > 0 ? targetSquare.transform.GetChild(0) : null;

        piece.transform.SetParent(targetSquare.transform);
        if (captured != null) captured.gameObject.SetActive(false);

        bool inCheck = IsInCheck();

        piece.transform.SetParent(originalParent);
        if (captured != null) captured.gameObject.SetActive(true);

        return !inCheck;
    }
}
