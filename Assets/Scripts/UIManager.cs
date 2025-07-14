using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public static event Action<bool> OnChoiceButtonPressed;

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
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // --- Subscribing to Events ---
    private void OnEnable()
    {
        // Start listening for broadcasts from the GameManager
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;
    }

    // --- Unsubscribing from Events ---
    private void OnDisable()
    {
        // Stop listening when this object is disabled to prevent errors
        GameManager.OnDayChanged -= UpdateDayDisplay;
        GameManager.OnStatsUpdated -= UpdateStatsDisplay;
        GameManager.OnNewEvent -= DisplayEvent;
        GameManager.OnGameOver -= ShowGameOverScreen;
    }

    void Start()
    {
        // The UIManager now fires an event instead of calling the GameManager directly
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));

        HideGameOverScreen();
    }

    /// <summary>
    /// Updates the Day counter text.
    /// </summary>
    public void UpdateDayDisplay(int day)
    {
        if (dayText != null)
        {
            dayText.text = "Day: " + day;
        }
    }

    /// <summary>
    /// Updates the text elements that display the player's current stats.
    /// </summary>
    public void UpdateStatsDisplay(Player player)
    {
        if (player == null) return;
        survivalText.text = "Survival: " + player.Survival;
        happinessText.text = "Happiness: " + player.Happiness;
        wealthText.text = "Wealth: " + player.Wealth;
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
