using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectTriggerer : MonoBehaviour {

    public enum CollisionType
    {
        collision, trigger
    }

    public CollisionType collisionType;

    [System.Serializable]
    public class MyEvent: UnityEvent
    {

    }

    [Space]
    public MyEvent collisionEvent;

    [Space]
    public List<string> tagsToFind;

    private void OnTriggerEnter(Collider other)
    {
        if(collisionType == CollisionType.trigger)
        {
            if (tagsToFind.Contains(other.tag))
            {
                collisionEvent.Invoke();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collisionType == CollisionType.collision)
        {
            if (tagsToFind.Contains(collision.collider.tag))
            {
                collisionEvent.Invoke();
            }
        }
    }
}
