using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// Manages all User Interface elements. This version is fully decoupled and only
/// reacts to events broadcast from other systems.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- UI Events ---
    public static event Action OnSummaryAcknowledged;
    public static event Action<bool> OnChoiceButtonPressed;
    public GameManager gameManager;
    public WorkEvent workEvent;
    public StayEvent stayEvent;
    private string endEvent;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject goalUI;
    [SerializeField] private GameObject questionUI;
    [SerializeField] private GameObject splashScreen;
    [SerializeField] private GameObject endDayEventUI;
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
    [SerializeField] private TextMeshProUGUI weekText;
    [SerializeField] private TextMeshProUGUI survivalText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI wealthText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI endDayEventText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private TextMeshProUGUI splashText;

    private int currentDay;

    private void OnEnable()
    {
        GameManager.OnDayEndSummary += ShowDaySummary;
        GameManager.OnGameStarted += SetInitialUIState;
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;
        GameManager.OnWeekChanged += UpdateWeekDisplay;
        GameManager.DisplayGoal += ShowGoal;

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
        GameManager.OnWeekChanged -= UpdateWeekDisplay;
        GameManager.DisplayGoal += ShowGoal;

        // NEW: Unsubscribe from the event to prevent memory leaks.
        GameManager.OnEventConcluded -= HideEventPanel;
    }

    void Start()
    {
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));
        endDayEventUI.SetActive(false);
        goalUI.SetActive(false);
        splashScreen.SetActive(false);
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
        dayText.text = $"{day}";

        if (daySummaryPanel.activeSelf)
        {
            daySummaryPanel.SetActive(false);
            gameplayPanel.SetActive(true);
        }
       
    }

    private void UpdateWeekDisplay(int week)
    {
        if (weekText != null)
        {
            weekText.text = $"{week}";
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

    private void ShowGoal(GameManager.Goal goal)
    {
        GameManager.isStartEvent = false;
        goalText.text = goal.narasi.Replace("{value}", goal.value.ToString());
        DisplayQuestion(false);
        goalUI.SetActive(true);
        
    }

    public void DisplayQuestion(bool isStartEvent)
    {
        if (!isStartEvent)
        {
            GameManager.isStartEvent = false;
            questionUI.SetActive(false);
        }
        else
        {
            questionUI.SetActive(true);
        }
    }
    public void CloseGoalButton()
    {
        GameManager.isStartEvent = true;
        goalUI.SetActive(false);
        gameManager.BeginNewDay();
        //DisplayQuestion(true);
    }

    public void DisplayEndDayEvent(string textEvent)
    {
        endDayEventText.text = $"Apakah Anda ingin {textEvent}?";
        endEvent = textEvent;
        endDayEventUI.SetActive(true);
    }
    public void CloseEndDayEventButton(bool isYes)
    {
        endDayEventUI.SetActive(false);
        workEvent.CloseDoor();
        workEvent.isDoorOpen = false;
        GameManager.isEventRunning = false;
        if (isYes)
        {
            CheckEventEndDay();
            return;
        }
    }

    private void CheckEventEndDay()
    {
        if (endEvent == "bekerja")
        {
            DisplaySplashScreen("Anda pergi bekerja");
            workEvent.Work();
        }
        if(endEvent == "di rumah")
        {
            DisplaySplashScreen("Anda menghabiskan Waktu di rumah Bersama keluarga");
            stayEvent.Stay();
        }
    }
    private void DisplaySplashScreen(string message)
    {
        StartCoroutine(DisplaySplashScreenCoroutine(message));
    }

    private IEnumerator DisplaySplashScreenCoroutine(string message)
    {
        splashText.text = message;
        splashScreen.SetActive(true);

       
        yield return new WaitForSeconds(1.5f);
        GameManager.isEndDay = false;
        splashScreen.SetActive(false);
        //gameManager.StartEvent();
    }


}
