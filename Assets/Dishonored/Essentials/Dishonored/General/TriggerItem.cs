using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerItem : MonoBehaviour {

    public enum SystemType
    {
        normalType, openCloseType
    }

    GameObject triggerer;

    [System.Serializable]
    public class MyEvent: UnityEvent
    {

    }

    public SystemType systemType;

    public MyEvent onTriggerEvents;
    public MyEvent openEvents;
    public MyEvent closeEvents;

    public bool closed = true;

    bool inSight = false;

    MeshRenderer meshRenderer;
    SkinnedMeshRenderer skinnedMeshRenderer;

    private void Start()
    {
        gameObject.tag = "Trigger";

        meshRenderer = GetComponent<MeshRenderer>();
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        SetFresnel(0);
    }

    private void Update()
    {
        inSight = false;
    }

    public void TriggerSystem(GameObject triggerer)
    {
        this.triggerer = triggerer;

        if (systemType == SystemType.normalType)
        {
            onTriggerEvents.Invoke();
        }
        else if (systemType == SystemType.openCloseType)
        {
            if (closed)
            {
                closeEvents.Invoke();
            }
            else
            {
                openEvents.Invoke();
            }

            closed = !closed;

        }
    }

    public void SendTriggererMessage(string message)
    {
        triggerer.SendMessage(message, SendMessageOptions.DontRequireReceiver);
    }

    float GetFresnel()
    {
        if (meshRenderer)
        {
            return meshRenderer.material.GetFloat("Vector1_1730EF8B");
        }
        else
        {
            return skinnedMeshRenderer.material.GetFloat("Vector1_1730EF8B");
        }
    }

    void SetFresnel(float value)
    {
        if (meshRenderer)
        {
            meshRenderer.material.SetFloat("Vector1_1730EF8B", value);
        }
        else
        {
            skinnedMeshRenderer.material.SetFloat("Vector1_1730EF8B", value);
        }

    }

}
