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
    // --- UI Events ---
    public static event Action OnSummaryAcknowledged;
    public static event Action<bool> OnChoiceButtonPressed;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject daySummaryPanel;

    // NEW: A specific reference to the panel containing the question and buttons.
    [Tooltip("The child panel that holds the event question and Yes/No buttons.")]
    [SerializeField] private GameObject eventQuestionPanel;

    [Header("Day Summary Elements")]
    [SerializeField] private TextMeshProUGUI summaryTitleText;
    [SerializeField] private TextMeshProUGUI summarySurvivalText;
    [SerializeField] private TextMeshProUGUI summaryHappinessText;
    [SerializeField] private TextMeshProUGUI summaryWealthText;
    [SerializeField] private Button summaryContinueButton;

    [Header("Gameplay Display Elements")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI wealthText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private int currentDay;

    private void OnEnable()
    {
        GameManager.OnDayEndSummary += ShowDaySummary;
        GameManager.OnGameStarted += SetInitialUIState;
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;

        // NEW: Subscribe to the new event from the GameManager.
        GameManager.OnEventConcluded += HideEventPanel;
    }

    private void OnDisable()
    {
        GameManager.OnDayEndSummary -= ShowDaySummary;
        GameManager.OnGameStarted -= SetInitialUIState;
        GameManager.OnDayChanged -= UpdateDayDisplay;
        GameManager.OnStatsUpdated -= UpdateStatsDisplay;
        GameManager.OnNewEvent -= DisplayEvent;
        GameManager.OnGameOver -= ShowGameOverScreen;

        // NEW: Unsubscribe from the event to prevent memory leaks.
        GameManager.OnEventConcluded -= HideEventPanel;
    }

    void Start()
    {
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));
        summaryContinueButton.onClick.AddListener(() => OnSummaryAcknowledged?.Invoke());
    }

    private void SetInitialUIState()
    {
        gameplayPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        daySummaryPanel.SetActive(false);

        // Ensure the event panel is hidden at the very start.
        eventQuestionPanel.SetActive(false);
    }

    private void ShowDaySummary(DaySummaryData summary)
    {
        if (summaryTitleText != null)
        {
            summaryTitleText.text = $"Hari ke-{currentDay} sudah berakhir";
        }

        // Using color tags to make the summary more readable.
        summarySurvivalText.text = $"Survival: <color={(summary.survivalChange >= 0 ? "green" : "red")}>{summary.survivalChange:+#;-#;0}</color>";
        summaryHappinessText.text = $"Happiness: <color={(summary.happinessChange >= 0 ? "green" : "red")}>{summary.happinessChange:+#;-#;0}</color>";
        summaryWealthText.text = $"Wealth: <color={(summary.wealthChange >= 0 ? "green" : "red")}>{summary.wealthChange:+#;-#;0}</color>";

        gameplayPanel.SetActive(false);
        daySummaryPanel.SetActive(true);
    }

    private void UpdateDayDisplay(int day)
    {
        currentDay = day;
        dayText.text = $"Day: {day}";

        if (daySummaryPanel.activeSelf)
        {
            daySummaryPanel.SetActive(false);
            gameplayPanel.SetActive(true);
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

    /// <summary>
    /// REFACTORED: This now shows the specific question panel.
    /// </summary>
    private void DisplayEvent(GameEvent gameEvent)
    {
        if (gameEvent != null)
        {
            questionText.text = gameEvent.question;
            eventQuestionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// NEW: This method is called by the OnEventConcluded event from the GameManager.
    /// </summary>
    private void HideEventPanel()
    {
        eventQuestionPanel.SetActive(false);
    }

    private void ShowGameOverScreen()
    {
        gameplayPanel.SetActive(false);
        daySummaryPanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }
}
