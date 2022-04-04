using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecursiveDungeon : DungeonGenerator
{
    public override void Generate()
    {
        Generate(Random.Range(1, size_X), Random.Range(1, size_Z));
    }

    void Generate(int x, int z)
    {
        if (CountNeighbouringHalls(x,z) >= 2) return;

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

        for(int i = 0; i < directions.Count; i++)
        {
            int t = Random.Range(0, tempList.Count);
            directions[i] = tempList[t];
            tempList.RemoveAt(t);
        }
    }
}
