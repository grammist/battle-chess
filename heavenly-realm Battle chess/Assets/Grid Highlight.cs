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
        generalmoving gm = FindObjectOfType<generalmoving>();
        if (curObject != null && gm.IsValidMove(curObject, this.gameObject))
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

    

    public void colorChange(){
        objectRenderer.material.color = curColor;
    }
    
}
