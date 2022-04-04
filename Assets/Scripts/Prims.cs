using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prims : DungeonGenerator
{
    [SerializeField]
    [Range(1,10)]
    int noOfIterations = 1; // ? To use 

    Point selectedPoint, previousPoint;

    bool isDone = false;

    bool generateVertical = false;

    public override void Generate()
    {
        for(int c = 0; c < noOfIterations; c++)
        {
            selectedPoint = new Point(Random.Range(1, size_X - 1), Random.Range(1, size_Z - 1));
            previousPoint = selectedPoint;

            map[selectedPoint.x, selectedPoint.z] = 0;
        
            while (!isDone)
            {
                if (Random.Range(0, 10) < 5)
                {
                    selectedPoint.x += Random.Range(-1, 2);
                }
                else
                {
                    selectedPoint.z += Random.Range(-1, 2);
                }

                if(CountNeighbouringHalls(selectedPoint.x, selectedPoint.z) == 1)
                {
                    map[selectedPoint.x, selectedPoint.z] = 0;
                    previousPoint = selectedPoint;
                }
                else
                {
                    selectedPoint = previousPoint;
                }

                isDone |= (selectedPoint.x < 1 || selectedPoint.x == size_X - 1 || selectedPoint.z < 1 || selectedPoint.z == size_Z - 1);
            }

            isDone = false;
        }
    }
}
