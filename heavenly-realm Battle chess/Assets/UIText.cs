using TMPro; // For TextMeshPro
using UnityEngine;
using System;

public class UIText : MonoBehaviour
{
    public TextMeshProUGUI textUI; // Reference to the TextMeshProUGUI component
    public float myVariable;    // Example variable to display

    void Start()
    {
        textUI.fontSize = 18;
    }

    void Update()
    {
        myVariable = (float)Math.Round((300f - TimeoutPenalty.timeElapsed), 2);
        // Update the text to show the variable value
        if (textUI != null)
        {
            textUI.text = "Time remaining: " + myVariable;
        }
    }
}
