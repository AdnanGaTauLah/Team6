using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all User Interface elements. This version is fully decoupled and only
/// reacts to events broadcast from other systems.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static event Action<bool> OnChoiceButtonPressed;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Display Elements")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI wealthText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private void OnEnable()
    {
        GameManager.OnGameStarted += SetInitialUIState;
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= SetInitialUIState;
        GameManager.OnDayChanged -= UpdateDayDisplay;
        GameManager.OnStatsUpdated -= UpdateStatsDisplay;
        GameManager.OnNewEvent -= DisplayEvent;
        GameManager.OnGameOver -= ShowGameOverScreen;
    }

    void Start()
    {
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));
    }

    // --- Event Handler Methods ---

    /// <summary>
    /// A callback method triggered by the GameManager's OnGameStarted event.
    /// Sets the UI to its correct initial state.
    /// </summary>
    private void SetInitialUIState()
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(true);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void UpdateDayDisplay(int day)
    {
        if (dayText != null)
        {
            dayText.text = $"Day: {day}";
        }
    }

    private void UpdateStatsDisplay(Player player)
    {
        if (player != null)
        {
            survivalText.text = player.Survival.ToString();
            happinessText.text = player.Happiness.ToString();
            wealthText.text = player.Wealth.ToString();
        }
    }

    private void DisplayEvent(GameEvent gameEvent)
    {
        if (gameEvent != null)
        {
            questionText.text = gameEvent.question;
        }
    }

    private void ShowGameOverScreen()
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(false);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
