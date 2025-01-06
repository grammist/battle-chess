using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of movement

    // Update is called once per frame
    void Update()
    {
    float horizontal = -Input.GetAxis("Horizontal"); // A/D keys
    float vertical = -Input.GetAxis("Vertical");     // W/S keys

    // Move in WORLD space instead of LOCAL space
    if(transform.position.x <= 1 && transform.position.x >= -14 && transform.position.z <= 5 && transform.position.z >= -10 ){
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    if(transform.position.x > 1){
        Vector3 newPosition = transform.position; // Get current position
        newPosition.x = 1;                      // Modify only the X value
        transform.position = newPosition; 
    }
    if(transform.position.z > 5){
        Vector3 newPosition = transform.position; // Get current position
        newPosition.z = 5;                      // Modify only the X value
        transform.position = newPosition; 
    }
    if(transform.position.x < -14){
        Vector3 newPosition = transform.position; // Get current position
        newPosition.x = -14;                      // Modify only the X value
        transform.position = newPosition; 
    }
    if(transform.position.z < -10){
        Vector3 newPosition = transform.position; // Get current position
        newPosition.z = -10;                      // Modify only the X value
        transform.position = newPosition; 
    }
    }
}
