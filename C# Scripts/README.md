# Cube-Run-MSc-Dissertation-Project
This file contains all 10 C# scripts for my MSc Advanced Software Engineering dissertation project. This README file gives more detail on the code sections and development process.

## **Script Descriptions**
All C# scripts shown here were created from scratch within Unity, however the AIPathfinding.cs script does utilise two scripts from the AStar Pathfinding Project package (https://arongranberg.com/astar/) for the purpose of implementing the AStar pathfinding algorithm as the creation of a pathfinding algorithm was outside of the requirements for the project since generation of Cellular Automata rules had been implemented. Minor sections for the setup of the Astar Pathfinding Project package were also based on the Brackeys 2D Pathfinding Tutorial (https://www.youtube.com/watch?v=jvtFUfJ6CP8) and are noted in the AIPathfinding.cs script.

### **AIPathfinding.cs**

**Start() (Lines 41-59)**

When the AIPathfinding script starts, an initial scan for a grid-graph overlay of walkable and non-walkable nodes to aid pathfinding is executed. The path calculation is then started based on the generated grid-graph and the seeker (the target of the path) to determine if a viable route to the target is possible. Variables are also assigned here for the offset positions of sensors that surround the character to detect terrain for automated movement and assigning a variable to represent the PlayerCharacter GameObject for component use.

**OnPathComplete() (Lines 61-70)**

Runs after the pathfinding calculation has started and sets the currentWaypoint counter to zero so that the current waypoint is set as the start of the path.

**CheckObstruction() (Lines 72-96)**

Sets a LayerMask for each of the terrain sensors to detect colliders on all layers, except the layer of the player character to prevent interference with the characters own collider. Use raycasts to detect obstructions in front of the character both at character hight and incoming gaps below in the movement direction. Raycasts also detect and platfroms above the character (i.e. when under overhanging terrain) and diagonally upward to the right to detect the end of an overhang. OverlapCircles are used to check when the character is in contact with a platform (grounded) as they detect colliders in a wider area not just at the end of a ray. If the character is more than two tiles below the surface level, the check sensor for platforms above the character extends in case platforms are not directly above. The sensor cannot be extended on the surface level otherwise it would detect the colliders on the tiles of the level border.

**AIMove() (Lines 98-158)**

If there is no path found or the end of the path is reached the method returns and the character will not move. Determine the direction of and distance to the next waypoint along the path. Using the range of sensors created in the CheckObstruction() method,the character moves in the x component direction of the generated path (to the right across the screen) until an obstruction or incoming gap is detected, causing the character to automatically jump if it is currently in contact with a platform tile (i.e. jump is enabled). As a human player can move left and right in the air to more accurately land on a platform, this mechanic was added as an option for the automated character. After the character reached the peak (maximum jump height) of a jump, a raycast is emitted and if a platform is detected below the velocity of the character is reduced causing a slowing effect visible when watching the automated playthrough. This intentional slowing allows the character to fall more accurately directly downward onto the platform (simulating the movement a human player could make). In an attempt to prevent the character becoming stuck under overhanging terrain, if the checkT raycast detects platforms above, the character attempts to move backward (to the left) which is likely back towards the enterance as the character continuously moves to the right. At the point when checkT (cast from the middle of the character) and checkUR (cast from the top right hand corner) detect that the player is fully out from under the overhang (there are no obstructions above) and checkDiag detects that the end tile of the overhang is diagonally upward to the right, the character can jump back to the surface level unobstructed (and engage movement to the right to land on the surface level at the peak of the jump). Each time a given waypoint alont the path is reached the currentWaypoint counter is inceased until the end of the route.

NOTE: Due to the randomised complexity of the generated terrain the automated character can still struggle with some terrain features, resulting in level failure.

**OnDrawGizmos() (Lines 160-181)** 

This method runs automatically to generate the visual representation of raycasts and overlapCircles in the Unity scene view. The gizmos are given the same dimensions and positions as the invisible sensors running in the game to accurately represent the same interactions. The extension of the checkT raycast is also represented.

**RecalculatePath() (Lines 184-192)**

This method is a public coroutine that acts similarly to a normal method but allows for delayed execution in this case by an interval of realtime seconds. The method is public as it is accessed by and executed within the persistent PlayerSetup.cs script when the test mode is activated. A new path is generated after every interval so that the pathfinding is constantly updating to detect the most direct route (often straight accross the surface level).

**DisableCoroutines() (Lines 194-197)**

A public method accessed and executed by the TestMode.cs script when the test mode is deactivated. Simply stops all running coroutines as the coroutines are only used during testing and deactivation returns control to the human player.

**FixedUpdate() (Lines 201-210)**

Called every fixed framerate frame, in line with the frequency of the physics system so that movement is framerate independed. Checks continuously for obstructions and character movement, resetting the level and updating the number of completed test runs if the character falls out of the level bounds.

### **CameraFollow.cs**

**Start() (Lines 19-22)**

Access the transform of the PlayerCharacter GameObject with a variable. Using the PlayerCharacter transform allows the camera to be placed initially at the position of the player and follow the player as it moves.

**LateUpdate() (Lines 25-36)**

Called after all other update methods and so ensures the character has finished moving. The camera position is continuously updated to the position of the character and clamped to ensure that it does not move upward when the character jumps. This prevents any area outside of the playable level being visible by the in-game camera. The camera moves when the character passes the centre of the camera view.

### **EndLevelTrigger.cs**

**Awake() (Lines 17-32)**

This method executes as soon as the script is run (before the Start method) and accesses GameObjects for later use in addition to using a singleton pattern to ensure persistence by deleting any new Endpoint instance created when a new level is generated.

**Start() (Lines 34-38)**

Executed after the Awake() method to set BoxCollider2D on the EndPoint GameObject as a trigger to reset the level when the player enters or touches the box collider.

**OnTriggerEnter2D() (Lines 40-49)**

A method executed automatically when a collider (in this case the character collider) enters the trigger area. This method also prevents the timer on the LevelCanvas GameObject (the timer on the games UI) from being destroyed when the level is restarted (using the DontDestroyOnLoad() Unity method), allowing the survival time to persist to the next level. The new zero timer created when the level restarts is quickly deleted by the Awake() method to prevent interference. While test mode is enabled, if the automated character reaches the trigger at the end of the level the counter for completed tests is increased and a new level is loaded.

### **LevelBackground.cs**

**Start() (Lines 16-19)**

Runs the CreateBackground method each time the script is run (when a new level is generated) to produce the white background for the level.

**CreateBackground() (Lines 21-42)**

Add white background tiles with no colliders onto a separate tilemap and Unity layer than the platform tiles. This allows the grid-graph layermask for the pathfinding to more easily generate the walkable and non-walkable nodes as only one layer can be selected. The same dimensions used in the LevelGenerator.cs script for the playform tile layer are used here to ensure the white background is of the same size and shape. The position of every tile on the tilemap is iterated through and set to a white background tile. The platform tilemap then sits on top of this layer to create the full level.

### **LevelGenerator.cs**

**Awake() (Lines 30-35)**

Runs before the Start() method and accesses the PlayerCharacter GameObject, spawns the player in the starting location and executes the CreateLevel() method to create a level of randomised 1's and 0's to later represent platform tiles and blank spaces respectively (blank spaces become white background cells as the background layer shows through).

**Start() (Lines 38-47)**

Adds platform tiles to the 1 positions to create an interactable tilemap with the CreateTileMap() method. Runs the pathfinding grid-graph scan each time a new level is created and the test mode is active as the scan on the AIPathfinding.cs script only runs once due to persistence so another scan was needed that would run for each new level.

**CreateLevel() (Lines 49-82)**

Creates a levelGrid array to define the fixed dimensions of the level, a border of one tile width is placed along the left, right and top of the level to prevent the player from falling off the sides and giving a sense of direction from the starting point so that the player instinctively knows to move to the right. The bottom half of the level is filled randomly with 1's and 0's. This produces the initial starting state of the level.

**CreateTileMap() (Lines 84-113)**

The LevelTilemap GameObject is accessed and used to apply a static Rigidbody2D to prevent movement of the tilemap. A TilemapCollider2D is also added and set to be used by a CompositeCollider2D so that each connected platform cell acts as a single collider (also helps with raycast detection by preventing multiple collider edges along platforms). If the levelGrid array is not empty (i.e. the CreateLevel() method has run) then each cell in the level is iterated through and a platform tile is placed if the position in the levelGrid contains a 1. This produces a complete tilemap of the initial level starting state and a set of Cellular Automata rules can then be generated and applied to alter the terrain with procedural generation using the ApplyCARules() method.


