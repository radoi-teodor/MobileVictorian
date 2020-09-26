using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManipulation : SkillBase
{

    public float duration = 5;
    [Space]
    public AudioClip endAudio;

    bool inEffect = false;

    float speed;

    private void Awake()
    {
        speed = Time.deltaTime;
    }

    public override void ActivateSkill()
    {
        if (!inEffect)
        {
            base.ActivateSkill();

            StartCoroutine(stopTime());
        }
        else
        {
            StopCoroutine(stopTime());
            StartCoroutine(startTime());
        }
    }

    IEnumerator stopTime()
    {
        inEffect = true;

        if (isUpgraded)
        {
            GameManager.instance.nextTimeScale = 0f;

            while (GameManager.instance.TimeScale > -0.05f)
            {
                GameManager.instance.TimeScale = Mathf.Lerp(GameManager.instance.TimeScale, -.1f, speed);
                owner.SetSaturation(Mathf.Lerp(owner.GetSaturation(), 0, speed));
                GameManager.instance.deltaTimeScale = -speed;
                yield return null;
            }
            GameManager.instance.TimeScale = Mathf.Clamp(GameManager.instance.TimeScale, 0, Mathf.Infinity);

            GameManager.instance.TimeScale = 0f;
            owner.SetSaturation(0);
            GameManager.instance.deltaTimeScale = 0;

            float t = duration;

            while (t > 0)
            {
                t -= Time.deltaTime * GameManager.instance.MasterTimeScale;
                yield return null;
            }

            while (GameManager.instance.TimeScale <= 1)
            {
                GameManager.instance.TimeScale = Mathf.Lerp(GameManager.instance.TimeScale, 1.1f, speed);
                owner.SetSaturation(Mathf.Lerp(owner.GetSaturation(), 1, speed));
                GameManager.instance.deltaTimeScale = speed;

                yield return null;
            }

            GameManager.instance.TimeScale = 1f;
            owner.SetSaturation(1);

        }
        else
        {
            GameManager.instance.nextTimeScale = .5f;

            while (GameManager.instance.TimeScale > 0.55)
            {
                GameManager.instance.TimeScale = Mathf.Lerp(GameManager.instance.TimeScale, .5f, speed);
                owner.SetSaturation(Mathf.Lerp(owner.GetSaturation(), 0, speed));
                GameManager.instance.deltaTimeScale = -speed;
                yield return null;
            }

            GameManager.instance.TimeScale = .5f;
            owner.SetSaturation(0);

            GameManager.instance.deltaTimeScale = 0;

            float t = duration;

            while (t > 0)
            {
                t -= Time.deltaTime * GameManager.instance.MasterTimeScale;
                yield return null;
            }

            while (Time.timeScale <= 1)
            {
                GameManager.instance.TimeScale = Mathf.Lerp(GameManager.instance.TimeScale, 1.1f, speed);
                owner.SetSaturation(Mathf.Lerp(owner.GetSaturation(), 1, speed));
                GameManager.instance.deltaTimeScale = speed;

                yield return null;
            }

            GameManager.instance.TimeScale = 1f;
            owner.SetSaturation(1);

        }

        GameManager.instance.deltaTimeScale = 0;

        owner.audioGeneral.PlayAudio(endAudio);

        inEffect = false;
    }

    IEnumerator startTime()
    {

        while (GameManager.instance.TimeScale <= 1)
        {
            GameManager.instance.TimeScale = Mathf.Lerp(GameManager.instance.TimeScale, 1.1f, speed);
            owner.SetSaturation(Mathf.Lerp(owner.GetSaturation(), 1, speed));
            GameManager.instance.deltaTimeScale = speed;

            yield return null;
        }

        GameManager.instance.TimeScale = 1f;
        owner.SetSaturation(1);

        GameManager.instance.deltaTimeScale = 0;

        owner.audioGeneral.PlayAudio(endAudio);

        inEffect = false;
    }

}
