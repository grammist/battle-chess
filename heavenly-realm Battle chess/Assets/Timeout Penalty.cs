using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeoutPenalty : MonoBehaviour
{
    public static float timeElapsed = 0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= 3f) {
            GameManager.NextState();
        }
    }
}
