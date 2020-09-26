using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode] // execute in edit mode for list member update
public class PathFollower : MonoBehaviour {

    /// <summary>
    /// public entities
    /// </summary>
    public PathCreator path; // path of the game object

    [System.Serializable] // save on exit
    public class Event : UnityEvent // unity event class for start and end of a curve
    {
    }

    [System.Serializable] // save on exit
    public struct PathStruct // struct for each curve
    {
        public Event startEvents; // event that triggers when you start walking on that curve
        public Event endEvents; // event when you stop walking from the curve (before the delay)

        [HideInInspector]
        public bool startEventsInvoked, endEventsInvoked; // bools to verify if we invoked the events

        [Space]
        public float delay; // if we want delay after we stop walking from curve
        public float speed; // spped to walk on the curve

        public PathStruct(float localDelay, float localSpeed): this() // constructor to set delay and speed
        {
            delay = localDelay;
            speed = localSpeed;
        }
    }

    public List<PathStruct> paths = new List<PathStruct>(); // list to memorize curves' structs

    /// <summary>
    /// private entities
    /// </summary>

    int pathIndex = 0; // current curve index
    bool waiting = false; // if we wait for a delay

    Vector3[] helperPoints = null; // array for the curve points

    float t = 0; // float for linear interpolation

    Rigidbody rb; // rigidbody for enabling kinematic and disabling use gravity

    // Use this for initialization
    void Start () {

        if (path == null)
        {
            return;
        }

        if (Application.isPlaying) // if we are in playing mode
        {

            StartCoroutine(wait(paths[pathIndex].delay)); // wait the first curve's delay
            helperPoints = path.path.GetPointsInSegment(pathIndex); // get the curve's points

            transform.position = helperPoints[0]; // we set the position of the gameobject to the first curve point

            for (int i = 0; i < paths.Count; i++) // we iterate through all curves
            {
                PathStruct temp_path = paths[i]; // save path in a local var
                temp_path.startEventsInvoked = false; // set bools as false
                temp_path.endEventsInvoked = false; // set bools as false

                paths[i] = temp_path; // we set the new path object in the list
            }

            rb = GetComponent<Rigidbody>(); // get the rigidbody
            if (rb)
            {
                rb.isKinematic = true; // is kinematic - true
                rb.useGravity = false; // use gravity - false
            }
        }
	}
	
	// Update is called once per frame
	void Update () {

        if(path == null)
        {
            return;
        }

        if (!Application.isPlaying) // if we are not in playing mode
        {
            if (paths.Count != path.path.NumSegments) // if the list members are not equal with the path's segments
            {

                int dif = Mathf.Abs(path.path.NumSegments - paths.Count); // we get the difference

                if (path.path.NumSegments > paths.Count) // if we have less object than the segments
                {
                    for (int i = 0; i < dif; i++)
                    {
                        paths.Add(new PathStruct(0, 1)); // we add until the segments are equal to the count of objects
                    }
                }
                else // if we have more object than the segments
                {
                    for (int i = path.path.NumSegments + dif - 1; i >= path.path.NumSegments; i--)
                    {
                        PathStruct pathStruct = paths[i];
                        paths.Remove(pathStruct); // we remove the objects until they are equal to the segment count
                    }
                }

                }
        }

        if (Application.isPlaying) // if we are in play mode
        {
            if (!waiting) // if we are not waiting
            {

                if (!paths[pathIndex].startEventsInvoked) // if we have not invoked the start event
                {
                    PathStruct temp_path = paths[pathIndex];
                    temp_path.startEventsInvoked = true;
                    paths[pathIndex] = temp_path; // we set the bool to true
                    paths[pathIndex].startEvents.Invoke(); // and we invoke the event
                }

                if ((transform.position - helperPoints[3]).magnitude > .1f) // if we are not at the las point of the curve
                {
                    transform.position = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                    // we move a little closer to the last point of the curve
                    t += Time.deltaTime * paths[pathIndex].speed; // we increase the t constant

                    Vector3 lookPos = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                    // we set the look position on the next point in the curve
                    transform.LookAt(new Vector3(lookPos.x, transform.position.y, lookPos.z));
                    // we look at the point
                }
                else // if we are at the last point
                {
                    t = 0; // we set the t constant to 0

                    if (!paths[pathIndex].endEventsInvoked) // if we have not invoked the end event
                    {
                        PathStruct temp_path = paths[pathIndex];
                        temp_path.endEventsInvoked = true;
                        paths[pathIndex] = temp_path; // we set the bool to true
                        paths[pathIndex].endEvents.Invoke(); // and we invoke the event
                    }

                    if (pathIndex < paths.Count - 1) // if we were not at the last curve
                    {
                        pathIndex++; // we increase the curve index
                        helperPoints = path.path.GetPointsInSegment(pathIndex); // we set the new curve points
                        StartCoroutine(wait(paths[pathIndex].delay)); // we wait the new curve's delay
                    }
                    else // if we were on the last curve
                    {
                        if (path.path.IsClosed) // and the path is closed
                        {
                            for (int i = 0; i < paths.Count; i++) // we iterate through all curves
                            {
                                PathStruct temp_path = paths[i]; // save path in a local var
                                temp_path.startEventsInvoked = false; // set bools as false
                                temp_path.endEventsInvoked = false; // set bools as false

                                paths[i] = temp_path; // we set the new path object in the list
                            }

                            pathIndex = 0; // we reset the index
                            helperPoints = path.path.GetPointsInSegment(pathIndex); // we reset the points
                            StartCoroutine(wait(paths[pathIndex].delay)); // we wait for the first curve delay
                        }
                        else // else, we finished the path
                        {
                            if (rb)
                            {
                                rb.isKinematic = false; // we set kinematic to false
                                rb.useGravity = true; // and we use gravity
                            }
                        }
                    }
                }
            }
        }
	}

    IEnumerator wait(float delay) // coroutine to wait
    {
        waiting = true; // wait - true
        yield return new WaitForSeconds(delay); // we wait for the delay
        waiting = false; // we reset the wait bool
    }

}
