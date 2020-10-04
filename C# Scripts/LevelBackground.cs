using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBackground : MonoBehaviour
{
    /* This script provides the white cell background on the layer below the Level layer (containing the platforms), the white cells have no colliders 
     * and the separate layers allow the Astar pathfinding gridgraph layermask to only search the layer containing platforms, more easily detecting colliders and improving pathfinding.
     */
    int[,] levelGrid;
    GameObject level;
    public TileBase background;
    Tilemap backgroundTilemap;

    void Start()
    {
        CreateBackground();
    }

    void CreateBackground()
    {
        //Use the predefined variables from the LevelGenerator.cs script to ensure the generated white background size fits the level.
        //Make each cell in the levelGrid for this tilemap a white background cell.
        level = GameObject.Find("LevelTilemap");
        int levelHeight = level.GetComponent<LevelGenerator>().levelHeight;
        int levelWidth = level.GetComponent<LevelGenerator>().levelWidth;
        levelGrid = new int[levelWidth, levelHeight];
        backgroundTilemap = GetComponent<Tilemap>();
        for (int yAxis = 0; yAxis < levelHeight; yAxis++)
        {
            for (int xAxis = 0; xAxis < levelWidth; xAxis++)
            {
                Vector3Int cellPosition = new Vector3Int(-levelWidth + xAxis, -levelHeight + yAxis, 0);
                levelGrid[xAxis, yAxis] = 0;
                if (levelGrid[xAxis, yAxis] == 0)
                {
                    backgroundTilemap.SetTile(cellPosition, background);
                }
            }
        }
    }

}
