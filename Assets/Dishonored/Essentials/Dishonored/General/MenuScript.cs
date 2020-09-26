using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    public string startLevel;

    [Space]
    [Space]
    [Header("UI")]
    public Animator loadingAnim;

    public void LoadStartLevel()
    {
        StartCoroutine(LoadCor(PlayerPrefs.GetString("level", startLevel)));
    }

    IEnumerator LoadCor(string level)
    {
        loadingAnim.SetTrigger("Load");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync(level);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
