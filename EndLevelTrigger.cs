using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevelTrigger : MonoBehaviour
{
    /*This script provides an endpoint to the level which acts as a target for AI pathfinding and as a way to restart the level while maintaining the current timer.
     */
    GameObject endPoint;
    BoxCollider2D endTrigger;
    GameObject playerCharacter;
    GameObject timer;
    private static EndLevelTrigger triggerInstance;
    
    void Awake()
    {
        //Ensure persistence so that the endpoint is always available as the target for the AIPathfinding.cs script. 
        endPoint = GameObject.Find("Endpoint");
        playerCharacter = GameObject.Find("PlayerCharacter");
        timer = GameObject.Find("LevelCanvas");
        if (triggerInstance == null)
        {
            DontDestroyOnLoad(this);
            triggerInstance = this;
        }
        else if (triggerInstance != null)
        {
            Destroy(this.gameObject);
        }
    }
    
    void Start()
    {
        //Set the collider to be used as a trigger for the OnTriggerEnter2D method.
        endPoint.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        //When the player collides with the endpoint the game object containing the timer is set to persist across to the next level.
        DontDestroyOnLoad(GameObject.Find("LevelCanvas"));
        if (playerCharacter.GetComponent<TestMode>().enabled == true)
        {
            playerCharacter.GetComponent<TestMode>().testCount += 1;
        }
        SceneManager.LoadScene("LevelScene");
    }

}
