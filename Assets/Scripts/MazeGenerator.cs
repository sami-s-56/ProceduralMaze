using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TunnelPieces
{
    Straight,
    TJunction,
    XJunction,
    Corner,
    DeadEnd,
    StraightStairs,
    DeadendStairs
}

[System.Serializable]
public struct PieceInfo
{
    public TunnelPieces tunnelPiece;
    public GameObject pieceModel;

    public PieceInfo(TunnelPieces p, GameObject m)
    {
        tunnelPiece = p;
        pieceModel = m;
    }
}

public class MazeGenerator : MonoBehaviour
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
    public int size_X = 10, size_Z = 10;

    [SerializeField]
    protected byte[,] map;

    public PieceInfo[,] pieceMap;

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
    
    public int level = 0;
    public int heightOffset = 1;



    //To get the position of hall where player should spawn
    protected void SetPosition(int x, int z)
    {
        playerPos = new Vector3(x * scale, 5f, z * scale);
    }

    // Start is called before the first frame update
    public void BuildMaze()
    {
        InitializeMap();
        Generate(Random.Range(1, size_X), Random.Range(1, size_Z));
        DrawMap();
        PlacePlayer(playerPos);
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
    private void Generate(int x, int z)
    {
        //for (int x = 0; x < size_X; x++)
        //{
        //    for (int z = 0; z < size_Z; z++)
        //    {
        //        if (UnityEngine.Random.Range(0, 2) == 0)
        //        {
        //            map[x, z] = 0;
        //        }
        //    }
        //}

        if (CountNeighbouringHalls(x, z) >= 2) return;

        map[x, z] = 0;

        SetPosition(x, z);

        ShuffleList();

        Generate(x + directions[0].x, z + directions[0].z);
        Generate(x + directions[1].x, z + directions[1].z);
        Generate(x + directions[2].x, z + directions[2].z);
        Generate(x + directions[3].x, z + directions[3].z);
    }

    void ShuffleList()
    {
        List<Point> tempList = new List<Point>(directions);

        for (int i = 0; i < directions.Count; i++)
        {
            int t = Random.Range(0, tempList.Count);
            directions[i] = tempList[t];
            tempList.RemoveAt(t);
        }
    }

    //Placing cubes according to map
    private void DrawMap()
    {
        pieceMap = new PieceInfo[size_X, size_Z];

        for (int x = 0; x < size_X; x++)
        {
            for (int z = 0; z < size_Z; z++)
            {
                /** Place Cubes for walls */
                //if (map[x, z] == 1)
                //{
                //    Vector3 pos = new Vector3(x * scale, scale / 2f + (level * heightOffset), z * scale);
                //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    cube.transform.position = pos;
                //    cube.transform.localScale = new Vector3(scale, scale, scale);
                //    cube.name = "Cube_" + z.ToString() + "_" + x.ToString();
                //}

                //Point p = new Point(x, z);

                /** Place appropriate maze piece */
                
                PlaceMazePiece(x, z);
                
            }
        }
    }

    private void PlaceMazePiece(int x, int z)
    {
        if (map[x, z] == 0)
        {
            int n = CountNeighbouringHalls(x, z);
            Vector3 pos = new Vector3(x * scale, scale / 2f + (level * heightOffset), z * scale);
            GameObject g = null;
            switch (n)
            {
                case 1:
                    //Place DeadEnd
                    if (ComparePatern(x, z, new int[] { 1, 0, 1, 1 }))
                    {
                        g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetDeadEndPiece, 0f)));
                    }
                    else if (ComparePatern(x, z, new int[] { 1, 1, 0, 1 }))
                    {
                        g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetDeadEndPiece, 0f)));
                    }
                    else if (ComparePatern(x, z, new int[] { 1, 1, 1, 0 }))
                    {
                        g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetDeadEndPiece, 0f)));
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 1, 1, 1 }))
                    {
                        g = Instantiate(DeadEndPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetDeadEndPiece, 0f)));
                    }
                    pieceMap[x, z] = new PieceInfo(TunnelPieces.DeadEnd, g);
                    //Instantiate(DeadEndPiece, pos, Quaternion.identity);
                    break;
                case 2:
                    //Determine which piece to put
                    if (ComparePatern(x, z, new int[] { 1, 0, 1, 0 }))
                    {
                        g = Instantiate(StraightPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetStraightPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Straight, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 1, 0, 1 }))
                    {
                        g = Instantiate(StraightPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetStraightPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Straight, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 1, 0, 0, 1 }))
                    {
                        g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetCornerPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Corner, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 1, 1, 0, 0 }))
                    {
                        g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetCornerPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Corner, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 1, 1, 0 }))
                    {
                        g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetCornerPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Corner, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 0, 1, 1 }))
                    {
                        g = Instantiate(CornerPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetCornerPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.Corner, g);
                    }
                    break;
                case 3:
                    if (ComparePatern(x, z, new int[] { 1, 0, 0, 0 }))
                    {
                        g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 0f + offsetTJuncPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.TJunction, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 1, 0, 0 }))
                    {
                        g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 90f + offsetTJuncPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.TJunction, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 0, 1, 0 }))
                    {
                        g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 180f + offsetTJuncPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.TJunction, g);
                    }
                    else if (ComparePatern(x, z, new int[] { 0, 0, 0, 1 }))
                    {
                        g = Instantiate(TJunctionPiece, pos, Quaternion.Euler(new Vector3(0f, 270f + offsetTJuncPiece, 0f)));
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.TJunction, g);
                    }
                    break;
                case 4:
                    if (ComparePatern(x, z, new int[] { 0, 0, 0, 0 }))
                    {
                        g = Instantiate(XJunctionPiece, pos, Quaternion.identity);
                        pieceMap[x, z] = new PieceInfo(TunnelPieces.XJunction, g);
                    }
                    break;
            }
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
        return CountNeighbouringHalls(x, z) + CountDiagonalHalls(x, z);
    }

    bool ComparePatern(int x, int z, int[] patern)
    {
        int c = 0;

        if (patern.Length != 4) return false;

        if (map[x, z + 1] == patern[0]) c++;
        if (map[x + 1, z] == patern[1]) c++;
        if (map[x, z - 1] == patern[2]) c++;
        if (map[x - 1, z] == patern[3]) c++;

        if (c < 4) return false;

        return true;
    }

}
