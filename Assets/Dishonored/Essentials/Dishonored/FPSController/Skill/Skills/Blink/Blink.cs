using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : SkillBase
{

    public AudioClip blinkStart;

    GameObject helperGO = null;

    public float range = 7, upgradedRange = 15;

    

    public override void ActivateSkill()
    {
        if (helperGO)
        {
            base.ActivateSkill();

            Vector3 teleportPos = helperGO.transform.position;

            Destroy(helperGO);
            helperGO = null;

            StartCoroutine(teleport(teleportPos));
        }
    }

    public override void CancelPower()
    {
        if (helperGO)
        {
            StopAllCoroutines();
            Destroy(helperGO);
        }
    }

    public override void StartSkill()
    {
        base.StartSkill();

        helperGO = Instantiate(helper, transform.position, transform.rotation);

        owner.audioGeneral.PlayAudio(blinkStart);

    }

    public override void LoopSkill()
    {
        base.LoopSkill();

        if (helperGO)
        {

            Vector3 pos;
            Ray camRay = owner.bobCamComp.ViewportPointToRay(new Vector3(.5f, .5f, 0));

            RaycastHit hit;

            if (Physics.Raycast(camRay, out hit, (isUpgraded ? upgradedRange : range)))
            {
                pos = hit.point - owner.bobCamComp.transform.forward;
            }
            else
            {
                pos = owner.bobCamComp.transform.position + camRay.direction.normalized * (isUpgraded?upgradedRange:range);
            }

            pos += owner.transform.up * .5f;

            helperGO.transform.position = pos;
        }
    }

    IEnumerator teleport(Vector3 position)
    {
        owner.controller.enabled = false;

        while(owner.bobCamComp.fieldOfView <= 80)
        {
            owner.bobCamComp.fieldOfView = Mathf.Lerp(owner.bobCamComp.fieldOfView, 85, Time.deltaTime * 5);
            yield return null;
        }

        while(owner.bobCamComp.fieldOfView > 60 && (transform.position - position).magnitude > .1f)
        {
            if (owner.bobCamComp.fieldOfView >= 60)
            {
                owner.bobCamComp.fieldOfView = Mathf.Lerp(owner.bobCamComp.fieldOfView, 55, Time.deltaTime * 5);
            }

            if((transform.position - position).magnitude >= .1f)
            {
                owner.transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 30);
            }

            yield return null;
        }

        owner.controller.enabled = true;

    }
}
