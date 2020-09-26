using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour {

    public GameObject baseGO;
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;

        if(Physics.Raycast(new Ray(transform.position, -transform.up), out hit, Mathf.Infinity))
        {
            baseGO.transform.position = hit.point + transform.up * .05f;
        }

	}
}
