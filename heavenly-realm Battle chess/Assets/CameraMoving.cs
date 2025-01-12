using System.Collections;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of movement
    public float horizontal;
    public float vertical;

    // Camera positions and rotations
    private Vector3 whitePosition = new Vector3(-7, 11, -15);
    private Quaternion whiteRotation = Quaternion.Euler(60, 0, 0);
    private Vector3 blackPosition;
    private Quaternion blackRotation;

    private bool isWhite = true; // Toggle state
    public float transitionDuration = 1.5f; // Smooth transition duration

    void Start()
    {
        // Record the initial position and rotation as black
        blackPosition = transform.position;
        blackRotation = transform.rotation;
    }

    void Update()
    {
        // Handle movement
        HandleMovement();

        // Handle space key for toggling
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWhite)
            {
                StartCoroutine(SmoothMove(blackPosition, blackRotation));
                isWhite = false;
            }
            else
            {
                StartCoroutine(SmoothMove(whitePosition, whiteRotation));
                isWhite = true;
            }
        }
    }

    private void HandleMovement()
    {
        // Get input for movement
        horizontal = Input.GetAxis("Horizontal"); // A/D keys
        vertical = Input.GetAxis("Vertical");     // W/S keys

        // Adjust movement direction based on the camera's current rotation
        Vector3 forward = transform.forward; // Camera's forward direction
        Vector3 right = transform.right;     // Camera's right direction

        // Project forward and right onto the horizontal plane (Y = 0)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction in world space
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;

        // Check boundaries and apply movement
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -24, 10); // X boundaries
        newPosition.z = Mathf.Clamp(newPosition.z, -20, 15); // Z boundaries

        transform.position = newPosition;
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Smoothly interpolate position and rotation
            transform.position = Vector3.Slerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Ensure exact position and rotation at the end
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }
}
