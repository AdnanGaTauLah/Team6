using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// A public event that this UIManager broadcasts when a choice button is pressed.
    /// The GameManager listens for this event to receive input from the UI.
    /// The boolean payload is true for 'Yes' and false for 'No'.
    /// </summary>
    public static event Action<bool> OnChoiceButtonPressed;

    [Header("UI Panels")]
    [Tooltip("A parent object that holds all the active gameplay UI (stats, question, buttons).")]
    [SerializeField] private GameObject gameplayPanel;
    [Tooltip("The panel that is shown when the game ends.")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Display Elements")]
    [Tooltip("The TextMeshPro element used to display the current day.")]
    [SerializeField] private TextMeshProUGUI dayText;
    [Tooltip("The TextMeshPro element used to display the player's Survival stat.")]
    [SerializeField] private TextMeshProUGUI survivalText;
    [Tooltip("The TextMeshPro element used to display the player's Happiness stat.")]
    [SerializeField] private TextMeshProUGUI happinessText;
    [Tooltip("The TextMeshPro element used to display the player's Wealth stat.")]
    [SerializeField] private TextMeshProUGUI wealthText;
    [Tooltip("The TextMeshPro element used to display the current event's question.")]
    [SerializeField] private TextMeshProUGUI questionText;
    [Tooltip("The button for the 'Yes' choice.")]
    [SerializeField] private Button yesButton;
    [Tooltip("The button for the 'No' choice.")]
    [SerializeField] private Button noButton;

    /// <summary>
    /// Subscribes to all relevant events from the GameManager when this component is enabled.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;
    }

    /// <summary>
    /// Unsubscribes from all events when this component is disabled to prevent errors and memory leaks.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnDayChanged -= UpdateDayDisplay;
        GameManager.OnStatsUpdated -= UpdateStatsDisplay;
        GameManager.OnNewEvent -= DisplayEvent;
        GameManager.OnGameOver -= ShowGameOverScreen;
    }

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// Sets up the initial button listeners.
    /// </summary>
    void Start()
    {
        // FIX: This ensures the UIManager fires its OWN event instead of trying to call a private method in another class.
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));

        HideGameOverScreen();
    }

    // --- Event Handler Methods ---

    /// <summary>
    /// A callback method that is triggered by the GameManager's OnDayChanged event.
    /// </summary>
    /// <param name="day">The current day number.</param>
    private void UpdateDayDisplay(int day) => dayText.text = "Day: " + day;

    /// <summary>
    /// A callback method triggered by the GameManager's OnStatsUpdated event. Updates all stat displays.
    /// </summary>
    /// <param name="player">The player object containing the latest stats.</param>
    private void UpdateStatsDisplay(Player player)
    {
        survivalText.text = "Survival: " + player.Survival;
        happinessText.text = "Happiness: " + player.Happiness;
        wealthText.text = "Wealth: " + player.Wealth;
    }

    /// <summary>
    /// A callback method triggered by the GameManager's OnNewEvent event. Displays the new question.
    /// </summary>
    /// <param name="gameEvent">The event object containing the question text.</param>
    private void DisplayEvent(GameEvent gameEvent) => questionText.text = gameEvent.question;

    /// <summary>
    /// A callback method triggered by the GameManager's OnGameOver event. Hides the gameplay UI and shows the game over panel.
    /// </summary>
    private void ShowGameOverScreen()
    {
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the Game Over panel and shows the main gameplay UI. Used for initialization.
    /// </summary>
    private void HideGameOverScreen()
    {
        gameplayPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }
}
