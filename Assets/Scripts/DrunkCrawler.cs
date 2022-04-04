using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrunkCrawler : DungeonGenerator
{
    bool isDone = false;

    bool generateVertical = false;

    [SerializeField] int verticalIterations = 1, horizontalIterations = 1;

    int x, z;

    void Init_X_Z(bool isVertical)
    {
        x = isVertical ? Random.Range(1, size_X-1) : 1;
        z = isVertical ? 1 : Random.Range(1, size_Z-1);
    }

    public override void Generate()
    {
        int totalIterations = verticalIterations + horizontalIterations;
        int completedIterations = 0;

        if (totalIterations < 1) return;

        if(horizontalIterations < 1)
        {
            Init_X_Z(true);
        }
        else
        {
            Init_X_Z(false);
        }

        SetPosition(x, z);

        while (!isDone)
        {
            map[x, z] = 0;

            if(Random.Range(0,10) < 5)
            {
                if (Random.Range(0, 10) < 5)
                {
                    x += generateVertical ? -1 : 0;
                }
                else
                {
                    x += 1;
                }
            }
            else
            {
                if (Random.Range(0, 10) < 5)
                {
                    z += generateVertical ? 0 : -1;
                }
                else
                {
                    z += 1;
                }
            }

            //Simple X OR Operation 
            isDone |= (x < 1 || x == size_X - 1 || z < 1 || z == size_Z -1);

            if (isDone && ++completedIterations < totalIterations)
            {
                isDone = false;
                if(completedIterations < horizontalIterations)
                {
                    Init_X_Z(false);
                }
                else
                {
                    Init_X_Z(true);
                    generateVertical = true;
                }
            }
        }
    }
}
