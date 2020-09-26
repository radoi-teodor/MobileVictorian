using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : SkillBase {

    public float force = 50;
    public GameObject effect;

    public override void ActivateSkill()
    {
        if (effect)
        {
            base.ActivateSkill();

            Collider[] cols = Physics.OverlapBox(owner.transform.position + owner.transform.forward * (5f + (isUpgraded?2.5f:0f)), new Vector3(2, 2, (5f + (isUpgraded ? 2.5f : 0f))), owner.transform.rotation);

            Destroy(Instantiate(effect, owner.bobCamComp.transform.position + owner.bobCamComp.transform.forward - owner.bobCamComp.transform.up, owner.bobCamComp.transform.rotation), 10f);

            foreach(Collider col in cols)
            {
                Rigidbody colRb = col.gameObject.GetComponent<Rigidbody>();

                if (colRb && !colRb.isKinematic && col.gameObject != owner.gameObject)
                {
                    colRb.AddForce(owner.transform.forward * (force + (isUpgraded?20:0)), ForceMode.VelocityChange);
                }

                if(col.gameObject.tag == "AI")
                {
                    col.SendMessage("DoDamage", (isUpgraded?1000:50));
                }
            }

        }
    }
}
