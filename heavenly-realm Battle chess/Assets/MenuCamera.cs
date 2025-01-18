using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCamera : MonoBehaviour
{
    // Time in seconds for the transition
    public float transitionTime = 3f;

    // Target position, rotation, and FOV for the camera
    private Vector3 targetPosition = new Vector3(-7f, 11f, 1f);
    private Quaternion targetRotation = Quaternion.Euler(60f, 180f, 0f);
    private float targetFOV = 60f;

    // Time in seconds for the light transition
    public float transitionTimel = 3f;

    // Target position and color for the light
    private Vector3 targetPositionl = new Vector3(-6.5f, 3f, -6.5f);
    private Color targetColor = new Color(1f, 0.988f, 0.757f); // RGB for #FFEEC1

    // Reference to the Directional Light
    private Light directionalLight;

    public GameObject btn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Coroutine to smoothly change the camera properties
    public void StartCameraTransition(Action callback)
    {
        StartCoroutine(SmoothTransition(callback));
    }

    private IEnumerator SmoothTransition(Action callback)
    {
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
            Camera.main.fieldOfView = Mathf.Lerp(initialFOV, targetFOV, timeElapsed / transitionTime);

            yield return null; // Wait for the next frame
        }

        // Ensure the final values are set (in case transition time is over)
        Camera.main.transform.position = targetPosition;
        Camera.main.transform.rotation = targetRotation;
        Camera.main.fieldOfView = targetFOV;

        // Invoke the callback function after the camera transition is complete
        callback?.Invoke();
    }

    // The callback function that will be called after the camera transition
    private void OnCameraTransitionCompleted()
    {
        // Start the light movement after the camera has finished moving
        StartCoroutine(MoveLightSmoothly(() =>
        {
            // Load the SampleScene after the light movement is complete
            SceneManager.LoadScene("SampleScene");
        }));
        Debug.Log(0);
    }

    private IEnumerator MoveLightSmoothly(Action callback)
    {
        // Store the initial values of the light
        Vector3 initialPosition = directionalLight.transform.position;
        Color initialColor = directionalLight.color;

        float timeElapsedl = 0f;

        // Smoothly transition over time
        while (timeElapsedl < transitionTimel)
        {
            timeElapsedl += Time.deltaTime;

            // Interpolate the position and color of the light
            directionalLight.transform.position = Vector3.Lerp(initialPosition, targetPositionl, timeElapsedl / transitionTimel);
            directionalLight.color = Color.Lerp(initialColor, targetColor, timeElapsedl / transitionTimel);

            yield return null; // Wait for the next frame
        }

        // Ensure the final values are set (in case transition time is over)
        directionalLight.transform.position = targetPositionl;
        directionalLight.color = targetColor;

        // Invoke the callback function after the light transition is complete
        callback?.Invoke();
    }
    public void OnClick(){
        Destroy(btn);
    // Get the Directional Light component
        directionalLight = GetComponent<Light>();

        // Start the camera transition and pass the light movement as the callback
        StartCameraTransition(OnCameraTransitionCompleted);
    }
}

