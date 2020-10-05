using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Possession : SkillBase
{
    GameObject helperGO = null;
    GameObject _target;
    GameObject target
    {
        get { return _target; }
        set { _target = value; }
    }

    bool inEffect = false;

    // Public
    public float range = 7;
    public LayerMask powerLayerMask;

    [Space]
    public float duration = 3;
    public float upgradedDuration = 5;
    public AudioClip possessionEndAudio;

    int cameraEffectsLoopDivision = 10;

    float startVignette = 1;
    float startVignetteSmoothness = 1;

    float endVignette = 0.5f;
    float endVignetteSmoothness = 0.2f;

    float intensitySpeed = 7;
    float smoothnessSpeed = 10;

    private void Awake()
    {
    }

    private void Update()
    {
        if(inEffect && ControlFreak2.CF2Input.GetMouseButtonDown(1))
        {
            ActivateSkill();
        }
    }

    // Start is called before the first frame update
    public override void ActivateSkill()
    {

        if (inEffect)
        {
            StopAllCoroutines();
            StartCoroutine(StopPower());
            return;
        }
        else
        {
            if (target)
            {
                base.ActivateSkill();

                StartCoroutine(possess());
            }
        }
    }

    public override void CancelPower()
    {
        if (inEffect)
            return;

        StopAllCoroutines();
        if (helperGO)
        {
            Destroy(helperGO);
        }
    }

    public override void StartSkill()
    {
        if (inEffect)
            return;

        base.StartSkill();
    }

    public override void LoopSkill()
    {

        if (inEffect)
            return;

        base.LoopSkill();

        Ray camRay = owner.bobCamComp.ViewportPointToRay(new Vector3(.5f, .5f, 0));

        RaycastHit hit;

        if (Physics.Raycast(camRay, out hit, range, powerLayerMask))
        {
            target = hit.collider.gameObject;

            if (!helperGO)
                helperGO = Instantiate(helper, target.transform.position + target.transform.up * 2, Quaternion.identity);
        }
        else
        {
            target = null;

            if (helperGO)
                Destroy(helperGO);
        }

        if (target)
        {
            helperGO.transform.position = target.transform.position + target.transform.up * 2;
        }
    }

    public IEnumerator cameraEffectStart()
    {

        owner.settingVignette = true;

        float differenceIntensity = startVignette - owner.vignette.intensity.value;
        float differenceSmoothness = startVignetteSmoothness - owner.vignette.smoothness.value;

        float deltaIntesity = differenceIntensity / cameraEffectsLoopDivision;
        float deltaSmoothness = differenceSmoothness / cameraEffectsLoopDivision;

        for (int i = 1; i <= cameraEffectsLoopDivision; i++)
        {
            owner.SetVignette(owner.vignette.intensity.value + deltaIntesity);
            owner.vignette.smoothness.value += deltaSmoothness;
            yield return null;
        }

        owner.vignette.intensity.value = startVignette;
        owner.vignette.smoothness.value = startVignetteSmoothness;
    }

    public IEnumerator cameraEffectEnd()
    {
        float differenceIntensity = endVignette - owner.vignette.intensity.value;
        float differenceSmoothness = endVignetteSmoothness - owner.vignette.smoothness.value;

        float deltaIntesity = differenceIntensity / cameraEffectsLoopDivision;
        float deltaSmoothness = differenceSmoothness / cameraEffectsLoopDivision;

        for (int i = 1; i <= cameraEffectsLoopDivision; i++)
        {
            owner.SetVignette(owner.vignette.intensity.value + deltaIntesity);
            owner.vignette.smoothness.value += deltaSmoothness;
            yield return null;
        }

        owner.vignette.intensity.value = endVignette;
        owner.vignette.smoothness.value = endVignetteSmoothness;
    }

    public IEnumerator StartPower()
    {
        inEffect = true;

        owner.canUseAbilities = false;
        owner.hideAbilitiesButtons();

        yield return new WaitForSeconds(1f);

        owner.UnequipAll();

        // Effect
        yield return StartCoroutine(cameraEffectStart());

        // TELEPORT
        Vector3 teleportPos = target.transform.position;
        target.SetActive(false);

        owner.transform.position = teleportPos;
        // TELEPORT

        yield return StartCoroutine(cameraEffectEnd());
        // Effect


        Destroy(helperGO);
    }

    public IEnumerator StopPower()
    {

        // Effect
        yield return StartCoroutine(cameraEffectStart());

        // End possession
        target.SetActive(true);
        target.transform.position = owner.transform.position + owner.transform.forward * 1.5f;
        target.transform.rotation = owner.transform.rotation;
        target.SendMessage("MakeSick");
        // End possession

        yield return StartCoroutine(cameraEffectEnd());
        // Effect

        owner.settingVignette = false;
        owner.canUseAbilities = true;
        owner.showAbilitiesButtons();

        inEffect = false;

        owner.audioGeneral.PlayAudio(possessionEndAudio);
    }

    IEnumerator possess()
    {

        yield return StartCoroutine(StartPower());

        yield return new WaitForSeconds(isUpgraded? upgradedDuration:duration);

        yield return StartCoroutine(StopPower());

    }
}
