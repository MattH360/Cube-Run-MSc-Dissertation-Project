using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class LevelGenerator : MonoBehaviour
{
    /* This script generates a random initial state for each level and creates and applies the Cellular Automata rulesets to produce procedurally generated levels.
     * The player is also moved into position with the SpawnPlayer method at the start of each level.
     */
    int[,] levelGrid;
    int[,] nCells;
    public int levelWidth = 200;
    public int levelHeight = 10;
    int cellNumber, nCellSelectX, nCellSelectY;
    int cellCount;
    public int ruleAmount = 10;
    public TileBase platform;
    Tilemap levelTilemap;
    GameObject levelMapObject;
    GameObject playerCharacter;
    LevelGenerator levelGen;
    public int[][][] ruleList;
    public Vector3 spawnPosition;
    

    //Awake is called before the Start method
    void Awake()
    {
        playerCharacter = GameObject.Find("PlayerCharacter");
        SpawnPlayer();
        CreateLevel();
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateTileMap();
        //Scan a new gridgraph for pathfinding with each new level if TestMode is enabled.
        if (playerCharacter.GetComponent<TestMode>().enabled == true)
        {
            AstarPath.active.Scan();
        }

    }

    void CreateLevel()
    {
        /* Iterate through each cell in the level and randomly assign 1's and 0's to denote platforms and background cells respectively.
         * This produces the inital starting state to then apply Cellular Automata rules.
         */ 
        levelGrid = new int[levelWidth, levelHeight];
        System.Random randBinary = new System.Random();
        //Add a border of platforms one cell width along the left, right and top.
        for (int yAxis = 0; yAxis < levelHeight; yAxis++)
        {
            for (int xAxis = 0; xAxis < levelWidth; xAxis++)
            {
                if (xAxis == 0 || xAxis == levelWidth - 1 || yAxis == levelHeight -1)
                {
                    levelGrid[xAxis, yAxis] = 1;
                }
                
            }
        }

        //Fill the bottom half of the level randomly with blank cells "0" and black '1' cells.
        //The white background is created on a separate layer with the LevelBackground.cs script.
        for (int yAxis = 0; yAxis < levelHeight/2; yAxis++)
        {
            for (int xAxis = 0; xAxis < levelWidth; xAxis++)
            {
                if (xAxis >= 3 || xAxis <= levelWidth -4)
                {
                    cellNumber = randBinary.Next(0, 2);
                    levelGrid[xAxis, yAxis] = cellNumber;
                }
            }
        }   
    }

    void CreateTileMap()
    {
        //Create the level as a Tilemap to allow the character to collide with each platform, assign a platform tile to each "1" cell.
        levelMapObject = GameObject.Find("LevelTilemap");
        levelTilemap = GetComponent<Tilemap>();
        levelMapObject.AddComponent<Rigidbody2D>();
        levelMapObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        levelMapObject.AddComponent<TilemapCollider2D>();
        levelMapObject.AddComponent<CompositeCollider2D>();
        levelMapObject.GetComponent<TilemapCollider2D>().usedByComposite = true;

        if (levelGrid != null)
        {
            
            for (int yAxis = 0; yAxis < levelHeight; yAxis++)
            {
                for (int xAxis = 0; xAxis < levelWidth; xAxis++)
                {
                    Vector3Int cellPosition = new Vector3Int(-levelWidth + xAxis, -levelHeight + yAxis, 0); //Vector3 format to work with the SetTile method.
                    if (levelGrid[xAxis, yAxis] == 1)
                    {
                        levelTilemap.SetTile(cellPosition, platform);
                    }

                }
            }
            
            ApplyCARules();
        }
    }


    void ApplyCARules()
    {
        /*Create a starting ruleset and a new ruleset each time a specified number of tests is completed, initialise an array with randomised cell counts to store each cell and rule that make the ruleset.
         * Attempt to prevent duplicate cells where possible, the ruleset is saved and then loaded each time the level restarts to prevent a new ruleset being used for each new test level.
         * Rules are generated in a Moore neighbourhood and checked for each cell within the level, rules are applied if the surrounding cells match the configuration.
         */
        System.Random randCell = new System.Random();
        //Only run new ruleset code when testmode is disabled (first time running the game) or when e.g. 100 testing iterations have been reached.
        if (playerCharacter.GetComponent<PlayerSetup>().switchRuleCount == 0 || playerCharacter.GetComponent<PlayerSetup>().switchRuleCount == playerCharacter.GetComponent<TestMode>().maxTests)
        {
            playerCharacter.GetComponent<PlayerSetup>().switchRuleCount = 0;

            ruleList = null;
            ruleList = new int[ruleAmount][][];
            for (int rAmount = 0; rAmount < ruleAmount; rAmount++)
            {
                cellCount = randCell.Next(0, 9);
                ruleList[rAmount] = new int[cellCount][];
                playerCharacter.GetComponent<PlayerSetup>().testRuleList[rAmount] = new int[cellCount][];

            }
            for (int rAmount = 0; rAmount < ruleAmount; rAmount++)
            {
                for (int cell = 0; cell < ruleList[rAmount].Length; cell++)
                {
                    ruleList[rAmount][cell] = new int[2];
                    playerCharacter.GetComponent<PlayerSetup>().testRuleList[rAmount][cell] = new int[2];
                }
            }
            
            //Select 10 rules to make up a ruleset.
            for (int rAmount = 0; rAmount < ruleAmount; rAmount++)
            {
                cellCount = ruleList[rAmount].Length; //Sets cellCount equal to the array length for each set of cells.
                 
                for (int cell = 0; cell < cellCount; cell++)
                {
                    //Selects the axis position for random rule cell placement.
                    CalculateCells(ruleList, rAmount, cell, randCell);
                }
                //Attempts to prevent the inclusion of the target cell [1,1] and any duplicate cells within a rule.
                for (int cell = 0; cell < cellCount; cell++)
                {
                    for (int nextCell = cell + 1; nextCell < cellCount; nextCell++)
                    {
                        //Check all cells in front of current position for duplicate matches or for the inclusion of the target cell, randomise again if any matches are found.
                        while (ruleList[rAmount][cell][0] == ruleList[rAmount][nextCell][0] && ruleList[rAmount][cell][1] == ruleList[rAmount][nextCell][1])
                        {
                            CalculateCells(ruleList, rAmount, cell, randCell);

                            //If the cell has preceding cells check each of these for duplicate matches.
                            if (cell >= 1)
                            {
                                for (int prevCell = cell - 1; prevCell >= 0; prevCell--)
                                {
                                    while (ruleList[rAmount][cell][0] == ruleList[rAmount][prevCell][0] && ruleList[rAmount][cell][1] == ruleList[rAmount][prevCell][1])
                                    {
                                        CalculateCells(ruleList, rAmount, cell, randCell);

                                        while (ruleList[rAmount][cell][0] == 1 && ruleList[rAmount][cell][1] == 1)
                                        {
                                            CalculateCells(ruleList, rAmount, cell, randCell);
                                        }
                                    }
                                }

                            }

                            while (ruleList[rAmount][cell][0] == 1 && ruleList[rAmount][cell][1] == 1)
                            {
                                CalculateCells(ruleList, rAmount, cell, randCell);
                            }
                        }

                    }
                    //Check again for the selected target cell outside of the check for duplicate cells in case no duplicates are found.
                    while (ruleList[rAmount][cell][0] == 1 && ruleList[rAmount][cell][1] == 1)
                    {
                        CalculateCells(ruleList, rAmount, cell, randCell);
                    }
                }
            }

            playerCharacter.GetComponent<PlayerSetup>().rulesetCount += 1;
            //Save rule list to the testRuleList array in the PlayerSetup.cs script for persistent storage.
            for (int r = 0; r < ruleAmount; r++)
            {
                for (int c = 0; c < ruleList[r].Length; c++)
                {
                    for(int i = 0; i < 2; i++)
                    {
                        playerCharacter.GetComponent<PlayerSetup>().testRuleList[r][c][i] = ruleList[r][c][i];
                    }
                }
            }
        }

        //Load saved rule list each time a level restarts until a new ruleset is created.
        ruleList = playerCharacter.GetComponent<PlayerSetup>().testRuleList;
        
        playerCharacter.GetComponent<PlayerSetup>().switchRuleCount += 1;
        for (int yAxis = 1; yAxis < levelHeight / 2; yAxis++)
        {
            for (int xAxis = 3; xAxis < levelWidth - 3; xAxis++)
            {
                Vector3Int cellPosition = new Vector3Int(-levelWidth + xAxis, -levelHeight + yAxis, 0);
                nCells = new int[3, 3];
                //Moore neighbourhood cells
                nCells[0, 0] = levelGrid[xAxis - 1, yAxis - 1]; //bottom-left
                nCells[1, 0] = levelGrid[xAxis, yAxis - 1]; //bottom-middle
                nCells[2, 0] = levelGrid[xAxis + 1, yAxis - 1]; //bottom-right
                nCells[0, 1] = levelGrid[xAxis - 1, yAxis]; //middle-left
                nCells[1, 1] = levelGrid[xAxis, yAxis]; //middle (centre target cell)
                nCells[2, 1] = levelGrid[xAxis + 1, yAxis]; //middle-right
                nCells[0, 2] = levelGrid[xAxis - 1, yAxis + 1]; //top-left
                nCells[1, 2] = levelGrid[xAxis, yAxis + 1]; //top-middle
                nCells[2, 2] = levelGrid[xAxis + 1, yAxis + 1]; //top-right
                
                for (int ruleCount = 0; ruleCount < ruleAmount; ruleCount++)
                {
                    CheckEachRule(ruleCount, nCells, cellPosition, randCell);
                }
            }

        }
    }

    void CalculateCells(int[][][] ruleList, int rAmount, int cell, System.Random randCell) 
    {
        //Calculate or recalculate cell positions within a given Cellular Automata rule.
        nCellSelectX = randCell.Next(0, 3);
        nCellSelectY = randCell.Next(0, 3);
        ruleList[rAmount][cell][0] = nCellSelectX;
        ruleList[rAmount][cell][1] = nCellSelectY;
    }

    void CheckEachRule(int ruleCount, int[,] nCells, Vector3Int cellPosition, System.Random randCell)
    {
        /* Loop through the cells that make up the rule ensuring the cells match platform positions in the level scene (i.e. the rule is valid) and change the target cell accordingly.
         * As the white background tiles are placed on another layer with the LevelBackground script, if a rule matches the surrounding cell configuration either a platform is placed or the function exits.
         * For rules containing no cells the surrounding cells are checked ensuring no platforms are found and the target cell is changed without applying any specific rules.
         */
        int randTile = randCell.Next(0, 2);
        cellCount = ruleList[ruleCount].Length;
        bool checkRule = false;
        bool checkCell = false;
        if (cellCount == 0) //For cases of no surrounding platform cells check all positions are equal to 0 (no platforms).
        {
            for (int nCellX = 0; nCellX < 3; nCellX++)
            {
                for (int nCellY = 0; nCellY < 3; nCellY++)
                {
                    if (nCells[nCellX, nCellY] == 0 && nCells[1, 1] == 1)
                    {
                        checkCell = true;
                        
                    }
                    else
                    {
                        checkCell = false;
                        return;
                    }
                }
            }

            if (checkCell == true)
            {
                if(randTile == 1)
                {
                    levelTilemap.SetTile(cellPosition, platform);
                }
                else
                {
                    return;
                }  
            }
        }
        else
        {
            //Check that the platform configuration of the rule matches the surrounding platform positions. 
            for (int cell = 0; cell < cellCount; cell++)
            {
                if (nCells[ruleList[ruleCount][cell][0], ruleList[ruleCount][cell][1]] == 1)
                {
                    checkRule = true;
                }
                else
                {
                    checkRule = false;
                    return;
                }
            }
            
            //If platform cells match ensure that surrounding white background cells also match their positions for the rule.
            if (checkRule == true)
            {
                for (int nCellX = 0; nCellX < 3; nCellX++)
                {
                    for (int nCellY = 0; nCellY < 3; nCellY++)
                    {
                        for (int cell = 0; cell < cellCount; cell++)
                        {
                            if(nCells[nCellX, nCellY] != nCells[ruleList[ruleCount][cell][0], ruleList[ruleCount][cell][1]] && nCells[nCellX, nCellY] != nCells[1, 1])
                            {
                                if (nCells[nCellX, nCellY] == 0)
                                {
                                    checkRule = true;
                                }
                                else
                                {
                                    checkRule = false;
                                    return;
                                }
                            }
                            
                        }
                    }
                }
            }
            //Randomise the target cell.
            if (checkRule == true)
            {
                if (randTile == 1)
                {
                    levelTilemap.SetTile(cellPosition, platform);
                }
                else
                {
                    return;
                }
            }
        }
    }

    void SpawnPlayer()
    {
        /* Move the PlayerCharacter object to the spawn position each time a level is generated and set the character layer to 10,
         * ensuring player appears on top of the level scene (i.e. player is visible) and by setting z axis to -1.
         */
        spawnPosition = new Vector3(-levelWidth + 3, -levelHeight / 2 + 1, -1);
        playerCharacter.transform.position = spawnPosition;
        playerCharacter.layer = 10;
    }

}
