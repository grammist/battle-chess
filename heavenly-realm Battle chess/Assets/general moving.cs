using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generalmoving : MonoBehaviour
{
    private float moveSpeed = 10.0f;
    private bool allowClick = true;

    public GameObject chess;
    public GameObject targetSquare;
    public GameObject curObject;
    public static GameObject curGrid;

    private Transform lastTargetParent;
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    [SerializeField] public AudioClip seM;
    private AudioSource player;

    // Multiple piece prefabs for promotion; assign these in Inspector
    [SerializeField] private GameObject whiteQueenPrefab;
    [SerializeField] private GameObject blackQueenPrefab;
    [SerializeField] private GameObject whiteRookPrefab;
    [SerializeField] private GameObject blackRookPrefab;
    [SerializeField] private GameObject whiteBishopPrefab;
    [SerializeField] private GameObject blackBishopPrefab;
    [SerializeField] private GameObject whiteKnightPrefab;
    [SerializeField] private GameObject blackKnightPrefab;

    [SerializeField] private Transform boardTransform;

    void Start()
    {
        player = Camera.main.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Transform hitTransform = hit.transform;

            if (hitTransform != null)
            {
                if (Input.GetMouseButtonDown(0) && allowClick && curObject != null && hitTransform.childCount < 2)
                {
                    if (curGrid == null)
                    {
                        // First click
                        curGrid = hitTransform.gameObject;
                        HighlightGrid(curGrid, Color.blue);
                    }
                    else if (curGrid == hitTransform.gameObject)
                    {
                        // Second click to confirm
                        if (hitTransform.childCount == 0)
                        {
                            Debug.Log("Child count = 0");
                            lastTargetParent = hitTransform;
                            if (IsValidMove(curObject, hitTransform.gameObject))
                            {
                                allowClick = false;
                                Move(curObject, hitTransform.gameObject);
                            }
                            else
                            {
                                Debug.Log("Invalid move!");
                                allowClick = true;
                                ResetGridHighlight();
                                curGrid = null;
                            }
                        }
                        else if (hitTransform.GetChild(0).tag != curObject.tag)
                        {
                            // Attempt a capture
                            Debug.Log("Attempting capture");
                            allowClick = false;
                            lastTargetParent = hitTransform;
                            if (IsValidMove(curObject, hitTransform.gameObject))
                            {
                                allowClick = false;
                                Move(curObject, hitTransform.gameObject);
                            }
                            else
                            {
                                Debug.Log("Invalid capture attempt!");
                                allowClick = true;
                                ResetGridHighlight();
                                curGrid = null;
                            }
                        }

                        ResetGridHighlight();
                    }
                    else // a different grid => cancel selection
                    {
                        ResetGridHighlight();
                        curGrid = hitTransform.gameObject;
                        HighlightGrid(curGrid, Color.blue);
                    }
                }
            }
        }
    }

    private bool IsValidMove(GameObject chessObject, GameObject targetSquare)
    {
        Debug.Log("IsValidMove");
        Debug.Log($"Clicked object is {curObject.name}");

        // Pawn
        PawnMovement pawnMovement = chessObject.GetComponentInChildren<PawnMovement>();
        if (pawnMovement != null)
        {
            Debug.Log("Pawn movement");
            return pawnMovement.IsValidMove(targetSquare);
        }

        // Knight
        KnightMovement knightMovement = chessObject.GetComponent<KnightMovement>();
        if (knightMovement != null)
        {
            Debug.Log("Knight movement");
            return knightMovement.IsValidMove(targetSquare);
        }

        // Rook
        RookMovement rookMovement = chessObject.GetComponent<RookMovement>();
        if (rookMovement != null)
        {
            Debug.Log("Rook movement");
            return rookMovement.IsValidMove(targetSquare);
        }

        // Bishop
        BishopMovement bishopMovement = chessObject.GetComponent<BishopMovement>();
        if (bishopMovement != null)
        {
            Debug.Log("Bishop movement");
            return bishopMovement.IsValidMove(targetSquare);
        }

        // King
        KingMovement kingMovement = chessObject.GetComponent<KingMovement>();
        if (kingMovement != null)
        {
            Debug.Log("King Movement");
            return kingMovement.IsValidMove(targetSquare);
        }

        // Queen
        QueenMovement queenMovement = chessObject.GetComponent<QueenMovement>();
        if (queenMovement != null)
        {
            Debug.Log("Queen movement");
            return queenMovement.IsValidMove(targetSquare);
        }

        return true;
    }

    private void OnEnable()
    {
        HoverChangeColor.OnObjectClicked += HandleObjectClicked;
        TimeoutPenalty.OnTimeOut += HandleTimeOut;
    }

    private void OnDisable()
    {
        HoverChangeColor.OnObjectClicked -= HandleObjectClicked;
        TimeoutPenalty.OnTimeOut -= HandleTimeOut;
    }

    private void HandleObjectClicked(GameObject clickedObject)
    {
        curObject = clickedObject;
    }

    private void HandleTimeOut()
    {
        if (curObject != null)
        {
            curObject.GetComponent<HoverChangeColor>().unClick();
        }
        curObject = null;
        ResetGridHighlight();
        curGrid = null;
    }

    void Move(GameObject chessObject, GameObject targetSquareObject)
    {
        if (chessObject == null || targetSquareObject == null)
        {
            Debug.LogError("Chess Object or Target Square Object is null!");
            return;
        }

        float originalY = chessObject.transform.position.y;
        Vector3 targetPosition = new Vector3(
            targetSquareObject.transform.position.x,
            originalY,
            targetSquareObject.transform.position.z
        );

        StartCoroutine(MoveToTarget(chessObject, targetSquareObject.transform, targetPosition, moveSpeed, OnMoveCompleted));
    }

    public IEnumerator MoveToTarget(GameObject chessObject, Transform targetParent, Vector3 targetPosition, float moveSpeed, Action onMoveComplete)
    {
        while (Vector3.Distance(chessObject.transform.position, targetPosition) > 0.01f)
        {
            chessObject.transform.position = Vector3.MoveTowards(
                chessObject.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        chessObject.transform.SetParent(targetParent);

        // Re-enable clicking after the move
        allowClick = true;

        onMoveComplete?.Invoke();
    }

    private void OnMoveCompleted()
    {
        // If capturing (square had a piece), remove the occupant
        if (lastTargetParent != null && lastTargetParent.childCount == 2)
        {
            Transform child = lastTargetParent.GetChild(0);
            Destroy(child.gameObject);
        }

        // If we had an SFX, play it
        player.PlayOneShot(seM);

        // (Optional) Next turn logic
        GameManager.NextState();

        // Un-click visuals
        if (curObject != null)
        {
            curObject.GetComponent<HoverChangeColor>().unClick();
            // Check if it's a pawn that needs promotion
            PawnMovement pm = curObject.GetComponentInChildren<PawnMovement>();
            if (pm != null)
            {
                Debug.Log("Check for promotion");
                CheckForPromotion(curObject);
            }
        }

        curObject = null; // Clear current piece reference
    }

    /// <summary>
    /// Checks if the pawn reached a promotion rank, then calls PromotionUI for user choice.
    /// Adjust the rank logic for your board orientation.
    /// </summary>
    private void CheckForPromotion(GameObject pawnObject)
    {
        Vector2Int finalCoords = GetBoardCoordinates(pawnObject.transform.position);
        bool isWhite = pawnObject.CompareTag("White");

        // Example: White final rank = y == -6, Black final rank = y == 1
        if ((isWhite && finalCoords.y == -6) ||
            (!isWhite && finalCoords.y == 1))
        {
            // Instead of auto-promoting, show the UI
            Debug.Log("Pawn on promotion rank. Requesting Promotion UI...");
            PromotionUI.Instance.ShowPromotionPanel(pawnObject);
        }
    }

    /// <summary>
    /// Called by PromotionUI. Actually replaces the pawn with the chosen piece type.
    /// </summary>
    public void PerformPromotion(GameObject pawnObject, string pieceType)
    {
        Debug.Log($"Promoting pawn to {pieceType}!");

        Transform parentSquare = pawnObject.transform.parent;
        bool isWhite = pawnObject.CompareTag("White");

        // 1. Destroy old pawn
        Destroy(pawnObject);

        // 2. Choose correct prefab
        GameObject newPiecePrefab = GetPromotionPrefab(isWhite, pieceType);
        if (newPiecePrefab == null)
        {
            Debug.LogError($"No prefab found for {pieceType}");
            return;
        }

        // 3. Instantiate new piece
        GameObject newPiece = Instantiate(newPiecePrefab, parentSquare.position, Quaternion.identity);

        // 4. Parent to same square
        newPiece.transform.SetParent(parentSquare);

        // 5. Optionally fix scale & position
        newPiece.transform.localScale = newPiecePrefab.transform.localScale;
        float yOffset = 0.5f;
        Vector3 finalPos = new Vector3(
            parentSquare.position.x,
            parentSquare.position.y + yOffset,
            parentSquare.position.z
        );
        newPiece.transform.position = finalPos;

        Debug.Log($"Pawn promoted to {pieceType}!");
    }

    /// <summary>
    /// Returns the correct prefab for the chosen piece type (Queen/Rook/Bishop/Knight).
    /// </summary>
    private GameObject GetPromotionPrefab(bool isWhite, string pieceType)
    {
        switch (pieceType)
        {
            case "Rook":
                return isWhite ? whiteRookPrefab : blackRookPrefab;
            case "Bishop":
                return isWhite ? whiteBishopPrefab : blackBishopPrefab;
            case "Knight":
                return isWhite ? whiteKnightPrefab : blackKnightPrefab;
            default:
                // "Queen"
                return isWhite ? whiteQueenPrefab : blackQueenPrefab;
        }
    }

    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        float squareSize = 2.0f;
        Vector3 boardOrigin = boardTransform.position;

        int x = Mathf.RoundToInt((worldPosition.x - boardOrigin.x) / squareSize);
        int z = Mathf.RoundToInt((worldPosition.z - boardOrigin.z) / squareSize);

        return new Vector2Int(x, z);
    }

    void HighlightGrid(GameObject grid, Color color)
    {
        Renderer renderer = grid.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!originalColors.ContainsKey(grid))
            {
                originalColors[grid] = renderer.material.color;
            }
            renderer.material.color = color;
        }
    }

    void ResetGridHighlight()
    {
        if (curGrid != null)
        {
            Renderer renderer = curGrid.GetComponent<Renderer>();
            if (renderer != null && originalColors.ContainsKey(curGrid))
            {
                renderer.material.color = originalColors[curGrid];
            }
            curGrid = null;
        }
    }
}
