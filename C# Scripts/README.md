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

If there is no path found or the end of the path is reached the method returns and the character will not move. Determine the direction of and distance to the next waypoint along the path. Using the range of sensors created in the CheckObstruction() method,the character moves in the x component direction of the generated path (to the right across the screen) until an obstruction or incoming gap is detected, causing the character to automatically jump if it is currently in contact with a platform tile (i.e. jump is enabled). As a human player can move left and right in the air to more accurately land on a platform, this mechanic was added as an option for the automated character. After the character reached the peak (maximum jump height) of a jump, a raycast is emitted and if a platform is detected below the velocity of the character is reduced causing a slowing effect visible when watching the automated playthrough. This intentional slowing allows the character to fall more accurately directly downward onto the platform (simulating the movement a human player could make).
