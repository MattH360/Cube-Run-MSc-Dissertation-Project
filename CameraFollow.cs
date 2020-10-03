using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    /*This script ensures the camera stays within the bounds of the level and smoothly follows the character so the player can see incoming obstructions both to the left and right while moving.
     */
    [SerializeField]
    Transform playerTransform;
    Vector3 cameraPosition;
    Vector3 playerPosition;
    float minWidth = -188.0f;
    float maxWidth = -12.0f;
    float minHeight = -10.0f;
    float maxHeight = 5.0f;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // LateUpdate ensures the character has finished moving for that frame before repositioning the camera (called after Update or FixedUpdate).
    void LateUpdate()
    {
        /*Set camera and player position vectors, set camera position to player position in x axis, 
         *clamp camera position so no area outside level is visible, update camera position with new values.
         */
        cameraPosition = transform.position;
        playerPosition = playerTransform.position;
        cameraPosition.x = playerPosition.x;
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minWidth, maxWidth);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minHeight, maxHeight);
        transform.position = cameraPosition; 
    }
    
   
}
