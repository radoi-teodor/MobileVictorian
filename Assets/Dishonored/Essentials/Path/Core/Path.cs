using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path {

    [SerializeField, HideInInspector]
    List<Vector3> points;

    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControlPoints;

    // order: anchor 1, helper 1.1 helper 2.1 anchor 2 helper 2.2 helper 3.1 anchor 3 etc.

    public Path(Vector3 center) // constructor
    {
        points = new List<Vector3>
        {
            center+Vector3.left, // left point
            center+(Vector3.left+Vector3.up)*.5f, // left up helper point
            center+(Vector3.right + Vector3.down)*.5f, // right down helper point
            center+Vector3.right // right point
        };
    }

    public Vector3 this[int i] // access points elements with this class
    {
        get // getter
        {
            return points[i]; // return point
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }

        set
        {
            if(isClosed != value)
            {
                isClosed = value;

                // after change
                if (isClosed) // if is closed
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]); // add last point second helper
                    points.Add(points[0] * 2 - points[1]); // add first point second helper

                    if (autoSetControlPoints) // if must auto set
                    {
                        AutoSetAnchorControlPoints(0); // sets the point that used to be the first
                        AutoSetAnchorControlPoints(points.Count - 3); // sets the point that used to be the last
                    }
                }
                else // if is not closed
                {
                    points.RemoveRange(points.Count - 2, 2); // remove helpers from close sequence

                    if (autoSetControlPoints) // if must auto set
                    {
                        AutoSetStartAndEndControls(); // autoset start and end
                    }
                }
            }
        }
    }
    public bool AutoSetControlPoints
    {
        get // returns auto set bool
        {
            return autoSetControlPoints;
        }

        set // sets auto set bool
        {
            if(autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints) // if must auto set
                {
                    AutoSetAllControlPoints(); // auto set
                }
            }
        }
    }


    public int NumPoints // getter to return the number of the points (all of them)
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments // getter to return the number of the curves/segments
    {
        get
        {
            return points.Count / 3; // will return +1 if closed :D - quick math
        }
    }

    public void AddSegment(Vector3 anchor) // add segment; anchor = new point
    {

        points.Add(points[points.Count - 1] - (points[points.Count - 2] - points[points.Count - 1])); // add last point second helper
        points.Add((points[points.Count - 1] + anchor) / 2); // add helper for the new point (average position between last helper and the new point)
        points.Add(anchor); // add the actual point position

        if (autoSetControlPoints) // if must auto set
        {
            AutoSetAllAffectedControlPoints(points.Count - 1); // auto set created point
        }

    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero }); // adds the points on the segment specified
        // after the start point from the segment

        if (autoSetControlPoints) // if needs to auto set
        {
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3); // auto set created anchor and affected point
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3); // else, auto set only created anchor
        }
    }

    public Vector3[] GetPointsInSegment(int i) // get points from curve index
    {
        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i]; // delta vector (how much we moved the point)

        if (i % 3 == 0 || !autoSetControlPoints) // if has autocontrolpoints, helpers position is automatic (not allow to change manual)
        {
            points[i] = pos; // set the point new position

            if (autoSetControlPoints) // if must auto set
            {
                AutoSetAllAffectedControlPoints(i); // auto set affected points of the moved point
            }
            else
            {

                if (i % 3 == 0) // if it is an anchor point
                {
                    if (i + 1 < points.Count || isClosed) // if does have a second helper
                        points[LoopIndex(i + 1)] += deltaMove; // move the second helper

                    if (i - 1 >= 0 || isClosed) // if does have a first helper
                        points[LoopIndex(i - 1)] += deltaMove; // move the first helper
                }
                else // if the point is helper
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0; // if the next point in list ios an anchor
                    int correspondingIndex = (nextPointIsAnchor) ? i + 2 : i - 2; // if this is the first helper, we need to work with the second helper
                                                                                  // else we need to substract 2 and to work with the first helper
                    int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1; // get the helper's anchor index

                    if ((correspondingIndex >= 0 && correspondingIndex < points.Count) || isClosed) // if the other helper exists (is in list)
                    {
                        float dist = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingIndex)]).magnitude; // distance from anchor to the other point
                        Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized; // opposite direction from the anchor to the first helper moved
                        points[LoopIndex(correspondingIndex)] = points[LoopIndex(anchorIndex)] + dir * dist; // recalculate second helper position
                    }
                }
            }
        }
    }

    public float BezierLength(int index) // calculate the aproximate bezier length
    {
        float length = 0; // base length

        float t = 0.1f; // we do exactly like in evenly calculations of the points in bezier

        Vector3[] ps = GetPointsInSegment(index); // get the points in segment

        Vector3 lastPointPos = ps[0]; // memorize the last point

        while (t < 1f) // while t from lerp is less than 1
        {
            Vector3 temp_p = Bezier.EvaluateCubic(ps[0], ps[1], ps[2], ps[3], t); // calculate the lerp by t

            length += (temp_p - lastPointPos).magnitude; // we add the length between last point and actual point
            lastPointPos = temp_p; // the actual point is the new last point

            t += .05f; // we go to the next t (next point)
        }

        return length; // we return the length
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing = 1) // return evenly spaced points as an array
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>(); // list to store points
        evenlySpacedPoints.Add(points[0]); // add the start point

        for (int i = 0; i < NumSegments; i++) // iterate through segments
        {
            float t = 0f, curveLength = BezierLength(i); // t is the progress of the lerp through curve
            // we save the segment curve

            Vector3[] ps = GetPointsInSegment(i); // get the points in segment

            while (t < 1f) // while t from lerp is less than 1
            {
                Vector3 temp_p = Bezier.EvaluateCubic(ps[0], ps[1], ps[2], ps[3], t); // calculate the lerp by t
                evenlySpacedPoints.Add(temp_p); // add the point in the points array

                t += (.1f * spacing) / curveLength; // we progrees further by adding spacing and
                                                    // we make sure that the points are placed independent from curve length
            }
        }

        return evenlySpacedPoints.ToArray(); // return array
    }

    public Vector3[] CalculateEvenlySpacedPointsCurve(int curveIndex, float spacing = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>(); // list to store points

        float t = 0f, curveLength = BezierLength(curveIndex); // t is the progress of the lerp through curve

        Vector3[] ps = GetPointsInSegment(curveIndex); // get the points in segment

        evenlySpacedPoints.Add(ps[0]); // add the start point

        while (t < 1f) // while t from lerp is less than 1
        {
            Vector3 temp_p = Bezier.EvaluateCubic(ps[0], ps[1], ps[2], ps[3], t); // calculate the lerp by t
            evenlySpacedPoints.Add(temp_p);

            t += (.1f * spacing) / curveLength; // we progrees further by adding spacing and
                                                // we make sure that the points are placed independent from curve length
        }

        return evenlySpacedPoints.ToArray(); // return array

    }

    public float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    int LoopIndex(int i) // return index if greater than count or negative, like in a circle
    {
        return (i + points.Count) % points.Count;
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex]; // anchor position
        Vector3 dir = Vector3.zero; // base direction
        float[] neighbourDistances = new float[2]; // anchor's neighbour distance array

        if (anchorIndex - 3 >= 0 || isClosed) // if closed or neighbour exists
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos; // direction from anchor to one neighbour
            dir += offset.normalized; // direction += normalized direction from anchor to one neighbour
            neighbourDistances[0] = offset.magnitude; // assign neighbour distance in array
        }

        if (anchorIndex + 3 >= 0 || isClosed) // if closed or neighbour exists
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos; // direction from anchor to the other neighbour
            dir -= offset.normalized; // direction -= normalized direction from anchor to one neighbour
            neighbourDistances[1] = -offset.magnitude; // assign negative neighbour distance in array
        }

        dir.Normalize(); // normalize resulting distance

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1; // gets helper's indexes of anchor and anchor's neighbour
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed) // if index exists or it's closed
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f; // sets helper's position
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * .5f; // set the first point based on helpers
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f; // set last point based on helpers
        }
    }

    void AutoSetAllControlPoints() // sets all points
    {
        for (int i = 0; i < points.Count; i += 3) // iterate throught all anchors
        {
            AutoSetAnchorControlPoints(i); // and sets the positions
        }

        AutoSetStartAndEndControls(); // sets start and end nodes
    }

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex) // sets affected points
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3) // iterate through 6 neighbours and
        {
            if (i >= 0 && i < points.Count || isClosed) // verify if exists or is closed
            {
                AutoSetAnchorControlPoints(LoopIndex(i)); // sets the positions
            }
        }

        AutoSetStartAndEndControls(); // sets start and end nodes
    }
    
    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || (!isClosed && NumSegments > 1)) // if is closed (min 2 segments) or has at least 2 segments
        {
            if (anchorIndex == 0) // if we want to delete first anchor
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2]; // second helper of the first point will be set as the second helper of the second anchor
                    // because second anchor will be the first anchor after deleting
                }
                points.RemoveRange(0, 3); // then delete fisrt anchor, its helper, and the old second anchor of the second anchor
            }
            else if (anchorIndex == points.Count - 1) // if it is the last anchor that we want to delete
            {
                points.RemoveRange(anchorIndex - 2, 3); // remove the second helper of the ante-last anchor, the helper of the last anchor and the last anchor
                // the ante-last anchor will become the last anchor
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3); // else remove the point as usual (him and his helpers)
            }

            // auto set sequence
            if (autoSetControlPoints)
            {
                AutoSetAllControlPoints();
            }
            else
            {
                AutoSetAllAffectedControlPoints(anchorIndex);
            }
        }
    }
}
