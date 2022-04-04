using System.Collections.Generic;
using UnityEngine;


public enum DungeonPieces
{
    Straight,
    TJunction,
    XJunction,
    Corner,
    DeadEnd,
    DeadendStairs,
    Room
}

[System.Serializable]
public struct DungeonInfo
{
    public DungeonPieces dungeonPieces;
    public GameObject pieceModel;

    public DungeonInfo(DungeonPieces p, GameObject m)
    {
        dungeonPieces = p;
        pieceModel = m;
    }
}

[System.Serializable]
public class Point
{
    public int x, z;
    
    public Point(int _x, int _z) { x = _x; z = _z; }

}

public class DungeonGenerator : MonoBehaviour
{

    [SerializeField]
    [Range(1, 10)]
    int scale = 5;  //Appropriate Scale for my pieces 

    protected List<Point> directions = new List<Point>() {
                                            new Point(1,0),
                                            new Point(0,1),
                                            new Point(-1,0),
                                            new Point(0,-1) };

    [SerializeField]
    List<Point> roomPoints = new List<Point>();

    [SerializeField]
    public int size_X= 10, size_Z= 10;

    [SerializeField]
    protected byte[,] map;

    public DungeonInfo[,] dungeonInfo;

    [SerializeField] GameObject playerPrefab;

    Vector3 playerPos;

    [SerializeField] GameObject StraightPiece;
    [SerializeField] GameObject CornerPieceCurved;
    [SerializeField] GameObject CornerPiece;
    [SerializeField] GameObject TJunctionPiece;
    [SerializeField] GameObject XJunctionPiece;
    [SerializeField] GameObject DeadEndPiece;

    //To manipulate how models should be rotated
    [SerializeField] int offsetStraightPiece = 0;
    [SerializeField] int offsetCornerPiece = 0;
    [SerializeField] int offsetTJuncPiece = 0;
    [SerializeField] int offsetXJuncPiece = 0;
    [SerializeField] int offsetDeadEndPiece = 0;


    [Header("Room Pieces")]
    [SerializeField] GameObject roomMiddle;
    [SerializeField] int roomMiddleOffset;
    [SerializeField] GameObject roomSide;
    [SerializeField] int roomSideOffset;
    [SerializeField] GameObject roomCorner;
    [SerializeField] int roomCornerOffset;
    [SerializeField] GameObject roomEntranceLeft;
    [SerializeField] int roomLeftEntranceOffset;
    [SerializeField] GameObject roomEntranceMiddle;
    [SerializeField] int roomMiddleEntranceOffset;
    [SerializeField] GameObject roomEntranceRight;
    [SerializeField] int roomRightEntranceOffset;

    //To determine neighbours position
    private bool top, right, bottom, left;

    [SerializeField]
    List<Point> entrancePoints = new List<Point>();

    //To Store list of entrance per room
    [SerializeField]
    List<Point> tempEntrancePoint = new List<Point>();

    //For multilevel maze
    public int level = 0;
    [SerializeField] int heightOffset = 1;

    //To get the position of hall where player should spawn
    protected void SetPosition(int x, int z)
    {
        playerPos = new Vector3(x * scale, 5f, z * scale);
    }

    // Start is called before the first frame update
    private void Start()
    {
        //BuildDungeon();
    }

    internal void BuildDungeon()
    {
        dungeonInfo = new DungeonInfo[size_X, size_Z];

        InitializeMap();
        Generate();
        AddRooms(3, 2, 5);
        DrawMap();
        //PlacePlayer(playerPos);
    }

    //Dermine positions of 
    private void DetermineEntrance(int noOfDoors)
    {
        SortTempList();

        for(int c = 0; c < noOfDoors; c++)
        {
            int r = Random.Range(0, tempEntrancePoint.Count - 1);

            entrancePoints.Add(tempEntrancePoint[r]);

            //Handle Empty tempEntranceList
            
            if (DetermineHallwayPosition(tempEntrancePoint[r]) == 1)
            {
                if (top)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x, tempEntrancePoint[r].z + 1));
                }
                if (right)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x + 1, tempEntrancePoint[r].z));
                }
                if (bottom)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x, tempEntrancePoint[r].z - 1));
                }
                if (left)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x - 1, tempEntrancePoint[r].z));
                }
            }

            if (DetermineHallwayPosition(tempEntrancePoint[r]) == 2)
            {
                if (top && right)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x + 1, tempEntrancePoint[r].z));
                }
                if (right && bottom)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x + 1, tempEntrancePoint[r].z));
                }
                if (bottom && left)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x, tempEntrancePoint[r].z - 1));
                }
                if (left && top)
                {
                    entrancePoints.Add(new Point(tempEntrancePoint[r].x, tempEntrancePoint[r].z + 1));
                }
            }

            tempEntrancePoint.RemoveAt(r);
        }
    }

    private void SortTempList()
    {
        for(int i = 0; i < tempEntrancePoint.Count; i++)
        {
            if (DetermineHallwayPosition(tempEntrancePoint[i]) == 0)
            {
                tempEntrancePoint.RemoveAt(i--);
            }
        }
    }

    private int DetermineHallwayPosition(Point point)
    {
        top = false;
        right = false; 
        bottom = false; 
        left = false;

        int count = 0;
        //if (point.x <= 0 || point.x >= size_X - 1 || point.z <= 0 || point.z >= size_Z - 1) return 5;   //To determine edge

        if (map[point.x - 1, point.z] == 0 && !IsRoomPoint(point.x - 1, point.z)) { left = true; count++; }
        if (map[point.x + 1, point.z] == 0 && !IsRoomPoint(point.x + 1, point.z)) { right = true; count++; }
        if (map[point.x, point.z - 1] == 0 && !IsRoomPoint(point.x, point.z - 1)) { bottom = true; count++; }
        if (map[point.x, point.z + 1] == 0 && !IsRoomPoint(point.x, point.z + 1)) { top = true; count++; }

        print(point.x + " " + point.z + " " + count + " " + top + " " + left + " " + right + " " + bottom);

        return count;
    }

    //Add Specified no of rooms with Random Width and Depth between specified min and max size
    private void AddRooms(int noOfRooms, int minSize, int maxSize)
    {
        for (int n = 0; n < noOfRooms; n++)
        {
            tempEntrancePoint.Clear();

            int start_X = Random.Range(1, size_X - maxSize);
            int start_Z = Random.Range(1, size_Z - maxSize);

            print("Start Pos: " + start_Z + " " + start_X);

            int roomWidth = Random.Range(minSize, maxSize);
            int roomDepth = Random.Range(minSize, maxSize);

            print("Room Dimentions: " + roomWidth + " " + roomDepth);

            for (int z = 0; z < roomDepth; z++)
            {
                for (int x = 0; x < roomWidth; x++)
                {
                    print(x + " " + z);
                    map[x + start_X, z + start_Z] = 0;

                    roomPoints.Add(new Point(x + start_X, z + start_Z));

                    if (x == 0 || x == roomWidth - 1 || z == 0 || z == roomDepth - 1)
                    {
                        tempEntrancePoint.Add(new Point(x + start_X, z + start_Z));
                    }
                }
            }

            DetermineEntrance(3);
        }
    }

    //Place player in the first hall created by algorithm (as of now) 
    private void PlacePlayer(Vector3 pos)
    {
        Instantiate(playerPrefab, pos, Quaternion.identity);
    }


    //It is advisable to make a generate() which will either based on some algorithm or randomly set the bytes to 0, in initialize, set all elements to 1
    private void InitializeMap()
    {
        map = new byte[size_X, size_Z];
        for (int x = 0; x < size_X; x++)
        {
            for (int z = 0; z < size_Z; z++)
            {
                map[x, z] = 1;   
            }
        }
    }

    //Generate map
    public virtual void Generate()
    {
        for (int x = 0; x < size_X; x++)
        {
            for (int z = 0; z < size_Z; z++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    map[x, z] = 0;
                }
            }
        }
    }

    //Placing cubes according to map
    private void DrawMap()
    {
        for (int x = 0; x < size_X; x++)
        {
            for (int z = 0; z < size_Z; z++)
            {
                /** Place Cubes for walls 
                //if (map[x, z] == 1)
                //{
                //    Vector3 pos = new Vector3(x * scale, scale / 2f, z * scale);
                //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    cube.transform.position = pos;
                //    cube.transform.localScale = new Vector3(scale, scale, scale);
                //    cube.name = "Cube_" + z.ToString() + "_" + x.ToString();
                //}
                */
                Point p = new Point(x, z);

                /** Place appropriate maze piece */
                if(!IsRoomPoint(x,z) && !IsEntrancePoint(x,z))
                {
                    PlaceMazePiece(x, z);
                }
                else if(!IsEntrancePoint(x,z))
                {
                    PlaceRoomPieces(x, z);
                }
                else
                {
                    PlaceEntrancePieces(x, z);
                }
            }
        }
    }

    private void PlaceEntrancePieces(int x, int z)
    {
        Vector3 pos = new Vector3(x * scale, scale / 2f + (level * heightOffset), z * scale);

        GameObject g = null;

        if (IsRoomPoint(x, z))
        {
            if (DetermineHallwayPosition(new Point(x,z)) == 1)
            {
                if (top)
                {
                    if (CompareRoomPatern(x, z, new int[] { 5, 0, 0, 5 }))
                    {
                        //TopLeftCornerPiece
                        g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, 0f + roomLeftEntranceOffset, 0f)));
                    }
                    else if(CompareRoomPatern(x, z, new int[] { 5, 5, 0, 0 }))
                    {
                        //TopRightCornerPiece
                        g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 0f + roomRightEntranceOffset, 0f)));
                    }
                    else if(CompareRoomPatern(x,z,new int[] { 5, 0, 0, 0 }))
                    {
                        g = Instantiate(roomEntranceMiddle, pos, Quaternion.Euler(new Vector3(0f, 0f + roomMiddleEntranceOffset, 0f)));
                    }
                }
                if (right)
                {
                    if (CompareRoomPatern(x, z, new int[] { 5, 5, 0, 0 }))
                    {
                        //TopLeftCornerPiece
                        g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, 90f + roomLeftEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 0, 5, 5, 0 }))
                    {
                        //TopRightCornerPiece
                        g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 90f + roomRightEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 0, 5, 0, 0 }))
                    {
                        g = Instantiate(roomEntranceMiddle, pos, Quaternion.Euler(new Vector3(0f, 90f + roomMiddleEntranceOffset, 0f)));
                    }
                }
                if (bottom)
                {
                    if (CompareRoomPatern(x, z, new int[] { 0, 5, 5, 0 }))
                    {
                        //TopLeftCornerPiece
                        g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, 180f + roomLeftEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 0, 0, 5, 5 }))
                    {
                        //TopRightCornerPiece
                        g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 180f + roomRightEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 0, 0, 5, 0 }))
                    {
                        g = Instantiate(roomEntranceMiddle, pos, Quaternion.Euler(new Vector3(0f, 180f + roomMiddleEntranceOffset, 0f)));
                    }
                }
                if (left)
                {
                    if (CompareRoomPatern(x, z, new int[] { 0, 0, 5, 5 }))
                    {
                        //TopLeftCornerPiece
                        g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, 270f + roomLeftEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 5, 0, 0, 5 }))
                    {
                        //TopRightCornerPiece
                        g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 270f + roomRightEntranceOffset, 0f)));
                    }
                    else if (CompareRoomPatern(x, z, new int[] { 0, 0, 0, 5 }))
                    {
                        g = Instantiate(roomEntranceMiddle, pos, Quaternion.Euler(new Vector3(0f, 270f + roomMiddleEntranceOffset, 0f)));
                    }
                }
            }

            if (DetermineHallwayPosition(new Point(x,z)) == 2)
            {
                if (top && right)
                {
                    g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, 90f + roomLeftEntranceOffset, 0f)));
                }
                if (right && bottom)
                {
                    g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 90f + roomRightEntranceOffset, 0f)));
                }
                if (bottom && left)
                {
                    g = Instantiate(roomEntranceRight, pos, Quaternion.Euler(new Vector3(0f, 180f + roomRightEntranceOffset, 0f)));
                }
                if (left && top)
                {
                    g = Instantiate(roomEntranceLeft, pos, Quaternion.Euler(new Vector3(0f, roomLeftEntranceOffset, 0f)));
                }

            }


            dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Room, g);
        }
        else
        {
            PlaceMazePiece(x, z);
        }
    }

    /// <summary>
    /// 1. Check to Place Entrance, 
    /// 2. Check to Place Corners,
    /// 3. Check to Place Sides
    /// 4. Place Middle Pieces
    /// </summary>

    private void PlaceRoomPieces(int x, int z)
    {
        Vector3 pos = new Vector3(x * scale, scale / 2f + (level * heightOffset), z * scale);

        GameObject g = null;

        #region CornerPieces
        if (CompareRoomPatern(x, z, new int[] { 5, 0, 0, 5 }))
        {
            //TopLeftCornerPiece
            g = Instantiate(roomCorner, pos, Quaternion.Euler(new Vector3(0f, 0f + roomCornerOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 5, 5, 0, 0 }))
        {
            //TopRightCornerPiece
            g = Instantiate(roomCorner, pos, Quaternion.Euler(new Vector3(0f, 90f + roomCornerOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 0, 0, 5, 5 }))
        {
            //BottomLeftCornerPiece
            g = Instantiate(roomCorner, pos, Quaternion.Euler(new Vector3(0f, 270f + roomCornerOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 0, 5, 5, 0 }))
        {
            //BottomRightCornerPiece
            g = Instantiate(roomCorner, pos, Quaternion.Euler(new Vector3(0f, 180f + roomCornerOffset, 0f)));
        }
        #endregion
        #region Side Pieces
        else if (CompareRoomPatern(x, z, new int[] { 0, 0, 0, 5 }))
        {
            //LeftSidePiece
            g = Instantiate(roomSide, pos, Quaternion.Euler(new Vector3(0f, 270f + roomSideOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 5, 0, 0, 0 }))
        {
            //TopSidePiece
            g = Instantiate(roomSide, pos, Quaternion.Euler(new Vector3(0f, 0f + roomSideOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 0, 5, 0, 0 }))
        {
            //RightSidePiece
            g = Instantiate(roomSide, pos, Quaternion.Euler(new Vector3(0f, 90f + roomSideOffset, 0f)));
        }
        else if (CompareRoomPatern(x, z, new int[] { 0, 0, 5, 0 }))
        {
            //BottomSidePiece
            g = Instantiate(roomSide, pos, Quaternion.Euler(new Vector3(0f, 180f + roomSideOffset, 0f)));
        }
        #endregion
        else if (CompareRoomPatern(x, z, new int[] { 0, 0, 0, 0 }))
        {
            //MiddlePiece
            g = Instantiate(roomMiddle, pos, Quaternion.Euler(new Vector3(0f, 0f + roomMiddleOffset, 0f)));
        }

        dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Room, g);
    }

    //To check whether x z position which is 0 on the map is actually a room piece 
    private bool IsRoomPoint(int x, int z)
    {
        foreach (var item in roomPoints)
        {
            if(item.x == x && item.z == z)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsEntrancePoint(int x, int z)
    {
        foreach (var item in entrancePoints)
        {
            if (item.x == x && item.z == z)
            {
                return true;
            }
        }

        return false;
    }

    private void PlaceMazePiece(int x, int z)
    {
        if (map[x, z] == 0)
        {
            int n = CountNeighbouringHalls(x, z);
            Vector3 pos = new Vector3(x * scale, scale / 2f + (level * heightOffset), z * scale);

            GameObject g = null;

            //Place DeadEnd
            if (ComparePatern(x, z, new int[] { 1, 0, 1, 1 }))
            {
                g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetDeadEndPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.DeadEnd, g);
            }
            else if (ComparePatern(x, z, new int[] { 1, 1, 0, 1 }))
            {
                g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetDeadEndPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.DeadEnd, g);
            }
            else if (ComparePatern(x, z, new int[] { 1, 1, 1, 0 }))
            {
                g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetDeadEndPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.DeadEnd, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 1, 1, 1 }))
            {
                g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetDeadEndPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.DeadEnd, g);
            }

            //Determine which piece to put
            if (ComparePatern(x, z, new int[] { 1, 0, 1, 0 }))
            {
                g = Instantiate(StraightPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetStraightPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Straight, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 1, 0, 1 }))
            {
                g = Instantiate(StraightPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetStraightPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Straight, g);
            }
            else if (ComparePatern(x, z, new int[] { 1, 0, 0, 1 }))
            {
                g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetCornerPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Corner, g);
            }
            else if (ComparePatern(x, z, new int[] { 1, 1, 0, 0 }))
            {
                g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetCornerPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Corner, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 1, 1, 0 }))
            {
                g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetCornerPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Corner, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 0, 1, 1 }))
            {
                g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetCornerPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.Corner, g);
            }

            //Place TJunction
            if (ComparePatern(x, z, new int[] { 1, 0, 0, 0 }))
            {
                g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetTJuncPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.TJunction, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 1, 0, 0 }))
            {
                g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetTJuncPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.TJunction, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 0, 1, 0 }))
            {
                g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetTJuncPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.TJunction, g);
            }
            else if (ComparePatern(x, z, new int[] { 0, 0, 0, 1 }))
            {
                g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetTJuncPiece, 0f)));
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.TJunction, g);
            }

            //Place XJunction
            if (ComparePatern(x, z, new int[] { 0, 0, 0, 0 }))
            {
                g = Instantiate(XJunctionPiece, pos, Quaternion.identity);
                dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.XJunction, g);
            }
                
            
            /** Omitted this switch case as counting neighbours doesnt works well after having rooms
            switch (n)
            {
                case 1:
                    
                    dungeonInfo[x, z] = new DungeonInfo(DungeonPieces.DeadEnd, g);
                    break;
                case 2:
                   
                    break;
                case 3:
                    
                    break;
                case 4:
                    
                    break;
            }
            */
        }
    }

    //To figure out how many adjecent halls we have which are non diagonal
    public int CountNeighbouringHalls(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= size_X - 1 || z <= 0 || z >= size_Z - 1) return 5;   //To determine edge
        if (map[x - 1, z] == 0) count++;
        if (map[x + 1, z] == 0) count++;
        if (map[x, z - 1] == 0) count++;
        if (map[x, z + 1] == 0) count++;

        return count;
    }

    //To figure out how many adjecent halls we have diagonally
    public int CountDiagonalHalls(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= size_X - 1 || z <= 0 || z >= size_Z - 1) return 5;   //To determine edge
        if (map[x - 1, z - 1] == 0) count++;
        if (map[x + 1, z + 1] == 0) count++;
        if (map[x + 1, z - 1] == 0) count++;
        if (map[x - 1, z + 1] == 0) count++;

        return count;
    }

    //To figure out how many adjecent halls we have in all
    public int CountAllNeighbouringHalls(int x, int z)
    {
        return CountNeighbouringHalls(x,z) + CountDiagonalHalls(x,z);
    }

    bool ComparePatern(int x, int z,int[] patern)
    {
        int c = 0;

        if (patern.Length != 4) return false;

        if(IsRoomPoint(x, z + 1) && !IsEntrancePoint(x,z))
        {
            if (1 == patern[0]) c++;
        }
        else
        {
            if (map[x, z + 1] == patern[0]) c++;
        }
        if (IsRoomPoint(x + 1, z) && !IsEntrancePoint(x, z))
        {
            if (1 == patern[1]) c++;
        }
        else
        {
            if (map[x + 1, z] == patern[1]) c++;
        }
        if (IsRoomPoint(x, z - 1) && !IsEntrancePoint(x, z))
        {
            if (1 == patern[2]) c++;
        }
        else
        {
            if (map[x, z - 1] == patern[2]) c++;
        }
        if (IsRoomPoint(x - 1, z) && !IsEntrancePoint(x, z))
        {
            if (1 == patern[3]) c++;
        }
        else
        {
            if (map[x - 1, z] == patern[3]) c++;
        }

        //if (map[x, z + 1] == patern[0]) c++;
        //if (map[x + 1, z] == patern[1]) c++;
        //if (map[x, z - 1] == patern[2]) c++;
        //if (map[x - 1, z] == patern[3]) c++;

        if (c < 4) return false;

        return true;
    }

    bool CompareRoomPatern(int x, int z, int[] patern)
    {
        int c = 0;

        if (patern.Length != 4) return false;

        if (map[x, z + 1] == patern[0] || !IsRoomPoint(x, z + 1)) c++;
        if (map[x + 1, z] == patern[1] || !IsRoomPoint(x + 1, z)) c++;
        if (map[x, z - 1] == patern[2] || !IsRoomPoint(x, z - 1)) c++;
        if (map[x - 1, z] == patern[3] || !IsRoomPoint(x - 1, z)) c++;

        if (c < 4) return false;

        return true;
    }
}
