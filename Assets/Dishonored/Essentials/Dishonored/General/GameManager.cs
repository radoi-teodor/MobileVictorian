using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    /// <summary>
    /// Public variables and components
    /// </summary>
    /// 

    [Header("General")]
    public string nextLevel;
    [Space]
    public bool isCinematic = false;
    public bool paused = false;
    [Space]
    public KeyCode pauseKey;
    
    [Space]
    [Space]
    [Header("Time")]
    public float TimeScale = 1f;
    public float MasterTimeScale = 1f;

    float lastTimeScale;
    float lastMasterTimeScale;

    [Space]
    public float deltaTimeScale;
    public float deltaMasterTimeScale;

    [Space]
    public float nextTimeScale;

    [Space]
    [Space]
    [Header("Chaos")]
    public int dead = 0;
    public int lowChaosDeadLimit = 5;

    [Space]
    [Space]
    [Header("UI")]
    public CanvasScaler cs;
    public Text subtitleTX;
    public GameObject cinematicPan;
    public Animator loadingAnim;
    [Space]
    public Image pausePan;
    public Animator pauseAnim;

    /// <summary>
    /// Pricate variables and components
    /// </summary>

    MotionCore playerCore;

    bool inSubtitle = false;
    bool controllerWasEnabled = true;

    List<string> subtitles = new List<string>();

    [HideInInspector]
    public bool lowChaos = true;

    public BaseAI.MyEvent onExitChatEvent = null;

    IEnumerator showSubCor=null;

    // Use this for initialization
    void Awake () {
        instance = this;

        PlayerPrefs.SetString("level", SceneManager.GetActiveScene().name);

        paused = false;
        SetBlurPause(0);

        cs.referenceResolution = new Vector2(Screen.width, Screen.height);

        subtitleTX.gameObject.SetActive(false);

        playerCore = FindObjectOfType<MotionCore>();
    }

    // Update is called once per frame
    void Update () {
        if (!inSubtitle && subtitles.Count > 0)
        {
            showSubCor = subtitleShow();
            StartCoroutine(showSubCor);
        }

        if (dead <= lowChaosDeadLimit)
        {
            lowChaos = true;
        }
        else
        {
            lowChaos = false;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            if (!paused)
            {
                if (playerCore)
                {
                    controllerWasEnabled = playerCore.controller.enabled;
                    playerCore.controller.enabled = false;
                    playerCore.CancelPower();
                }

                lastMasterTimeScale = MasterTimeScale;
                lastTimeScale = TimeScale;

                MasterTimeScale = 0;
                TimeScale = 0;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                if (playerCore)
                {
                    playerCore.controller.enabled = controllerWasEnabled;
                }

                MasterTimeScale = lastMasterTimeScale;
                TimeScale = lastTimeScale;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            paused = !paused;
            SetBlurPause(paused ? 1 : 0);

            pauseAnim.SetBool("Paused", paused);
        }

        cinematicPan.SetActive(isCinematic);
    }

    IEnumerator subtitleShow()
    {
        inSubtitle = true;

        subtitleTX.gameObject.SetActive(true);

        foreach (string subtitle in subtitles)
        {
            subtitleTX.text = subtitle;

            float dur = 2f;

            while(dur > 0)
            {
                dur -= Time.deltaTime * MasterTimeScale;
                yield return null;
            }
        }

        if (onExitChatEvent != null)
        {
            onExitChatEvent.Invoke();
            StartCoroutine(ResetEvents());
        }


        subtitleTX.gameObject.SetActive(false);

        subtitles = new List<string>();

        inSubtitle = false;
    }

    public void Resume()
    {
        if (!paused)
        {
            if (playerCore)
            {
                controllerWasEnabled = playerCore.controller.enabled;
                playerCore.controller.enabled = false;
                playerCore.CancelPower();
            }

            lastMasterTimeScale = MasterTimeScale;
            lastTimeScale = TimeScale;

            MasterTimeScale = 0;
            TimeScale = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (playerCore)
            {
                playerCore.controller.enabled = controllerWasEnabled;
            }

            MasterTimeScale = lastMasterTimeScale;
            TimeScale = lastTimeScale;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        paused = !paused;
        SetBlurPause(paused ? 1 : 0);

        pauseAnim.SetBool("Paused", paused);
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadCor("Menu"));
    }

    public void RestartLevel()
    {
        StartCoroutine(LoadCor(SceneManager.GetActiveScene().name));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetSubtitle(List<string> subs)
    {
        if (showSubCor != null)
        {
            StopCoroutine(showSubCor);
        }

        if (onExitChatEvent != null && inSubtitle)
        {
            onExitChatEvent.Invoke();
            StartCoroutine(ResetEvents());
        }

        inSubtitle = false;
        subtitles = subs;
    }

    IEnumerator ResetEvents()
    {
        yield return null;
        onExitChatEvent = null;
    }

    public void PublicSetSubtitle(string subs)
    {
        if (showSubCor != null)
        {
            StopCoroutine(showSubCor);
        }

        inSubtitle = false;

        List<string> actualSubs = new List<string>();
        string[] subsArray = subs.Split('*');

        if(subsArray.Length > 0)
        {
            foreach(string s in subsArray)
            {
                actualSubs.Add(s);
            }
        }
        else
        {
            actualSubs.Add(subs);
        }

        subtitles = actualSubs;
    }

    void SetBlurPause(float value)
    {
        pausePan.material.SetFloat("_Size", value);
    }

    IEnumerator LoadCor(string level)
    {
        loadingAnim.SetTrigger("Load");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync(level);

    }

    IEnumerator LoadCorWait(string level, float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(LoadCor(level));
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadCor(nextLevel));
    }

    public void LoadNextLevelWait(float time)
    {
        StartCoroutine(LoadCorWait(nextLevel, time));
    }

}
