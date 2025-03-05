using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class  UIScript : MonoBehaviour
{
    public GameObject pauseUI;
    public void OnRestartPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnGameResumePress()
    {   
        Time.timeScale = 1;
        pauseUI.SetActive(false);
    }

    public void OnGameExitPress()
    {
        Application.Quit();
    }

    public void OnPausePress()
    {
        Time.timeScale = 0;
        pauseUI.SetActive(true);
    }
}
