using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mode
{
    Maze,
    Dungeon
}

public class MazeManager : MonoBehaviour
{
    [SerializeField] int width, height, levels;

    [SerializeField] Mode mode;

    [SerializeField]
    MazeGenerator mazeGenerator;
    
    [SerializeField]
    RecursiveDungeon dungeonGenerator;

    MazeGenerator[] mazes;
    DungeonGenerator[] dungeon;

    [SerializeField] GameObject StairwellStraight;
    [SerializeField] GameObject StairwellEnd;

    [SerializeField] int offsetStraightStairwell = 0;
    [SerializeField] int offsetDeadEndStairwell = 0;

    [SerializeField] GameObject DungeonStairs;
    [SerializeField] int offsetDungeonStairwell = 0;

    private void Start()
    {
        mazes = new MazeGenerator[levels];
        dungeon = new DungeonGenerator[levels];

        for(int i = 0; i<levels; i++)
        {
            if (mode == Mode.Maze)
            {
                MazeGenerator mg = Instantiate(mazeGenerator.gameObject, transform).GetComponent<MazeGenerator>();
                
                mg.size_X = width;
                mg.size_Z = height;
                mg.level = i;

                mg.BuildMaze();

                mazes[i] = mg;
            }

            if(mode == Mode.Dungeon)
            {
                DungeonGenerator dg = Instantiate(dungeonGenerator.gameObject, transform).GetComponent<DungeonGenerator>();

                dg.size_X = width;
                dg.size_Z = height;
                dg.level = i;

                dg.BuildDungeon();

                dungeon[i] = dg;
            }
        }

        for (int j = 0; j < levels - 1; j++)
        {
            if (mode == Mode.Maze)
            {
                for (int a = 0; a < mazes[j].size_X; a++)
                {
                    for (int b = 0; b < mazes[j].size_Z; b++)
                    {
                        print(mazes[j].pieceMap[a, b].tunnelPiece + " AND " + mazes[j+1].pieceMap[a, b].tunnelPiece);
                        if ((mazes[j].pieceMap[a, b].pieceModel != null && mazes[j + 1].pieceMap[a, b].pieceModel != null) && (mazes[j].pieceMap[a, b].tunnelPiece == TunnelPieces.DeadEnd || mazes[j].pieceMap[a, b].tunnelPiece == TunnelPieces.Straight))
                        {
                           
                            if (mazes[j].pieceMap[a, b].tunnelPiece == mazes[j + 1].pieceMap[a, b].tunnelPiece &&
                                mazes[j].pieceMap[a, b].pieceModel.transform.rotation == mazes[j + 1].pieceMap[a, b].pieceModel.transform.rotation)
                            {
                                //Instantiate New Pieces
                                if (mazes[j].pieceMap[a, b].tunnelPiece == TunnelPieces.Straight)
                                {
                                    GameObject g = Instantiate(StairwellStraight,
                                        mazes[j].pieceMap[a, b].pieceModel.transform.position,
                                        Quaternion.Euler(mazes[j].pieceMap[a, b].pieceModel.transform.eulerAngles + new Vector3(0, offsetStraightStairwell, 0)));

                                    Destroy(mazes[j].pieceMap[a, b].pieceModel);
                                    Destroy(mazes[j + 1].pieceMap[a, b].pieceModel);

                                    //Update pieceMap
                                    mazes[j].pieceMap[a, b] = new PieceInfo(TunnelPieces.StraightStairs, g);

                                    mazes[j + 1].pieceMap[a, b] = new PieceInfo(TunnelPieces.StraightStairs, null);
                                }

                                if (mazes[j].pieceMap[a, b].tunnelPiece == TunnelPieces.DeadEnd)
                                {
                                    GameObject g = Instantiate(StairwellEnd,
                                        mazes[j].pieceMap[a, b].pieceModel.transform.position,
                                        Quaternion.Euler(mazes[j].pieceMap[a, b].pieceModel.transform.eulerAngles + new Vector3(0, offsetDeadEndStairwell, 0)));

                                    Destroy(mazes[j].pieceMap[a, b].pieceModel);
                                    Destroy(mazes[j + 1].pieceMap[a, b].pieceModel);

                                    //Update pieceMap
                                    mazes[j].pieceMap[a, b] = new PieceInfo(TunnelPieces.DeadendStairs, g);

                                    mazes[j + 1].pieceMap[a, b] = new PieceInfo(TunnelPieces.DeadendStairs, null);
                                }
                            }
                        }

                    }
                }
            }
            
            if (mode == Mode.Dungeon)
            {
                for (int a = 0; a < dungeon[j].size_X; a++)
                {
                    for (int b = 0; b < dungeon[j].size_Z; b++)
                    {
                        print(dungeon[j].dungeonInfo[a, b].dungeonPieces + " AND " + dungeon[j + 1].dungeonInfo[a, b].dungeonPieces);
                        if ((dungeon[j].dungeonInfo[a, b].pieceModel != null && dungeon[j + 1].dungeonInfo[a, b].pieceModel != null) && (dungeon[j].dungeonInfo[a, b].dungeonPieces == DungeonPieces.DeadEnd))
                        {

                            if (dungeon[j].dungeonInfo[a, b].dungeonPieces == dungeon[j + 1].dungeonInfo[a, b].dungeonPieces &&
                                dungeon[j].dungeonInfo[a, b].pieceModel.transform.rotation == dungeon[j + 1].dungeonInfo[a, b].pieceModel.transform.rotation)
                            {
                                //Instantiate New Pieces
                                
                                GameObject g = Instantiate(DungeonStairs,
                                    dungeon[j].dungeonInfo[a, b].pieceModel.transform.position,
                                    Quaternion.Euler(dungeon[j].dungeonInfo[a, b].pieceModel.transform.eulerAngles + new Vector3(0, offsetDungeonStairwell, 0)));

                                Destroy(dungeon[j].dungeonInfo[a, b].pieceModel);
                                Destroy(dungeon[j + 1].dungeonInfo[a, b].pieceModel);

                                //Update pieceMap
                                dungeon[j].dungeonInfo[a, b] = new DungeonInfo(DungeonPieces.DeadendStairs, g);
                                dungeon[j + 1].dungeonInfo[a, b] = new DungeonInfo(DungeonPieces.DeadendStairs, null);
                                
                            }
                        }

                    }
                }
            }
        }



    }
}
