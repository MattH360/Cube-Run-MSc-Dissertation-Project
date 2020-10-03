using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using UnityEditor;

public class TestMode : MonoBehaviour
{
    /* Script provides the test mode (toggled on and off with the 'T' key), distances travelled by the testing AI are logged here and output to a CSV file TestResults.csv 
     * along with the rulesets used in Rulesets.csv.
     */
    
    public int testCount = 0;
    public int totalTests = 1;
    public int maxTests = 1; //Can be changed to determine how many tests run before the ruleset changes.
    GameObject levelMap;
    int ruleAmount;
    int levelWidth;
    float currentXPosition;
    [SerializeField] //Allows the variable to display in the Unity editor inspector without being public.
    float loggedXPosition;
    [SerializeField]
    float spawnXPosition;
    [SerializeField]
    float distTravelled;
    [SerializeField]
    float percentComplete;
    float[] distanceArray;
    float[] perCompleteArray;
    bool logWait = true;
    
    void Start()
    {
        //Start is called before the first frame update and initialises variables to access game object variables such as starting spawn position of the character and level width.
        //Array sizes are also initialised.
        levelMap = GameObject.Find("LevelTilemap");
        spawnXPosition = levelMap.GetComponent<LevelGenerator>().spawnPosition.x;
        levelWidth = levelMap.GetComponent<LevelGenerator>().levelWidth;
        distanceArray = new float[totalTests];
        perCompleteArray = new float[totalTests];
        
    }

    public void StoreRulesets(int[][][] rList)
    {
        //Amends each rule to the Rulesets.csv file by iterating through each element of the array input into the method.
        //Any errors are caught and output to the Unity editor console.
        try
        {
            string filePath = "Assets/TestFiles/Rulesets.csv";
            StreamWriter sWriter = null;
            sWriter = new StreamWriter(filePath, true);
            ruleAmount = levelMap.GetComponent<LevelGenerator>().ruleAmount;
            sWriter.WriteLine("Rule:" + "," + "Cell:" + "," + "Cell X:" + "," + "Cell Y:");
            for (int rAmount = 0; rAmount < ruleAmount; rAmount++)
            {
                for (int cell = 0; cell < rList[rAmount].Length; cell++)
                {
                    sWriter.WriteLine(rAmount + "," + cell + "," + "[" + rList[rAmount][cell][0] + "," + rList[rAmount][cell][1] + "]");

                }

            }
            sWriter.Flush();
            sWriter.Close();
            EditorUtility.DisplayDialog("File Created!", "Rulesets added to file at: " + filePath, "Ok");
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
        
    }

    public void CreateResultsFile(float[] dist, float[] perComp)
    {
        //Creates a file called TestResults.csv containing all of the distances travelled and the percentage complete of each level iteration during testing.
        try 
        {
            string filePath = "Assets/TestFiles/TestResults.csv";
            StreamWriter sWriter = null;
            sWriter = new StreamWriter(filePath, true);
            sWriter.WriteLine("Test ID" + "," + "Distance Travelled" + "," + "Percentage Complete");
            for (int t = 0; t < totalTests; t++)
            {
                sWriter.WriteLine(t + "," + dist[t] + "," + perComp[t]);
                
            }
            sWriter.Flush();
            sWriter.Close();
            EditorUtility.DisplayDialog("File Created!", "Test results added to file at: " + filePath, "Ok");
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }

    
    void DeactivateTest()
    {
        //Deactivate automated testing with the 'T' Key, stopping any coroutines from running and disabling the relevant scripts.
        //Returns control to the player.
        if (Input.GetKeyDown(KeyCode.T))
        {
            GetComponent<AIPathfinding>().DisableCoroutines();
            GetComponent<AIPathfinding>().enabled = false;
            GetComponent<PlayerController>().enabled = true;
            StopAllCoroutines();
            GetComponent<TestMode>().enabled = false;
        }
    }

    public IEnumerator LogDistanceTravelled(float interval)
    {
        //Coroutine that takes a given time interval when called, and logs the distance the character travels, 
        //continously taking the current x-axis position and waiting for a given interval of seconds.
        //If the logged posiitions are equal or the character has fallen outside of the level, distances are calculated. 
        //As the game map is positioned in the negative x-axis, distance travelled is taken as a negative (to produce a positive value).
        while (logWait == true)
        {
            for(int t = testCount; t <= totalTests; t++)
            {
                currentXPosition = transform.position.x;
                yield return new WaitForSecondsRealtime(interval);
                loggedXPosition = transform.position.x;
                if (loggedXPosition == currentXPosition || transform.position.y <= GetComponent<PlayerController>().outOfLevel)
                {
                    distTravelled = -spawnXPosition + loggedXPosition;
                    percentComplete = (distTravelled / (float)levelWidth) * 100;
                    distanceArray[testCount] = distTravelled;
                    perCompleteArray[testCount] = percentComplete;
                    testCount += 1;
                    GetComponent<PlayerController>().RestartLevel();
                }
            }
        }
    }

    void Update()
    {
        //Update is called once per frame checking if the testMode is deactivated, or if the test is complete, produces the results file and exits test mode.
        DeactivateTest();
        if(testCount == totalTests)
        {
            CreateResultsFile(distanceArray, perCompleteArray);
            GetComponent<AIPathfinding>().DisableCoroutines();
            GetComponent<AIPathfinding>().enabled = false;
            GetComponent<PlayerController>().enabled = true;
            StopAllCoroutines();
            GetComponent<TestMode>().enabled = false;
        }
    }
}
