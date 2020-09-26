using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameObject : MonoBehaviour {

    public bool useBaseTimeScale = true;

    Rigidbody rb;
    Animator anim;

    Vector3 startVelocity;

    bool velocityTook = false, velocityUsed = false;
    bool isKinematic = false, useGravity = true;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        isKinematic = rb.isKinematic;
        useGravity = rb.useGravity;
	}
	
	// Update is called once per frame
	void Update () {
        if (useBaseTimeScale) {
            if (rb)
            {
                if (GameManager.instance.TimeScale <= .05f)
                {

                    if (!velocityTook)
                    {
                        startVelocity = rb.velocity;

                        velocityTook = true;
                        velocityUsed = false;

                        rb.isKinematic = true;
                        rb.useGravity = false;
                    }

                }else if (GameManager.instance.TimeScale >= 1f)
                {
                    rb.isKinematic = isKinematic;
                    rb.useGravity = useGravity;

                    if (!velocityUsed)
                    {
                        velocityUsed = true;
                        velocityTook = false;

                        rb.velocity = startVelocity;
                    }
                }
            }

            if (anim)
            {
                anim.speed = GameManager.instance.TimeScale;
            }
        }
	}
}
