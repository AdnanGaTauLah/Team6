using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("A parent object that holds all the active gameplay UI (stats, question, buttons).")]
    [SerializeField] private GameObject gameplayPanel;
    [Tooltip("The panel that is shown when the game ends.")]
    [SerializeField] private GameObject gameOverPanel;


    [Header("Stat Displays")]
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI wealthText;

    [Header("Event Display")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;


    /// <summary>
    /// Updates the text elements that display the player's current stats.
    /// </summary>
    public void UpdateStatsDisplay(Player player)
    {
        if (player == null) return;
        survivalText.text = "Survival: " + player.survival;
        happinessText.text = "Happiness: " + player.happiness;
        wealthText.text = "Wealth: " + player.wealth;
    }

    /// <summary>
    /// Displays the question for the current event.
    /// </summary>
    public void DisplayEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) return;
        questionText.text = gameEvent.question;
    }

    /// <summary>
    /// Shows the Game Over screen and hides the main gameplay UI.
    /// </summary>
    public void ShowGameOverScreen()
    {
        gameplayPanel.SetActive(false); // Hide the gameplay UI
        gameOverPanel.SetActive(true);  // Show the game over screen
    }

    /// <summary>
    /// Hides the Game Over panel and shows the main gameplay UI.
    /// This is used to set the initial state of the game.
    /// </summary>
    public void HideGameOverScreen()
    {
        gameplayPanel.SetActive(true);  // Show the gameplay UI
        gameOverPanel.SetActive(false); // Hide the game over screen
    }

    /// <summary>
    /// Connects the UI buttons to the GameManager's logic.
    /// This is called once at the start of the game.
    /// </summary>
    public void SetupButtonListeners(GameManager gameManager)
    {
        // Clear any previous listeners to be safe
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        // Add new listeners that call the MakeChoice method in the GameManager
        yesButton.onClick.AddListener(() => gameManager.MakeChoice(true));
        noButton.onClick.AddListener(() => gameManager.MakeChoice(false));
    }
}
