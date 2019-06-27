using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour {

    public GameObject stats;
    public GameObject pauseMenu;
    bool paused = false;
	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Cancel"))
        {
            paused = !paused;
            if (paused)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                stats.SetActive(false);
                pauseMenu.SetActive(true);
            }
            if (!paused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenu.SetActive(false);
                stats.SetActive(true);
                Time.timeScale = 1;
            }
        }
	}

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        stats.SetActive(true);
        Time.timeScale = 1;
        paused = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
