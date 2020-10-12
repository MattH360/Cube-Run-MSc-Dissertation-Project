# Cube-Run-MSc-Dissertation-Project
This file contains all 10 C# scripts for my MSc Advanced Software Engineering dissertation project. This README file gives more detail on the code sections and development process.

## **Script Descriptions**
All C# scripts shown here were created from scratch within Unity, however the AIPathfinding.cs script does utilise two scripts from the AStar Pathfinding Project package (https://arongranberg.com/astar/) for the purpose of implementing the AStar pathfinding algorithm as the creation of a pathfinding algorithm was outside of the requirements for the project since generation of Cellular Automata rules had been implemented. Minor sections for the setup of the Astar Pathfinding Project package were also based on the Brackeys 2D Pathfinding Tutorial (https://www.youtube.com/watch?v=jvtFUfJ6CP8) and are noted in the AIPathfinding.cs script.

### **AIPathfinding.cs**

void Start() (Lines 41-59) - When the AIPathfinding script starts, an initial scan for a grid-graph overlay of walkable and non-walkable nodes to aid pathfinding is executed. The path calculation is then started based on the generated grid-graph and the seeker (the target of the path) to determine if a viable route to the target is possible. Variables are also assigned here for the offset positions of sensors that surround the character to detect terrain for automated movement and assigning a variable to represent the PlayerCharacter GameObject 

