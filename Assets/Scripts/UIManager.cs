using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Stat Displays")]
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI wealthText;

    [Header("Event Display")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;

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
    /// Shows the Game Over panel.
    /// </summary>
    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the Game Over panel.
    /// </summary>
    public void HideGameOverScreen()
    {
        gameOverPanel.SetActive(false);
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
