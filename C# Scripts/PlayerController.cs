using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    /* This script deals with general player character movement and the restarting of the level. Other scripts also access the RestartLevel method from this script.
     */
 
    float charSpeed = 2.0f;
    float jumpHeight = 6.5f;
    int layerMask; 
    bool onGround;
    Vector3 playerPosition;
    Rigidbody2D playerBody;
    BoxCollider2D playerBCollider;
    float checkerRadius = 0.1f;
    Vector3 checkerOffset;
    public float outOfLevel = -12.0f; 
    GameObject timer;
    GameObject playerCharacter;
    GameObject levelGen;
    IEnumerator recalPath;
    
    void Start()
    {
        //Prevent character rotating, obtain game objects and variables needed while the script is running and initialise an offset position for the CheckGround method collider.
        playerBody = GetComponent<Rigidbody2D>();
        playerBody.freezeRotation = true;
        playerBCollider = GetComponent<BoxCollider2D>();
        checkerOffset = new Vector3(0.0f, -0.3f, 0.0f);
        timer = GameObject.Find("LevelCanvas");
        playerCharacter = GameObject.Find("PlayerCharacter");
        levelGen = GameObject.Find("LevelGenerator");
    }
    
    void HorizontalMovement()
    {
        //Simplistic character movement both left and right by changing player velocity in the positive and negative x-axis directions.
        if (Input.GetKey(KeyCode.RightArrow))
        {
            playerBody.velocity = new Vector2(charSpeed, playerBody.velocity.y);
            
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerBody.velocity = new Vector2(-charSpeed, playerBody.velocity.y);
        }
        
    }

    void JumpMovement()
    {
        //When the player is detected touching a platform, jump is enabled and the player can jump using the spacebar.
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            playerBody.velocity = Vector2.up * jumpHeight;

        }
    }

    void CheckGround()
    {
        //Checks if the player is touching the ground using a collider circle that detects any colliders under the player.
        //The collider circle checks every collider on all Unity editor layers except the layer for the character to prevent detection of the character's own collider.
        playerPosition = transform.position;
        layerMask = 1 << 10;
        layerMask = ~layerMask;
        onGround = Physics2D.OverlapCircle(playerPosition + checkerOffset, checkerRadius, layerMask);
        
    }

    void OnDrawGizmos()
    {
        //Visual representation of ground checker visible in the editor scene
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerPosition + checkerOffset, checkerRadius);

    }


    public void RestartLevel()
    {
        /*Loads a new scene and removes the current LevelCanvas object (containing the timer) so that the timer restarts with the new level.
         * If the testMode script is enabled running coroutines are disabled and started again here (so the 'T' key does not need to be pressed again).
         */
        if (GetComponent<TestMode>().enabled == true)
        {
            GetComponent<AIPathfinding>().DisableCoroutines();
            recalPath = playerCharacter.GetComponent<AIPathfinding>().RecalculatePath(0.3f);
            playerCharacter.GetComponent<AIPathfinding>().StartCoroutine(recalPath);
            GetComponent<TestMode>().StoreRulesets(GetComponent<PlayerSetup>().testRuleList);
        }
        Destroy(GameObject.Find("LevelCanvas"));
        SceneManager.LoadScene("LevelScene");
    }
    
    void ResetPlayer()
    {
        //Reset the player to the start of the level (in the event that the player gets stuck).
        if (GetComponent<PlayerController>().enabled == true && Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    void Update()
    {
        CheckGround();
        HorizontalMovement();
        JumpMovement();
        if (transform.position.y <= outOfLevel)
        {
            RestartLevel();
        }
        ResetPlayer();
    }
}
