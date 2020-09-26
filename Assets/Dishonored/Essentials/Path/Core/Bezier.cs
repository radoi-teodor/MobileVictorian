using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier {

	public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) // calculate linear interpolation between 3 vectors
    {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0,p1,t);
    }

    public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) // calculate linear interpolation between 4 vectors
    {
        Vector3 p0 = EvaluateQuadratic(a, b, c, t);
        Vector3 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector3.Lerp(p0,p1,t);
    }

    public static Vector3 CircleTransform(Vector3 center, float value, float radius) // calculate circle arrangment positions
    {
        
        float ang = value;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;

        return pos;
    }

}
