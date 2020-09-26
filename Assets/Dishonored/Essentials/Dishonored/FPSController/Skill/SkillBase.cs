using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase : MonoBehaviour {

    public Sprite skillIcon;
    public string powerName;
    public bool isHoldable;
    public bool isUpgraded;

    [Space]
    public float manaCost = 10;

    [Space]
    public GameObject helper;

    [Space]
    public AudioClip skillAudio;

    [HideInInspector]
    public MotionCore owner;

	public virtual void ActivateSkill()
    {
        owner.mana -= manaCost;
        owner.audioGeneral.PlayAudio(skillAudio);
    }

    public virtual void StartSkill()
    {

    }

    public virtual void LoopSkill()
    {

    }

    public virtual void CancelPower()
    {

    }
}
