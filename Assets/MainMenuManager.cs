using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        FindObjectOfType<AudioManager>().Play("bgm");
        Time.timeScale = 1;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuiteGame()
    {
        Application.Quit();
    }
}
