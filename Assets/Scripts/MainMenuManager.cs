using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The panel containing the 'Start' and 'Exit' buttons.")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("The panel containing the character selection buttons.")]
    [SerializeField] private GameObject characterSelectionPanel;

    [Header("Scene Configuration")]
    [Tooltip("The name of your main gameplay scene file.")]
    [SerializeField] private string gameplaySceneName = "GameplaySceneIBU"; // Make sure this matches your scene's filename

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Ensures the correct panel is visible at the start.
    /// </summary>
    private void Start()
    {
        // Start by showing the main menu and hiding the character selection.
        mainMenuPanel.SetActive(true);
        characterSelectionPanel.SetActive(false);
    }

    /// <summary>
    /// This public method is called by the 'Start' button's OnClick event.
    /// </summary>
    public void ShowCharacterSelection()
    {
        mainMenuPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
    }

    /// <summary>
    /// This public method is called by the character choice buttons (Father/Mother).
    /// It saves the choice to the persistent GameData instance.
    /// </summary>
    /// <param name="characterIndex">The integer index of the character. 1 for Father, 2 for Mother.</param>
    public void SelectCharacter(int characterIndex)
    {
        if (GameData.Instance != null)
        {
            // Cast the integer from the button to the Player.MentorFigure enum
            GameData.Instance.selectedMentor = (Player.MentorFigure)characterIndex;
            Debug.Log($"Character chosen: {GameData.Instance.selectedMentor}");

            // Load the main gameplay scene
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("GameData instance not found! Make sure the GameData prefab is in the scene.");
        }
    }

    /// <summary>
    /// This public method is called by the 'Exit' button's OnClick event.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}
