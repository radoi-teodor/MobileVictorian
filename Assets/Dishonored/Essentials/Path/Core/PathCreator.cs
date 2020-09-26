using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour {

    [HideInInspector]
    public Path path; // path object

    public void CreatePath()
    {
        path = new Path(transform.position); // new object path at the object position
    }

}
