using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wilsons : DungeonGenerator
{
    

    List<Point> notUsed = new List<Point>();

    public override void Generate()
    {
        //create a starting cell
        int x = Random.Range(2, size_X - 1);
        int z = Random.Range(2, size_Z - 1);
        map[x, z] = 2;

        while(GetAvailableCells() > 1)
            RandomWalk();
    }

    int CountSquareMazeNeighbours(int x, int z)
    {
        int count = 0;
        for (int d = 0; d < directions.Count; d++)
        {
            int nx = x + directions[d].x;
            int nz = z + directions[d].z;
            if (map[nx, nz] == 2)
            {
                count++;
            }
        }

        return count;
    }

    int GetAvailableCells()
    {
        notUsed.Clear();
        for (int z = 1; z < size_Z - 1; z++)
            for (int x = 1; x < size_X - 1; x++)
            {
                if (CountSquareMazeNeighbours(x, z) == 0)
                {
                    notUsed.Add(new Point(x, z));
                }
            }

        return notUsed.Count;
    }

    void RandomWalk()
    {
        List<Point> inWalk = new List<Point>();
        int cx;
        int cz;
        int rstartIndex = Random.Range(0, notUsed.Count);

        cx = notUsed[rstartIndex].x;
        cz = notUsed[rstartIndex].z;

        inWalk.Add(new Point(cx, cz));

        int loop = 0;
        bool validPath = false;
        while (cx > 0 && cx < size_X - 1 && cz > 0 && cz < size_Z - 1 && loop < 5000 && !validPath)
        {
            map[cx, cz] = 0;
            if (CountSquareMazeNeighbours(cx, cz) > 1)
                break;

            int rd = Random.Range(0, directions.Count);
            int nx = cx + directions[rd].x;
            int nz = cz + directions[rd].z;
            if (CountNeighbouringHalls(nx, nz) < 2)
            {
                cx = nx;
                cz = nz;
                inWalk.Add(new Point(cx, cz));
            }

            validPath = CountSquareMazeNeighbours(cx, cz) == 1;

            loop++;
        }

        if (validPath)
        {
            map[cx, cz] = 0;
            Debug.Log("PathFound");

            foreach (Point m in inWalk)
            {
                map[m.x, m.z] = 2;
            }
            inWalk.Clear();
        }
        else
        {
            foreach (Point m in inWalk)
                map[m.x, m.z] = 1;

            inWalk.Clear();
        }

    }

}
