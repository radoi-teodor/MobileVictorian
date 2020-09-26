using UnityEngine;

public class WorldCanvasEssentials : MonoBehaviour {

    public bool lookAtCamera = true; // has to look at the camera?

    Camera mainCamera; // main cam object

	// Use this for initialization
	void Start () {

        if(lookAtCamera == false) // if does not look at cam
        {
            Destroy(this); // destroy obj
        }

        mainCamera = Camera.main; // get main cam
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(transform.position - mainCamera.transform.rotation * Vector3.back, -(mainCamera.transform.rotation * Vector3.down)); // look at main cam
	}
}
