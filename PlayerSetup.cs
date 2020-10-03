using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PlayerSetup : MonoBehaviour
{
    /*Script to maintain a persistent saved ruleset and any other required persistent methods or variables.
    */
    private static PlayerSetup playerInstance;
    GameObject playerCharacter;
    Rigidbody2D playerBody;
    IEnumerator recalPath;
    IEnumerator logDistance;
    public int switchRuleCount = 0;
    public int rulesetCount = 0;
    public int[][][] testRuleList;

    void Awake()
    {
        //Initialise the playercharacter game object by adding the required component settings and use a singleton pattern to ensure persistence of the player character (and therefore this script).
        //Initialises the size of the jagged array (an array of different sized arrays) to store the rulelist created by the LevelGenerator script.
        if (playerInstance == null)
        {
            DontDestroyOnLoad(this);
            playerInstance = this;
        }
        else if (playerInstance != null)
        {
            Destroy(this.gameObject);
        }
        playerCharacter = GameObject.Find("PlayerCharacter");
        playerBody = playerCharacter.GetComponent<Rigidbody2D>();
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous; //Continuous ensures OnTriggerEnter() only runs once when player enters rather than continuously (with Discrete detection).
        
        testRuleList = new int[10][][];
    }

    void ActivateTest()
    {
        /*Activate Automated testing with 'T' Key, this method needs to be accessible when the TestMode script is disabled, 
         * Coroutines for recalculating the Astar pathfinding path every 0.3 seconds and logging the distance checking every 10 seconds are also started upon test mode activation. 
         */
        if (Input.GetKeyDown(KeyCode.T))
        {
            AstarPath.active.Scan();
            playerCharacter.GetComponent<TestMode>().enabled = true;
            playerCharacter.GetComponent<TestMode>().testCount = 0;
            playerCharacter.GetComponent<PlayerController>().enabled = false;
            playerCharacter.GetComponent<AIPathfinding>().enabled = true;
            recalPath = playerCharacter.GetComponent<AIPathfinding>().RecalculatePath(0.3f);
            playerCharacter.GetComponent<AIPathfinding>().StartCoroutine(recalPath);
            logDistance = playerCharacter.GetComponent<TestMode>().LogDistanceTravelled(10.0f); //as loadtimes increase for levels time interval before restarting with no movment should be increased.
            playerCharacter.GetComponent<TestMode>().StartCoroutine(logDistance);
        }
    }


    void Update()
    {
        ActivateTest();
    }
}
