using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectEssentials : MonoBehaviour {

    [System.Serializable]
    public class Event: UnityEvent // custom events class
    {
    }

    public Event onEnable, startEvent, onDestroyEvent; // event objects

    private void OnEnable()
    {
        onEnable.Invoke(); // onEnable invoke
    }

    // Use this for initialization
    void Start () {
        startEvent.Invoke(); // startEvent invoke
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void DestroyObject(float delay) // public destroy function
    {
        StartCoroutine(DestroyCor(delay));
    }

    IEnumerator DestroyCor(float delay)
    {
        yield return new WaitForSeconds(delay);
        onDestroyEvent.Invoke(); // we invoke on destroy event
        Destroy(gameObject); // we destroy gameobject after delay
    }
}
