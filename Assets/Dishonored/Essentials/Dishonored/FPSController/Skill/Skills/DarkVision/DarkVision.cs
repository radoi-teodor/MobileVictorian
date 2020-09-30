using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkVision : SkillBase {

    public float duration = 5;
    [Space]
    public AudioClip endAudio;

    bool inEffect = false;

    BaseAI[] baisGL;

    public override void ActivateSkill()
    {
        base.ActivateSkill();

        if (!inEffect)
        {
            base.ActivateSkill();

            StartCoroutine(darkVision());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(endDarkVision());
        }
    }

    IEnumerator darkVision()
    {
        inEffect = true;


        GameManager.instance.MakeXRAY();

        yield return new WaitForSeconds(duration + (isUpgraded?5:0));

        owner.audioGeneral.PlayAudio(endAudio);


        GameManager.instance.MakeNormal();


        inEffect = false;
    }

    IEnumerator endDarkVision()
    {
        owner.audioGeneral.PlayAudio(endAudio);

        yield return new WaitForSeconds(1f);


        GameManager.instance.MakeNormal();

        yield return new WaitForSeconds(1f);

        inEffect = false;

    }
}
