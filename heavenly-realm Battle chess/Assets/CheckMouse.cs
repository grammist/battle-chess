using UnityEngine;

public class HoverChangeColor : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public Color hoverColor = Color.yellow; // Color when hovered
    public Color clickColor = Color.red;   // Color when left-clicked
    public static string Selected;

    public bool isClicked = false; // Bool to track click state
    public bool isHovered = false;
    public bool isWhite;

    // Delegate and Event
    public delegate void ObjectClicked(GameObject clickedObject);
    public static event ObjectClicked OnObjectClicked;

    // Delegate and Event
    public delegate void ObjectUnClicked(GameObject unclickedObject);
    //public static event ObjectUnClicked OnObjectUnClicked;


    void Start()
    {
        // Get the Renderer component and save the original color
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    void Update()
    {
        if(this.transform.name != Selected) {
            isClicked = false;
        }
        // Cast a ray from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the object hit by the ray is this object
            if (hit.transform == transform)
            {
                isHovered = true;

                // Set bool true and change color on left-click
                if (Input.GetMouseButtonDown(0) && ((hit.transform.gameObject.CompareTag("White") && GameManager.currentTurn == GameManager.TurnState.white) || (hit.transform.gameObject.CompareTag("Black") && GameManager.currentTurn == GameManager.TurnState.black))) // Left-click
                {
                    isClicked = true;
                    Selected = hit.transform.name;
                    // Trigger the event and send this GameObject
                    OnObjectClicked?.Invoke(gameObject);
                }
                // Set bool false and revert color on right-click
                else if (Input.GetMouseButtonDown(1)) // Right-click
                {
                    isClicked = false;
                    OnObjectClicked?.Invoke(null);
                }
            }
            else
            {
                isHovered = false;
            }
        }

        // Update colors based on state
        if (isClicked)
        {
            objectRenderer.material.color = clickColor;
        }
        else if (isHovered)
        {
            objectRenderer.material.color = hoverColor;
        }
        else
        {
            objectRenderer.material.color = originalColor;
        }
    }
    public void unClick(){
        isClicked = false;
        objectRenderer.material.color = originalColor;
    }

    public bool getColor(){return isWhite;}
}
