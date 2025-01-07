using UnityEngine;

public class HoverChangeColor : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public Color hoverColor = Color.yellow; // Color when hovered
    public Color clickColor = Color.red;   // Color when left-clicked

    public bool isClicked = false; // Bool to track click state
    public bool isHovered = false;
    

    void Start()
    {
        // Get the Renderer component and save the original color
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    void Update()
    {
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
                if (Input.GetMouseButtonDown(0)) // Left-click
                {
                    isClicked = true;
                }
                // Set bool false and revert color on right-click
                else if (Input.GetMouseButtonDown(1)) // Right-click
                {
                    isClicked = false;
                }
            }
            else
            {
                isHovered = false;
            }
        }
        if(isClicked){
            objectRenderer.material.color = clickColor;
        }else if(isHovered){
            objectRenderer.material.color = hoverColor;
        }else{
            objectRenderer.material.color = originalColor;
        }

    }
    public bool getIsClicked(){return isClicked;}
}
