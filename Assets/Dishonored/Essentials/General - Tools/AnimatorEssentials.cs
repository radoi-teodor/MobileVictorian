using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEssentials : MonoBehaviour {

    Animator anim; // anim of gameobject

    private void OnEnable()
    {
        anim = GetComponent<Animator>(); // get the anim
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		if(anim == null) // verify
        {
            throw new System.NullReferenceException(); // throw null exception
        }
	}

    public void SetFloat(string value) // set float in animator
    {// Syntax: "Movement" 1
        if (anim) // if anim not equal null
        {
            string[] vals = value.Split('\"'); // split by "
            float v = 0; // value of the animator float
            if (vals.Length == 3) // if syntax is correct
            {
                if (float.TryParse(vals[2].Trim(), out v)) // trying to parse from string to float
                {
                    anim.SetFloat(vals[1], v); // set the float
                }
                else
                {
                    throw new System.Exception("Invalid argument"); // throw exception
                }
            }
            else
            {
                throw new System.Exception("Invalid argument"); // throw exception
            }
        }
        else
        {
            throw new System.NullReferenceException(); // throw null reference
        }
    }

    public void SetInt(string value) // set int in animator
    {
        if (anim) // if anim not equal null
        {
            string[] vals = value.Split('\"'); // split by "
            int v = 0; // value of the animator int
            if (vals.Length == 3) // if syntax is correct
            {
                if (int.TryParse(vals[2].Trim(), out v)) // trying to parse from string to int
                {
                    anim.SetInteger(vals[1], v); // set the float
                }
                else
                {
                    throw new System.Exception("Invalid argument"); // throw exception
                }
            }
            else
            {
                throw new System.Exception("Invalid argument"); // throw exception
            }
        }
        else
        {
            throw new System.NullReferenceException(); // throw null reference
        }
    }

    public void SetBool(string value) // set int in animator
    {
        if (anim) // if anim not equal null
        {
            string[] vals = value.Split('\"'); // split by "
            int v = 0; // value of the animator int (to be converted to bool)
            if (vals.Length == 3) // if syntax is correct
            {
                if (int.TryParse(vals[2].Trim(), out v)) // parse to int
                {
                    anim.SetBool(vals[1], v==0?false:true); // if int is 0, bool must be set to false
                                                            // else, it is true
                }
                else
                {
                    throw new System.Exception("Invalid argument"); // throw exception
                }
            }
            else
            {
                throw new System.Exception("Invalid argument"); // throw exception
            }
        }
        else
        {
            throw new System.NullReferenceException();
        }
    }

    public void SetTrigger(string value)
    {
        if (anim) // if anim not equal null
        {
            string[] vals = value.Split('\"'); // split by "

            if (vals.Length == 3) // if syntax is correct
            {
                anim.SetTrigger(vals[1]); // set trigger
            }
            else
            {
                throw new System.Exception("Invalid argument"); // throw exception
            }
        }
        else
        {
            throw new System.NullReferenceException(); // throw null reference
        }
    }

    public void ResetTrigger(string value)
    {
        if (anim) // if anim not equal null
        {
            string[] vals = value.Split('\"'); // split by "

            if (vals.Length == 3) // if syntax is correct
            {
                anim.ResetTrigger(vals[1]); // reset trigger
            }
            else
            {
                throw new System.Exception("Invalid argument"); // throw exception
            }
        }
        else
        {
            throw new System.NullReferenceException(); // throw null reference
        }
    }
}
