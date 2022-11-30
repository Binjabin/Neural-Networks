using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPoint : MonoBehaviour
{
    public EdgeCollider2D coll;
    Vector2[] points;
    
    int lastPoint;

    void Awake()
    {
        coll = GetComponent<EdgeCollider2D>();
        points = coll.points;
        lastPoint = points.Length - 1;
    }

    // Update is called once per frame
    public int GetProgress(Vector3 carPos, int mostRecentPoint)
    {
        Vector2 closestPoint = coll.ClosestPoint(carPos);
        Debug.DrawLine(carPos, closestPoint, Color.yellow);

        int outIndex = GetClosestPoint(closestPoint);
        Vector3 outPos = points[outIndex] + new Vector2(transform.position.x, transform.position.y);
        Debug.DrawLine(carPos, outPos, Color.blue);
        Debug.Log(outIndex + " >>> " + mostRecentPoint);
        if (outIndex > mostRecentPoint)
        {

            if (outIndex - mostRecentPoint > 3)
            {
                //prevent cheating AIs
                Debug.Log("Skipped more than 3 steps!");
                return mostRecentPoint;
                
            }
            else
            {
                return outIndex;
            }
            
        }
        //check if you were near the end last check, and is now near the begining
        else if (mostRecentPoint > lastPoint - 3 && outIndex < 3)
        {
            return outIndex;
        }
        else
        {
            //Debug.Log("Haven't progressed. Closest is " + outIndex + " and furthest is " + mostRecentPoint + "/" + lastPoint);
            return mostRecentPoint;
        }

    }

    public int InitProgress(Vector3 carPos)
    {
        Vector2 closestPoint = coll.ClosestPoint(carPos);
        Debug.DrawLine(carPos, closestPoint, Color.yellow);
        int outIndex = GetClosestPoint(closestPoint);

        return outIndex;

    }

    public int GetClosestPoint(Vector2 pointAlongLine)
    {
        float dist = Mathf.Infinity;
        Vector2 currentPoint = Vector2.zero;
        int outIndex = 0;
        int index = 0;
        Vector2 worldSpacePointAlongLine = pointAlongLine;

        foreach (Vector2 point in points)
        {
            Vector2 worldSpacePos = point + new Vector2(transform.position.x, transform.position.y);
            if (Vector2.Distance(pointAlongLine, worldSpacePos) < dist)
            {
                dist = Vector2.Distance(pointAlongLine, worldSpacePos);
                currentPoint = point;
                outIndex = index;
            }
            index++;
        }
        Debug.DrawLine(worldSpacePointAlongLine, currentPoint + new Vector2(transform.position.x, transform.position.y), Color.green);
        return outIndex;
    }

    public int GetSecondClosest(int closestIndex, Vector2 closestPoint)
    {
        int option1 = 0;
        int option2 = 0;

        if(closestIndex == 0)
        {
            option1 = lastPoint;
            option2 = 1;
        }
        else if (closestIndex == lastPoint)
        {
            option1 = lastPoint - 1;
            option2 = 0;
        }
        else
        {
            option1 = closestIndex - 1;
            option2 = closestIndex + 1;
        }
        
        float distanceToOption1 = Vector2.Distance(points[option1], closestPoint);
        float distanceToOption2 = Vector2.Distance(points[option2], closestPoint);

        if (Vector2.Distance(points[option1], points[closestIndex]) < Vector2.Distance(points[option2], points[closestIndex]))
        {

        }
        else if (Vector2.Distance(points[option1], points[closestIndex]) > Vector2.Distance(points[option2], points[closestIndex]))
        {

        }
        return 0;
    }
}
