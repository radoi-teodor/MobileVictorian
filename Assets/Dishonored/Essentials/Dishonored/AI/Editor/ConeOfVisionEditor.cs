using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseAI))]
public class ConeOfVisionEditor : Editor {

    private void OnSceneGUI()
    {
        BaseAI bai = (BaseAI)target;
        Handles.color = Color.red;
        Handles.DrawWireArc(bai.transform.position, Vector3.up, Vector3.forward, 360, bai.range);

        Vector3 viewingAngleA = bai.DirFromAngle(-bai.angle / 2, false);
        Vector3 viewingAngleB = bai.DirFromAngle(bai.angle / 2, false);

        Handles.DrawLine(bai.transform.position, bai.transform.position + viewingAngleA * bai.range);
        Handles.DrawLine(bai.transform.position, bai.transform.position + viewingAngleB * bai.range);
    }
}
