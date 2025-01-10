using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generalmoving : MonoBehaviour
{

    public float moveSpeed = 2.0f;

    private bool allowClick = true;

    public GameObject chess;
    public GameObject targetSquare;
    public GameObject curObject;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Cast a ray from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the object hit by the ray is a direct child of Game board
            Transform hitTransform = hit.transform;
            if (hitTransform != null && hitTransform.parent == this.transform)
            {
                if (Input.GetMouseButtonDown(0) && allowClick && curObject != null && hitTransform.childCount < 2) // Left-click
                {
                    if (hitTransform.childCount == 0){
                        allowClick = false;
                        Move(curObject, hitTransform.gameObject); 
                    } else if(hitTransform.GetChild(0).tag != curObject.tag){
                        allowClick = false;
                        Move(curObject, hitTransform.gameObject); 
                    }
                    
                }
            }
        }
    }

    private void OnEnable()
    {
        HoverChangeColor.OnObjectClicked += HandleObjectClicked;
        HoverChangeColor.OnObjectUnClicked += HandleObjectUnClicked;
    }

    private void OnDisable()
    {
        HoverChangeColor.OnObjectClicked -= HandleObjectClicked;
        HoverChangeColor.OnObjectUnClicked += HandleObjectUnClicked;
    }

    private void HandleObjectClicked(GameObject clickedObject)
    {
        curObject = clickedObject;
    }

    private void HandleObjectUnClicked(GameObject clickedObject)
    {
        curObject = null;
    }

    void Move (GameObject chessObject, GameObject targetSquareObject)
    {
       if (chessObject == null || targetSquareObject == null)
        {
            Debug.LogError("Chess Object or Target Square Object is null!");
            return;
        }

        chessObject.transform.SetParent(null);

        Vector3 targetPosition = targetSquareObject.transform.position + new Vector3(0, 1.25f, 0);

        StartCoroutine(MoveToTarget(chessObject, targetSquareObject.transform, targetPosition, 2.0f));
    }

    IEnumerator MoveToTarget(GameObject chessObject, Transform targetParent, Vector3 targetPosition, float moveSpeed)
    {
    // Smoothly move the object towards the target position
        while (Vector3.Distance(chessObject.transform.position, targetPosition) > 0.01f)
        {
            chessObject.transform.position = Vector3.MoveTowards(chessObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

    // Once at the target, set the new parent to the target square
        chessObject.transform.SetParent(targetParent);

        Debug.Log($"{chessObject.name} has been successfully moved to {targetParent.name}");
        allowClick = true;
    }
}
