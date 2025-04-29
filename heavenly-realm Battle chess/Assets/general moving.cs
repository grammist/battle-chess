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

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool save = false;

    public GameObject capturedPiece;
    public GameObject uiPanel;
    private bool battle = false;

    private GameObject retrieve; 
    private GameObject inBattlePiece;

    private GameObject b1;
    private GameObject b2;

    [SerializeField] private GameObject whiteQueenPrefab;
    [SerializeField] private GameObject blackQueenPrefab;
    [SerializeField] private GameObject whiteRookPrefab;
    [SerializeField] private GameObject blackRookPrefab;
    [SerializeField] private GameObject whiteBishopPrefab;
    [SerializeField] private GameObject blackBishopPrefab;
    [SerializeField] private GameObject whiteKnightPrefab;
    [SerializeField] private GameObject blackKnightPrefab;


    void Start(){
        uiPanel.SetActive(false); 
        player = Camera.main.GetComponent<AudioSource>();
        //uiPanel = GameObject.Find("Panel");
    }


    // Update is called once per frame
     void Update()
    {
        if (!battle)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Transform hitTransform = hit.transform;

                if (hitTransform != null && Input.GetMouseButtonDown(0) && allowClick && curObject != null && hitTransform.childCount < 2)
                {
                    curGrid = hitTransform.gameObject;
                    lastTargetParent = hitTransform;
                    
                    if (hitTransform.childCount == 0)
                    {
                        if (IsValidMove(curObject, hitTransform.gameObject))
                        {
                            allowClick = false;
                            Move(curObject, hitTransform.gameObject);
                        }
                        else
                        {
                            //Debug.Log("Invalid move!");
                            allowClick = true;
                            curGrid = null;
                        }
                    }
                    else if (hitTransform.GetChild(0).tag != curObject.tag)
                    {
                        if (IsValidMove(curObject, hitTransform.gameObject))
                        {
                            allowClick = false;
                            inBattlePiece = curObject;
                            retrieve = inBattlePiece.transform.parent.gameObject;
                            Move(curObject, hitTransform.gameObject);
                        }
                        else
                        {
                            allowClick = true;
                            curGrid = null;
                        }
                    }
                }
            }
        }
    }



    public static bool IsValidMove(GameObject chessObject, GameObject targetSquare)
    {
        //PawnMovement pawnMovement = chessObject.GetComponent<PawnMovement>();
        PawnMovement pawnMovement = chessObject.GetComponentInChildren<PawnMovement>();
        if (pawnMovement != null)
        {
            //Debug.Log("Pawn movement");
            return pawnMovement.IsValidMove(targetSquare);
        }

        // Add other piece movement validations here
        KnightMovement knightMovement = chessObject.GetComponent<KnightMovement>();
        if (knightMovement != null)
        {
            //Debug.Log("Knight movement");
            return knightMovement.IsValidMove(targetSquare);
        }

        RookMovement rookMovement = chessObject.GetComponent<RookMovement>();
        if (rookMovement != null)
        {
            //Debug.Log("Rook movement");
            return rookMovement.IsValidMove(targetSquare); 
        }

        BishopMovement bishopMovement = chessObject.GetComponent <BishopMovement>();
        if (bishopMovement != null)
        {
            //Debug.Log("Bishop movement");
            return bishopMovement.IsValidMove(targetSquare);
        }

        KingMovement kingMovement = chessObject.GetComponent<KingMovement>();
        if (kingMovement != null)
        {
            //Debug.Log("King Movement");
            return kingMovement.IsValidMove(targetSquare);
        }

        QueenMovement queenMovement = chessObject.GetComponent<QueenMovement>();
        if (queenMovement != null)
        {
            //Debug.Log("Queen movement");
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
        //ResetGridHighlight();
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
            return;
        }


        float originalY = chessObject.transform.position.y;
        Vector3 targetPosition = new Vector3(targetSquareObject.transform.position.x, originalY, targetSquareObject.transform.position.z);
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
            battle = true;
            b1 = lastTargetParent.GetChild(0).gameObject;
            b2 = lastTargetParent.GetChild(1).gameObject;
            Transform child = lastTargetParent.GetChild(0);
            capturedPiece = child.gameObject;
            Transform cameraTransform = Camera.main.transform;
            SaveCameraTransform(cameraTransform);
            StartCoroutine(SmoothTransition(OnCameraTransitionCompleted, curObject.transform.position + new Vector3(0, 0.1f, 2), Quaternion.Euler(-30, 180, 0), 100));
        }

        GameManager.NextState();

        if (curObject != null)
        {
            PawnMovement pm = curObject.GetComponentInChildren<PawnMovement>();
            if (pm != null)
            {
                // Debug.Log("Check for Pawn");
                Vector2Int finalCoords = GetBoardCoordinates(curObject.transform.position);
                // Debug.Log("Pawn finalCoords: " + finalCoords);
                if (finalCoords.y == -8 || finalCoords.y == -1)
                {
                    Debug.Log("Check for promotion");
                    CheckForPromotion(curObject);
                }
            }

            curObject.GetComponent<HoverChangeColor>().unClick();
        }

        curObject = null;
    }

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
                return isWhite ? whiteQueenPrefab : blackQueenPrefab;
        }
    }

    private Vector2Int GetBoardCoordinates(Vector3 worldPosition)
    {
        
        int x = Mathf.RoundToInt((12 - worldPosition.x) / 2f);

        
        int y = Mathf.RoundToInt((worldPosition.z - 2) / 2f);

        return new Vector2Int(x, y);
    }



    private void CheckForPromotion(GameObject pawnObject)
    {
        Vector2Int finalCoords = GetBoardCoordinates(pawnObject.transform.position);
        bool isWhite = pawnObject.CompareTag("White");

        // Example: White final rank = y == -6, Black final rank = y == 1
        if ((isWhite && finalCoords.y == -8) ||
            (!isWhite && finalCoords.y == -1))
        {
            Debug.Log("Pawn on promotion rank. Requesting Promotion UI...");
            PromotionUI.Instance.ShowPromotionPanel(pawnObject);
        }
    }

    public void PerformPromotion(GameObject pawnObject, string pieceType)
    {
        Debug.Log($"Promoting pawn to {pieceType}!");

        Transform parentSquare = pawnObject.transform.parent;
        bool isWhite = pawnObject.CompareTag("White");

        Destroy(pawnObject);

        GameObject newPiecePrefab = GetPromotionPrefab(isWhite, pieceType);
        if (newPiecePrefab == null)
        {
            Debug.LogError($"No prefab found for {pieceType}");
            return;
        }

        GameObject newPiece = Instantiate(newPiecePrefab, parentSquare.position, Quaternion.identity);
        newPiece.transform.SetParent(parentSquare);

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
        /*if (curGrid != null)
        {
            Renderer renderer = curGrid.GetComponent<Renderer>();
            if (renderer != null && originalColors.ContainsKey(curGrid))
            {
                if (renderer.material.color != Color.blue){
                    // Reset to the original color
                    renderer.material.color = originalColors[curGrid];
                    Debug.Log(0);
                } else {
                    renderer.material.color = Color.cyan;
                    Debug.Log(1);
                }                
            }

            curGrid = null; // Clear the current grid
        }*/
    }

public void SaveCameraTransform(Transform cameraTransform)
    {
        save = true;
        savedPosition = cameraTransform.position;
        savedRotation = cameraTransform.rotation;
    }

    public void RestoreCameraTransform()
    {
        save = false;
    }

    private IEnumerator SmoothTransition(Action callback, Vector3 targetPosition, Quaternion targetRotation, float fov)
    {
        float transitionTime = 1.5f;
        // Store the initial values of the camera
        Vector3 initialPosition = Camera.main.transform.position;
        Quaternion initialRotation = Camera.main.transform.rotation;
        float initialFOV = Camera.main.fieldOfView;

        // Timer to keep track of the transition progress
        float timeElapsed = 0f;

        // Smoothly transition over time
        while (timeElapsed < transitionTime)
        {
            timeElapsed += Time.deltaTime;

            // Interpolate the position, rotation, and FOV of the camera
            Camera.main.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeElapsed / transitionTime);
            Camera.main.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, timeElapsed / transitionTime);
            Camera.main.fieldOfView = Mathf.Lerp(initialFOV, fov, timeElapsed / transitionTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the final values are set (in case transition time is over)
        Camera.main.transform.position = targetPosition;
        Camera.main.transform.rotation = targetRotation;

        // Invoke the callback function after the camera transition is complete
        callback?.Invoke();
    }

    private void OnCameraTransitionCompleted()
    {
        uiPanel.SetActive(true);       
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ShowObjectImage(b2, b1);
        }
    }

    private void OnCameraTransitionCompleted0()
    {
        
    }

    public void OnClick(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        Destroy(capturedPiece);
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    public void OnClick0(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        //Debug.Log(curObject.name);
        Destroy(inBattlePiece);
        inBattlePiece = null;
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    public void OnClick1(){
        Assign_Char assignChar = FindObjectOfType<Assign_Char>();
        if (assignChar != null)
        {
            assignChar.ClearUIImage();
        }
        uiPanel.SetActive(false); 
        battle = false;
        Move(inBattlePiece, retrieve);
        GameManager.NextState();
        StartCoroutine(SmoothTransition(OnCameraTransitionCompleted0, savedPosition, savedRotation, 60));
    }

    


}
