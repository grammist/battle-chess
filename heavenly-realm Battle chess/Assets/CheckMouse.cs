using UnityEngine;

public class HoverChangeColor : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    private Renderer cubeobjectRenderer;
    private Color cubeoriginalColor;
    public Color hoverColor = Color.yellow; // Color when hovered
    public Color clickColor = Color.red;   // Color when left-clicked
    public static string Selected;

    public bool isClicked = false; // Bool to track click state
    public bool isHovered = false;
    public bool isWhite;

    private GameObject cube;

    


    // Delegate and Event
    public delegate void ObjectClicked(GameObject clickedObject);
    public static event ObjectClicked OnObjectClicked;

    // Delegate and Event
    public delegate void ObjectUnClicked(GameObject unclickedObject);
    //public static event ObjectUnClicked OnObjectUnClicked;

    [SerializeField] public AudioClip seC;
    private AudioSource player;

    void Start()
    {
        // Get the Renderer component and save the original color
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
        player = Camera.main.GetComponent<AudioSource>();
        cube = GameObject.Find("Cube");
        cubeobjectRenderer = cube.GetComponent<Renderer>();
        cubeoriginalColor = cubeobjectRenderer.material.color;
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
                if (Input.GetMouseButtonDown(0) && checkTurnValid(hit.transform)) // Left-click
                {
                    player.PlayOneShot(seC);
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
            //cube.transform.position = transform.position + Vector3.up * 3.0f;
        }
        else if (isHovered)
        {
            
            objectRenderer.material.color = hoverColor;
            //cube.transform.position = transform.position + Vector3.up * 3.0f;

        }
        else
        {
            
            objectRenderer.material.color = originalColor;
        }
        RaycastHit hita;
        /*if (Physics.Raycast(cube.transform.position, Vector3.down, out hita, Mathf.Infinity))
        {
            Renderer pieceRenderer = hita.collider.GetComponent<Renderer>();
            if (pieceRenderer != null)
            {
                cubeobjectRenderer.material.color = pieceRenderer.material.color;
            }
        }*/
        
    }

    public void unClick(){
        isClicked = false;
        objectRenderer.material.color = originalColor;
    }

    public bool getColor(){return isWhite;}

    public bool checkTurnValid(Transform t){
        return t.gameObject.CompareTag("White") && (GameManager.currentTurn == GameManager.TurnState.white) ||
        t.gameObject.CompareTag("Black") && (GameManager.currentTurn == GameManager.TurnState.black);
    }

    public bool checkisClicked(){
        return isClicked;
    }

    public bool checkisHovered(){
        return isHovered;
    }

}
