using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraScript : MonoBehaviour {

	public void ResetCameraCulling()
    {
        SendMessageUpwards("ResetCameraCullingFunc", SendMessageOptions.DontRequireReceiver);
    }
}
