using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHighlight : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public GameObject curObject;
    public Color highlightColor = Color.cyan;
    public Color whiteColor = Color.white;
    public Color blackColor = Color.black;
    private Color curColor;

    private Vector3 previousPosition; // To track the previous position of curObject

    //private List<GameObject> whitePieces = new List<GameObject>();
    //private List<GameObject> blackPieces = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {

        //InitializeWhitePieces();
        //InitializeBlackPieces();

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        else
        {
            Debug.LogError("Renderer not found on this GameObject.");
        }

        // Initialize the previous position
        if (curObject != null)
        {
            previousPosition = curObject.transform.position;
        }
    }

    private void OnEnable()
    {
        HoverChangeColor.OnObjectClicked += HandleObjectClicked;
    }

    private void OnDisable()
    {
        HoverChangeColor.OnObjectClicked -= HandleObjectClicked;
    }

    private void HandleObjectClicked(GameObject clickedObject)
    {
        curObject = clickedObject;

        // Update the previous position when the object changes
        if (curObject != null)
        {
            previousPosition = curObject.transform.position;
        }

        UpdateObjectHighlight();
    }

    private void Update()
    {
        // Check if curObject has moved
        if (curObject != null && curObject.transform.position != previousPosition)
        {
            // Reset color and update the previous position
            ResetColor();
            colorChange();
            previousPosition = curObject.transform.position;
        }
    }

    private void UpdateObjectHighlight()
    {
        if (curObject != null && generalmoving.IsValidMove(curObject, this.gameObject))
        {
            curColor = highlightColor;
            colorChange();
        }
        else
        {
            curColor = originalColor;
            colorChange();
        }
    }

    // Public method to reset the color to the original
    public void ResetColor()
    {
        if (objectRenderer != null)
        {
            curColor = originalColor;
        }
    }

    /*private void InitializeWhitePieces()
    {
        whitePieces.Add(GameObject.Find("Pawn1"));
        whitePieces.Add(GameObject.Find("Pawn2"));
        whitePieces.Add(GameObject.Find("Pawn3"));
        whitePieces.Add(GameObject.Find("Pawn4"));
        whitePieces.Add(GameObject.Find("Pawn5"));
        whitePieces.Add(GameObject.Find("Pawn6"));
        whitePieces.Add(GameObject.Find("Pawn7"));
        whitePieces.Add(GameObject.Find("Pawn8"));

        whitePieces.Add(GameObject.Find("Rook"));
        whitePieces.Add(GameObject.Find("Rook1"));

        whitePieces.Add(GameObject.Find("Knight"));
        whitePieces.Add(GameObject.Find("Knight1"));

        whitePieces.Add(GameObject.Find("Bishop"));
        whitePieces.Add(GameObject.Find("Bishop1"));

        whitePieces.Add(GameObject.Find("Queen"));
        whitePieces.Add(GameObject.Find("King"));
    }

    private void InitializeBlackPieces()
    {
        blackPieces.Add(GameObject.Find("Pawn01"));
        blackPieces.Add(GameObject.Find("Pawn02"));
        blackPieces.Add(GameObject.Find("Pawn03"));
        blackPieces.Add(GameObject.Find("Pawn04"));
        blackPieces.Add(GameObject.Find("Pawn05"));
        blackPieces.Add(GameObject.Find("Pawn06"));
        blackPieces.Add(GameObject.Find("Pawn07"));
        blackPieces.Add(GameObject.Find("Pawn08"));

        blackPieces.Add(GameObject.Find("Rook0"));
        blackPieces.Add(GameObject.Find("Rook01"));

        blackPieces.Add(GameObject.Find("Knight0"));
        blackPieces.Add(GameObject.Find("Knight01"));

        blackPieces.Add(GameObject.Find("Bishop0"));
        blackPieces.Add(GameObject.Find("Bishop01"));

        blackPieces.Add(GameObject.Find("Queen0"));
        blackPieces.Add(GameObject.Find("King0"));
    }

    public void whiteControlZone(){
        foreach (GameObject i in whitePieces)
        {
            if(generalmoving.IsValidMove(i,this.gameObject)) {
                objectRenderer.material.color = whiteColor;
            }else{
                colorChange();
            }
        }
    }*/

    public void colorChange(){
        objectRenderer.material.color = curColor;
    }
    
}
