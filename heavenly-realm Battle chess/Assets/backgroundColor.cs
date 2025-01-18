using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColor : MonoBehaviour
{
    public Color whiteColor = Color.white;
    public Color blackColor = Color.black;

    private Renderer objectRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // Find the Renderer of the child object named "default"
        Transform childTransform = transform.Find("default");
        if (childTransform != null)
        {
            objectRenderer = childTransform.GetComponent<Renderer>();
        }

        // Log an error if the child or Renderer is not found
        if (objectRenderer == null)
        {
            Debug.LogError("No Renderer found on the child object 'default'. Ensure it has a Mesh Renderer component.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (objectRenderer != null && GameManager.currentTurn != null)
        {
            if (GameManager.currentTurn == GameManager.TurnState.white)
            {
                objectRenderer.material.color = whiteColor;
            }
            else
            {
                objectRenderer.material.color = blackColor;
            }
        }
    }
}
