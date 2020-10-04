using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Pathfinding;
using System.IO;
using System.Threading;

public class AIPathfinding : MonoBehaviour
{
    /*This script provides the AI pathfinding capabilities with certain methods such as OnPathComplete and the first part of AIMove, 
     * based on the Brackeys 2D Pathfinding Youtube Tutorial (https://www.youtube.com/watch?v=jvtFUfJ6CP8).
     * All other methods were created independently of the tutorial.
     * Sensors to detect the character's surroundings and AI movement are also provided.
     * This script also utilises the Seeker script provided by the AStar pathfinding package (https://arongranberg.com/astar/).
     */

    //Pathfinding variables
    public Transform pathTarget;
    public float waypointDist = 0.1f;
    Pathfinding.Path currentPath;
    int currentWaypoint = 0;
    Seeker seeker;
    Rigidbody2D aiBody;
    //Movement sensor variables
    float aiSpeed = 2f;
    float aiJumpHeight = 6.5f;
    int layerMask;
    Vector3 aiPosition;
    Vector3 checkOffsetR, checkOffsetBM, checkOffsetBL, checkOffsetT, checkOffsetFR, checkOffsetLand, checkOffsetD, diagonalCheck, checkOffsetUR;
    float aiCheckRadius = 0.1f;
    bool checkR, checkBM, checkBL, checkT, checkFR, checkLand, checkDiag, checkUR;
    float checkDistance = 1.0f;
    float checkDistanceU = 3.0f;
    float undergroundLimit = -6.66f;
    float maxAIJumpHeight = -3.0f;
    //Coroutine variables
    bool repathWait = true;
    GameObject playerCharacter;
    
    void Start()
    {
        //Scan the level terrain to provide a grid-graph of unwalkable/walkable nodes on startup to allow pathfinding to generate, obtain required components for use within the script.
        //Set position offsets for the sensors around the character.
        AstarPath.active.Scan();
        seeker = GetComponent<Seeker>();
        aiBody = GetComponent<Rigidbody2D>();
        seeker.StartPath(aiBody.position, pathTarget.position, OnPathComplete); //3rd parameter is the function to call once the path has been calculated, create a new path if pathfinding is complete.
        checkOffsetR = new Vector3(0.3f, -0.3f, 0.0f);
        checkOffsetBM = new Vector3(0.0f, -0.3f, 0.0f);
        checkOffsetBL = new Vector3(-0.2f, -0.3f, 0.0f);
        checkOffsetT = new Vector3(0.0f, 0.3f, 0.0f);
        checkOffsetFR = new Vector3(0.3f, -0.3f, 0.0f);
        checkOffsetLand = new Vector3(0.0f, -0.3f, 0.0f);
        checkOffsetD = new Vector3(0.3f, 0.3f, 0.0f);
        checkOffsetUR = new Vector3(0.4f, 0.3f, 0.0f);
        diagonalCheck = new Vector3(1.0f, 1.0f, 0.0f);
        playerCharacter = GameObject.Find("PlayerCharacter");
    }

    void OnPathComplete(Pathfinding.Path p)
    {
        //Runs when pathfinding is complete, if there are no errors the start of a new path will become the current waypoint when the RecalculatePath coroutine is running.
        //Based on the Brackeys tutorial.
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        }
    }

    void CheckObstruction()
    {
        //A selection of sensors detecting colliders on each Unity editor layer (excluding the character's layer to prevent interference) detect terrain
        //to the Right and Front Right (to detect gaps). The Back Middle and Back Left circle colliders ensure the character is grounded for jumping, even when on the very edge of a platform.
        //Check the end of overhanging terrain with checkDiag and that the character is fully out from underneath a platform with checkUR (upward right).
        //If the y-axis position is at a specified depth the Top check is extended to check overhead platforms that may not be directly above the character.
        aiPosition = transform.position;
        layerMask = 1 << 10;
        layerMask = ~layerMask;

        checkR = Physics2D.Raycast(aiPosition + checkOffsetR, transform.TransformDirection(Vector3.right), checkDistance/2, layerMask); //Shorter right sensors allow for more movement in confined spaces.
        checkFR = Physics2D.Raycast(aiPosition + checkOffsetFR, transform.TransformDirection(Vector3.down), checkDistance, layerMask); //Check Front-Right downward from the character.
        checkDiag = Physics2D.Raycast(aiPosition + checkOffsetD, transform.TransformDirection(diagonalCheck), checkDistance, layerMask); //Check for platforms diagonally upward to the right of the character.
        checkUR = Physics2D.Raycast(aiPosition + checkOffsetUR, transform.TransformDirection(Vector3.up), checkDistance, layerMask); //Check directly upward from the top right corner.
        checkBM = Physics2D.OverlapCircle(aiPosition + checkOffsetBM, aiCheckRadius, layerMask);
        checkBL = Physics2D.OverlapCircle(aiPosition + checkOffsetBL, aiCheckRadius, layerMask);
        if (aiPosition.y > undergroundLimit)
        {
            checkT = Physics2D.Raycast(aiPosition + checkOffsetT, transform.TransformDirection(Vector3.up), checkDistance, layerMask);
        }
        else
        {
            checkT = Physics2D.Raycast(aiPosition + checkOffsetT, transform.TransformDirection(Vector3.up), checkDistanceU, layerMask);
        }
    }

    void AIMove()
    {
        //If there is a possible path to the target and the endpoint is not yet reached, determine the direction of and distance to the next waypoint.
        //Based on the Brackeys tutorial.
        if (currentPath == null)
        {
            return;
        }
        if (currentWaypoint >= currentPath.vectorPath.Count) //currentPath.vectorPath.Count refers to the total number of waypoints along the path.
        {
            return;
        }
        
        //Position of current waypoint minus current position giving a vector pointing to the next waypoint, cast vectorPath to a Vector2 to match aiBody.position in 2D.
        //Ensure vector is always length 1 by normalizing.
        Vector2 nextWayDirection = ((Vector2)currentPath.vectorPath[currentWaypoint] - aiBody.position).normalized;
        float nextWayDistance = Vector2.Distance(aiBody.position, currentPath.vectorPath[currentWaypoint]);

        //Created Independently
        /* The character moves in the x-axis direction of the generated path when there are no obstructions, the character will jump when an obstruction is detected in front or a platform has ended. 
         * The character must be in contact with a platform for jump to be enabled.
        */
        if (checkR == false && checkDiag == false)
        {
            aiBody.velocity = new Vector2(nextWayDirection.x * aiSpeed, aiBody.velocity.y);
        }
        if (checkR == true && (checkBM == true || checkBL == true))
        {
            aiBody.velocity = Vector2.up * aiJumpHeight;
        }
        if (checkFR == false && (checkBM == true || checkBL == true))
        {
            aiBody.velocity = Vector2.up * aiJumpHeight;
        }
        //To provide some in-air control to land on small platforms more consistently, a raycast is sent out to detect platforms underneath and velocity is slowed until height is decreased.
        if (playerCharacter.transform.position.y >= maxAIJumpHeight)
        {
            checkLand = Physics2D.Raycast(aiPosition + checkOffsetLand, transform.TransformDirection(Vector3.down), checkDistance * 3, layerMask);
            if (checkLand == true)
            {
                aiBody.velocity = Vector2.down;
            }
        }
        //If the character is underneath an overhanging platform it will move to the left in an attempt to move out 
        //and will detect when the end of the overhang is reached jumping to return to the surface.
        if (checkT == true)
        {
            aiBody.velocity = new Vector2(-nextWayDirection.x * aiSpeed, aiBody.velocity.y);
        }
        else if ((checkT == false && checkUR == false && checkDiag == true) && (checkBM == true || checkBL == true))
        {
            aiBody.velocity = Vector2.up * aiJumpHeight;
        }

        //Increase current waypoint counter each time the next waypoint is reached.
        //Based on Brackeys tutorial.
        if (nextWayDistance < waypointDist)
        {
            currentWaypoint++;
        }
    }

    void OnDrawGizmos()
    {
        //Visual representation of CheckObstruction method, visible in the Unity editor.

        Gizmos.color = Color.green;
        Gizmos.DrawRay(aiPosition + checkOffsetT, Vector3.up * checkDistance);
        Gizmos.DrawRay(aiPosition + checkOffsetR, Vector3.right * checkDistance/2);
        Gizmos.DrawRay(aiPosition + checkOffsetFR, Vector3.down * checkDistance);
        Gizmos.DrawRay(aiPosition + checkOffsetLand, Vector3.down * (checkDistance * 3)); //check when landing jump.
        Gizmos.DrawRay(aiPosition + checkOffsetD, diagonalCheck * checkDistance); //check diagonally upward in front of the character for platforms (could be the end of an overhang).
        Gizmos.DrawRay(aiPosition + checkOffsetUR, Vector3.up * checkDistance);
        Gizmos.DrawWireSphere(aiPosition + checkOffsetBM, aiCheckRadius);
        Gizmos.DrawWireSphere(aiPosition + checkOffsetBL, aiCheckRadius);
        if (transform.position.y > undergroundLimit)
        {
            Gizmos.DrawRay(aiPosition + checkOffsetT, Vector3.up * checkDistance);
        }
        else
        {
            Gizmos.DrawRay(aiPosition + checkOffsetT, Vector3.up * checkDistanceU);
        }
    }


    public IEnumerator RecalculatePath(float interval)
    {
        //Recalculates the AI path continuously after a given time interval (in seconds).
        while (repathWait == true)
        {
            yield return new WaitForSecondsRealtime(interval);
            seeker.StartPath(aiBody.position, pathTarget.position, OnPathComplete);
        }
    }

    public void DisableCoroutines()
    {
        StopAllCoroutines();
    }

    //FixedUpdate is called a fixed number of times per second inkeeping with the Unity physics systems.
    //Obstructions and movement are constantly checked.
    void FixedUpdate()
    {
        CheckObstruction();
        AIMove();
        if (transform.position.y <= playerCharacter.GetComponent<PlayerController>().outOfLevel)
        {
            playerCharacter.GetComponent<TestMode>().testCount += 1;
            GetComponent<PlayerController>().RestartLevel();
        }
    }
}
