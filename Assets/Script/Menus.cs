using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    //Creates a game object reference variable for the pause menu
    public GameObject pauseMenu;

    //When the Play Button is pressed, loads the first scene for the game
    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
    }

    //Closes the game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting Game");
    }

    //Resumes the game when paused
    public void ResumeGame()
    {
        Resume();
    }

    //Restarts the game/level (SUBJECT TO CHANGE) for now just reloads the "Level1" scene
    public void RestartGame()
    {
        SceneManager.LoadScene("Level1");
    }


    //Creates a boolean for to determine if the game is paused or not
    public static bool GameIsPaused = false;

    void Update()
    {
        //If the 'Esc' button is pressed, pauses the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused == false)
            {
                Pause();
            }
        }
    }

    //Method to resume the game when paused
    void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Debug.Log("Resuming Game");
    }

    //Method to pause the game
    void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        Debug.Log("Pausing Game");
    }

}
