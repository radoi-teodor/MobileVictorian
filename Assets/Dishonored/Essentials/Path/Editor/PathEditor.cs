using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor {

    PathCreator creator;
    Path path;

    const float segmentSelectDistance = .1f; // mouse offset select segment
    int selectedSegmentindex = -1; // selected segment index

    bool showingHelpers = true; // if we must see the helpers

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck(); // monitorize change in scene

        if (GUILayout.Button("Create New")) // button
        {
            Undo.RecordObject(creator, "Created New Path"); // undo
            creator.CreatePath(); // create a new path
            path = creator.path; // reassign path
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Is Closed?"); // toggle button
        if (isClosed != path.IsClosed) // button
        {
            Undo.RecordObject(creator, "Toggle Closed"); // undo
            path.IsClosed = isClosed; // trigger IsClosed setter
        }

        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points"); // toggle button
        if (autoSetControlPoints != path.AutoSetControlPoints) // button
        {
            Undo.RecordObject(creator, "Toggle Auto set controls"); // undo
            path.AutoSetControlPoints = autoSetControlPoints; // set the auto set bool
        }

        EditorGUILayout.Space();

        showingHelpers = GUILayout.Toggle(showingHelpers, "Showing helpers?"); // toggle showingHelpers bool

        if (EditorGUI.EndChangeCheck()) // if change in scene
        {
            SceneView.RepaintAll(); // repaint scene
        }
    }

    private void OnSceneGUI()
    {
        Input(); // input for points
        Draw(); // draw points and curves
    }

    void Input()
    {
        Event guiEvent = Event.current; // input event
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition); // we save the ray
        Vector2 mousePos = mouseRay.origin;
        // convert mouseposition to a ray and get the origin

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) // if mouse down and holding shift
        {
            if(selectedSegmentindex != -1)
            {
                Undo.RecordObject(creator, "Split segment"); // save state of object for undo action

                float minDistToSegment = segmentSelectDistance; // min distance to touch the segment

                Vector3[] points = path.CalculateEvenlySpacedPointsCurve(selectedSegmentindex); // get the points from specific curve
                Vector3 pointPos = points[0]; // we suppose we have the mouse near fisrt point of the segment

                float dst = path.DistanceToLine(mouseRay, points[0]); // we calculate the distance from mouse ray to that point

                for (int j = 1; j < points.Length; j++) // iterate through points
                {
                    float temp_dst = path.DistanceToLine(mouseRay, points[j]); // we calculate the distance from mouse ray to that point
                    if (temp_dst < dst) // if the distance is less than last distance
                    {
                        dst = temp_dst; // we save the distance
                        pointPos = points[j]; // we save the point
                    }

                    if (dst <= minDistToSegment) // if the distance is less than  distance threeshold
                    {
                        break; // break the loop for performance
                    }
                }

                if (dst <= minDistToSegment) // if the distance is less than  distance threeshold
                {
                    minDistToSegment = dst; // save the record
                    path.SplitSegment(pointPos, selectedSegmentindex); // split the segment
                }

            }
            else if(!path.IsClosed)
            {
                Undo.RecordObject(creator, "Add segment"); // save state of object for undo action
                path.AddSegment(path[path.NumPoints - 1] + (path[path.NumPoints-1] - path[path.NumPoints - 2]).normalized); // add a segment forward of the last point
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1) // if mouse down (right button) and holding shift
        {
            float minDstToAnchor = .05f; // min dist to anchor
            int closestAnchorIndex = -1; // variable that holds the selected anchor index

            for (int i = 0; i < path.NumPoints; i++) // iterate through points
            {
                float dst = path.DistanceToLine(mouseRay, path[i]); // calculate distance between mouse ray and 3D point

                if (dst < minDstToAnchor) // if it is close to the anchor
                {
                    minDstToAnchor = dst; // save the distance
                    closestAnchorIndex = i; // save the anchor
                }

            }

            if(closestAnchorIndex != -1) // if found any anchor
            {
                Undo.RecordObject(creator, "Delete segment"); // for undo
                path.DeleteSegment(closestAnchorIndex); // delete segment from index
            }
        }

        if (guiEvent.type == EventType.MouseMove) // if we move the mouse
        {
            float minDistToSegment = segmentSelectDistance; // min distance to touch the segment
            int newSelectedSegmentIndex = -1; // variable to save the selected segment index

            for (int i = 0; i < path.NumSegments; i++) // iterate through curves
            {

                Vector3[] points = path.CalculateEvenlySpacedPointsCurve(i); // calculate points from curve

                float dst = path.DistanceToLine(mouseRay, points[0]); // distance from mouse ray to first point of the curve

                for (int j = 1; j < points.Length; j++) // iterate throught curve's points
                {
                    float temp_dst = path.DistanceToLine(mouseRay, points[j]); // calculate the distance from the new point to ray
                    if (temp_dst < dst) // if it is less than last distance
                    {
                        dst = temp_dst; // save the new distance
                    }

                    if(dst <= minDistToSegment) // if it is less than threeshold
                    {
                        break; // break for performance
                    }
                }

                if(dst <= minDistToSegment) // if it is less than threeshold
                {
                    minDistToSegment = dst; // save the record
                    newSelectedSegmentIndex = i; // save the curve index
                }

            }

            if(newSelectedSegmentIndex != selectedSegmentindex)  // if the last selected curve index is not equal to
                // now selected curve
            {
                selectedSegmentindex = newSelectedSegmentIndex; // save the new selected curve
                HandleUtility.Repaint(); // repaint the scene gui (mouse move event does not automatically repaint)
            }
        }
    }

    void Draw()
    {

        for (int i = 0; i < path.NumSegments; i++) // iterate through all segments
        {
            Vector3[] points = path.GetPointsInSegment(i); // get the 'i' segment

            if (!path.AutoSetControlPoints && showingHelpers) // if not auto set helper and showing helpers
            {

                Handles.color = Color.black; // set the segment color - not selected

                Handles.DrawLine(points[1], points[0]); // draw line from first anchor to its helper
                Handles.DrawLine(points[2], points[3]); // draw line from second anchor to its helper
            }

            if (i == selectedSegmentindex && Event.current.shift) // change selected curve color
            {
                // SELECTED
                Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.yellow, null, 2); // draw curve from point 3 to point 1 (local) with the tangent
                // of the helpers
            }
            else
            {
                // NOT SELECTED
                Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2); // draw curve from point 3 to point 1 (local) with the tangent
                // of the helpers

            }
        }

        for (int i = 0; i < path.NumPoints; i++) // iterate through points
        {
            Vector3 newPos = Vector3.zero, newPos2 = Vector3.zero;
            bool isAnchor = false;

            if (i % 3 == 0)
            {
                Handles.color = Color.red; // set the handle color again
                newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .1f, Vector3.zero, Handles.CylinderHandleCap);
                // create a free to move handle in the shape of cylinder and save its position
                newPos2 = Handles.PositionHandle(path[i], Quaternion.identity);
                isAnchor = true;
            }
            else
            {
                if (!path.AutoSetControlPoints && showingHelpers)
                {
                    Handles.color = Color.white; // set the handle color again
                    newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .075f, Vector3.zero, Handles.CylinderHandleCap);
                    // create a free to move handle in the shape of cylinder and save its position
                }
            }

            if (i % 3 == 0 || (!path.AutoSetControlPoints && showingHelpers))
            {
                if (path[i] != newPos || (path[i] != newPos2 && isAnchor))
                {
                    Undo.RecordObject(creator, "Move point"); // save state of object for undo action
                }

                if (path[i] != newPos) // if handle point != point pos
                {
                    path.MovePoint(i, newPos); // move point
                }

                if (path[i] != newPos2 && isAnchor) // if handle point != point pos
                {
                    path.MovePoint(i, newPos2); // move point
                }
            }
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator)target; // get the object from scene

        if(creator.path == null) // if creator's path is null
        {
            creator.CreatePath(); // create the path
        }
        path = creator.path; // save the path in the current object
    }

}
