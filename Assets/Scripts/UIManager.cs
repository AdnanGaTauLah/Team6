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
    

    [Header("Display Elements")]
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

    private void OnEnable()
    {
        GameManager.OnGameStarted += SetInitialUIState;
        GameManager.OnDayChanged += UpdateDayDisplay;
        GameManager.OnStatsUpdated += UpdateStatsDisplay;
        GameManager.OnNewEvent += DisplayEvent;
        GameManager.OnGameOver += ShowGameOverScreen;
        GameManager.OnWeekChanged += UpdateWeekDisplay;
        GameManager.DisplayGoal += ShowGoal;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= SetInitialUIState;
        GameManager.OnDayChanged -= UpdateDayDisplay;
        GameManager.OnStatsUpdated -= UpdateStatsDisplay;
        GameManager.OnNewEvent -= DisplayEvent;
        GameManager.OnGameOver -= ShowGameOverScreen;
        GameManager.OnWeekChanged -= UpdateWeekDisplay;
        GameManager.DisplayGoal += ShowGoal;
    }

    void Start()
    {
        yesButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(true));
        noButton.onClick.AddListener(() => OnChoiceButtonPressed?.Invoke(false));
        endDayEventUI.SetActive(false);
        goalUI.SetActive(false);
        splashScreen.SetActive(false);
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

    private void UpdateWeekDisplay(int week)
    {
        if (weekText != null)
        {
            weekText.text = $"Week: {week}";
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
        DisplayQuestion(true);
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
        splashScreen.SetActive(false);
        gameManager.StartEvent();
    }


}
