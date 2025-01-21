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

    void Start(){
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

            if (hitTransform != null /*&& hitTransform.parent == this.transform*/)
            {
                if (Input.GetMouseButtonDown(0) && allowClick && curObject != null && hitTransform.childCount < 2)
                {
                    if (curGrid == null) // First click
                    {
                        curGrid = hitTransform.gameObject;
                        HighlightGrid(curGrid, Color.blue); // Turn the grid blue
                    }
                    else if (curGrid == hitTransform.gameObject) // Second click to confirm
                    {
                        // Confirm the move
                        if (hitTransform.childCount == 0)
                        {
                            Debug.Log("Child count = 0");
                           // allowClick = false;
                            lastTargetParent = hitTransform; // Store the parent for later use
                           // Move(curObject, hitTransform.gameObject);
                            if (IsValidMove(curObject, hitTransform.gameObject))
                            {

                                allowClick = false;

                                Move(curObject, hitTransform.gameObject);
                            } else
                            {
                                // Invalid move: let the player try again
                                Debug.Log("Invalid move!");
                                allowClick = true;            // Re-enable clicks
                                ResetGridHighlight();        // Remove any highlight
                                curGrid = null;              // Clear the selection
                                //ResetGridHighlight();        // Remove any highlight
                            }
                        }
                        else if (hitTransform.GetChild(0).tag != curObject.tag)
                        {
                            Debug.Log("Tag doesn't equal to current object");
                            allowClick = false;
                            lastTargetParent = hitTransform; // Store the parent for later use
                            //Move(curObject, hitTransform.gameObject);
                            if (IsValidMove(curObject, hitTransform.gameObject))
                            {
                                allowClick = false;

                                Move(curObject, hitTransform.gameObject);
                            } else
                            {
                                // Invalid move: let the player try again
                                Debug.Log("Invalid move!");
                                allowClick = true;            // Re-enable clicks
                                ResetGridHighlight();        // Remove any highlight
                                curGrid = null;              // Clear the selection
                                //ResetGridHighlight();        // Remove any highlight
                            }
                        }

                        ResetGridHighlight(); // Reset grid highlight after confirmation
                    }
                    else // Clicking a different grid cancels the selection
                    {
                        ResetGridHighlight();
                        curGrid = hitTransform.gameObject;
                        HighlightGrid(curGrid, Color.blue); // Highlight the new grid
                    }
                }
            }
        }
    }

    private bool IsValidMove(GameObject chessObject, GameObject targetSquare)
    {
        Debug.Log("IsValidMove");
        Debug.Log($"Clicked object is {curObject.name}");
        //PawnMovement pawnMovement = chessObject.GetComponent<PawnMovement>();
        PawnMovement pawnMovement = chessObject.GetComponentInChildren<PawnMovement>();
        Debug.Log($"Pawnmovement: {pawnMovement}");
        if (pawnMovement != null)
        {
            Debug.Log("Pawn movement");
            return pawnMovement.IsValidMove(targetSquare);
        }

        // Add other piece movement validations here
        KnightMovement knightMovement = chessObject.GetComponent<KnightMovement>();
        if (knightMovement != null)
        {
            Debug.Log("Knight movement");
            return knightMovement.IsValidMove(targetSquare);
        }

        RookMovement rookMovement = chessObject.GetComponent<RookMovement>();
        if (rookMovement != null)
        {
            Debug.Log("Rook movement");
            return rookMovement.IsValidMove(targetSquare); 
        }

        BishopMovement bishopMovement = chessObject.GetComponent <BishopMovement>();
        if (bishopMovement != null)
        {
            Debug.Log("Bishop movement");
            return bishopMovement.IsValidMove(targetSquare);
        }

        KingMovement kingMovement = chessObject.GetComponent<KingMovement>();
        if (kingMovement != null)
        {
            Debug.Log("King Movement");
            return kingMovement.IsValidMove(targetSquare);
        }

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
        //HoverChangeColor.OnObjectUnClicked += HandleObjectUnClicked;
        TimeoutPenalty.OnTimeOut += HandleTimeOut;
    }

    private void OnDisable()
    {
        HoverChangeColor.OnObjectClicked -= HandleObjectClicked;
        //HoverChangeColor.OnObjectUnClicked -= HandleObjectUnClicked;
        TimeoutPenalty.OnTimeOut -= HandleTimeOut;
    }

    private void HandleObjectClicked(GameObject clickedObject)
    {
        curObject = clickedObject;
    }
    private void HandleTimeOut()
    {
        if(curObject != null) {
            curObject.GetComponent<HoverChangeColor>().unClick();
        }
        curObject = null;
        ResetGridHighlight();
        curGrid = null;
    }

    private void HandleObjectUnClicked(GameObject clickedObject)
    {
        curObject = null;
    }

    void Move(GameObject chessObject, GameObject targetSquareObject)
    {
        if (chessObject == null || targetSquareObject == null)
        {
            Debug.LogError("Chess Object or Target Square Object is null!");
            return;
        }

        // Get the original Y position of the chess object
        float originalY = chessObject.transform.position.y;

        // Set the target position to the target square's position, maintaining the object's original Y position
        Vector3 targetPosition = new Vector3(targetSquareObject.transform.position.x, originalY, targetSquareObject.transform.position.z);

        // Start the movement coroutine to move the object only on the X and Z axes
        StartCoroutine(MoveToTarget(chessObject, targetSquareObject.transform, targetPosition, moveSpeed, OnMoveCompleted));
    }

    public IEnumerator MoveToTarget(GameObject chessObject, Transform targetParent, Vector3 targetPosition, float moveSpeed, Action onMoveComplete)
    {
        while (Vector3.Distance(chessObject.transform.position, targetPosition) > 0.01f)
        {
            chessObject.transform.position = Vector3.MoveTowards(chessObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        chessObject.transform.SetParent(targetParent);

        allowClick = true;

        onMoveComplete?.Invoke();
    }

    private void OnMoveCompleted()
    {
        player.PlayOneShot(seM);
        if (lastTargetParent != null && lastTargetParent.childCount == 2)
        {
            Transform child = lastTargetParent.GetChild(0);
            Destroy(child.gameObject);
        }
        GameManager.NextState();
        if(curObject != null) {
            curObject.GetComponent<HoverChangeColor>().unClick();
        }
        curObject = null;
    }

void HighlightGrid(GameObject grid, Color color)
{
    Renderer renderer = grid.GetComponent<Renderer>();
    if (renderer != null)
    {
        // Store the original color if it's not already stored
        if (!originalColors.ContainsKey(grid))
        {
            originalColors[grid] = renderer.material.color;
        }

        // Change the color of the grid
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
            // Reset to the original color
            renderer.material.color = originalColors[curGrid];
        }

        curGrid = null; // Clear the current grid
    }
}




}
