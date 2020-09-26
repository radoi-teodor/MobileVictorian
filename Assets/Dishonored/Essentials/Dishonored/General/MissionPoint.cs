using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPoint : MonoBehaviour {

    public bool loadNextPointOnTrigger = true;
    public bool loadNextLevelOnTrigger = false;

    [Space]
    public MissionPoint nextMissionPoint = null;
    [Space]
    public string missionText = "";

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (loadNextLevelOnTrigger)
            {
                GameManager.instance.LoadNextLevel();
            }else if (loadNextPointOnTrigger)
            {
                LoadNextMissionPoint();
                other.SendMessage("RefreshMission", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void LoadNextMissionPoint()
    {
        if (nextMissionPoint)
        {
            nextMissionPoint.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
