using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script is specifically for handling UI interactions on the Game Over panel.
/// It should be placed on the GameOverPanel GameObject.
/// </summary>
public class GameOverUIHandler : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("The name of the main menu scene file.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// This public method should be called by the 'Play Again' button's OnClick event.
    /// </summary>
    public void OnRestartButtonPressed()
    {
        // Reloads the currently active scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// This public method should be called by the 'Main Menu' button's OnClick event.
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        // Loads the main menu scene by name.
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
