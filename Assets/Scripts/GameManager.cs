using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// A new, simple data structure to hold the results of a day.
public struct DaySummaryData
{
    public int survivalChange;
    public int happinessChange;
    public int wealthChange;
}
public class GameManager : MonoBehaviour
{
    // --- Events ---
    public static event Action OnEventConcluded;
    public static event Action<DaySummaryData> OnDayEndSummary;
    public static event Action OnGameStarted;
    public static event Action<int> OnDayChanged;
    public static event Action<Player> OnStatsUpdated;
    public static event Action<GameEvent> OnNewEvent;
    public static event Action OnGameOver;
    private event Action<bool> OnChoiceMade;
    public static event Action<int> OnWeekChanged;
    public static event Action<Goal> DisplayGoal;

    [Header("Component References")]
    public EventController eventController;

    [Header("Game Configuration")]
    public int questionsPerDay = 1;
    [Tooltip("The delay in seconds before showing a new event question.")]
    public float eventDelay = 1.5f;

    // --- Static State Variables ---
    public static bool isStartEvent = false;
    public static bool isEndDay = false;
    public static bool isEventRunning = false;
    public static bool isWaitingSplash = false;

    // --- Private Instance Fields ---
    private Player player;
    private PlayerControls gameControls;
    private GameEvent currentEvent;
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;
    private int week = 0;
    private int previousWeek = -1;
    private int currentGoal = 0;
    private bool isGameReady = false;
    private bool isWaitingForSummary;
    private int survivalAtDayStart, happinessAtDayStart, wealthAtDayStart;
    private bool isWaitingForChoice = false;

    [System.Serializable]
    public class Goal
    {
        public string type;
        public string narasi;
        public int value;
    }
    public List<Goal> goals;

    // --- Unity Methods ---
    void Awake()
    {
        // Reset static variables to ensure a clean state on scene load/restart.
        isStartEvent = false;
        isEndDay = false;
        isEventRunning = false;
        isWaitingSplash = false;

        gameControls = new PlayerControls();
        if (eventController == null)
        {
            Debug.LogError("GameManager is missing a reference to the EventController!");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        CharacterSpawner.OnPlayerSpawned += InitializePlayer;
        OnChoiceMade += MakeChoice;
        gameControls.Gameplay.Enable();
        gameControls.Gameplay.ChooseYes.performed += ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed += ReportChoiceFromInput;
        UIManager.OnChoiceButtonPressed += ReportChoice;
        UIManager.OnSummaryAcknowledged += EndDaySequence;
    }

    private void OnDisable()
    {
        CharacterSpawner.OnPlayerSpawned -= InitializePlayer;
        OnChoiceMade -= MakeChoice;
        gameControls.Gameplay.Disable();
        gameControls.Gameplay.ChooseYes.performed -= ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed -= ReportChoiceFromInput;
        UIManager.OnChoiceButtonPressed -= ReportChoice;
        UIManager.OnSummaryAcknowledged -= EndDaySequence;
    }

    // --- Game Flow Methods ---
    private void InitializePlayer(Player spawnedPlayer)
    {
        this.player = spawnedPlayer;
        StartGame();
    }

    private void ReportChoiceFromInput(InputAction.CallbackContext context) => ReportChoice(context.action == gameControls.Gameplay.ChooseYes);
    private void ReportChoice(bool choice) => OnChoiceMade?.Invoke(choice);

    private void StartGame()
    {
        isGameReady = true;
        currentDay = 1;
        questionsAnsweredToday = 0;
        currentGoal = week % goals.Count;
        OnGameStarted?.Invoke();

        // REFACTORED: Start a coroutine to handle the startup sequence safely.
        StartCoroutine(StartGameDelayed());
    }

    /// <summary>
    /// NEW: This coroutine waits for the EventController to be ready before starting the game.
    /// This prevents a race condition in WebGL builds.
    /// </summary>
    private IEnumerator StartGameDelayed()
    {
        // Wait until the EventController signals that it has finished loading the JSON files.
        yield return new WaitUntil(() => eventController.IsReady);

        // Now that the data is loaded, it's safe to begin the game logic.
        BeginNewDay();
    }

    public void BeginNewDay()
    {
        OnDayChanged?.Invoke(currentDay);
        if (week != previousWeek)
        {
            OnWeekChanged?.Invoke(week);
            DisplayGoal?.Invoke(goals[currentGoal]);
            previousWeek = week;
        }
        eventController.StartNewDay();
        survivalAtDayStart = player.Survival;
        happinessAtDayStart = player.Happiness;
        wealthAtDayStart = player.Wealth;
        OnStatsUpdated?.Invoke(player);
        if (isStartEvent)
        {
            StartCoroutine(DailyEventRoutine());
        }
    }

    private IEnumerator DailyEventRoutine()
    {
        while (questionsAnsweredToday < questionsPerDay)
        {
            yield return new WaitForSeconds(eventDelay);
            currentEvent = eventController.GetUniqueEventForDay();
            OnNewEvent?.Invoke(currentEvent);
            isWaitingForChoice = true;
            yield return new WaitUntil(() => !isWaitingForChoice);
        }
        yield return new WaitUntil(() => !isEndDay);
        isWaitingForSummary = true;
        DaySummaryData summary = new DaySummaryData
        {
            survivalChange = player.Survival - survivalAtDayStart,
            happinessChange = player.Happiness - happinessAtDayStart,
            wealthChange = player.Wealth - wealthAtDayStart
        };
        OnDayEndSummary?.Invoke(summary);
    }

    private void MakeChoice(bool choseYes)
    {
        if (!isGameReady || isWaitingForSummary || currentEvent == null || !isWaitingForChoice || !isStartEvent) return;
        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        int survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        int happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        int wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);
        ChangePlayerState(survivalChange, happinessChange, wealthChange);
        OnEventConcluded?.Invoke();
        questionsAnsweredToday++;
        isWaitingForChoice = false;
        isEndDay = true;
    }

    private void EndDaySequence()
    {
        isWaitingForSummary = false;
        CheckForGameOver();
        if (isGameReady)
        {
            currentDay++;
            questionsAnsweredToday = 0;
            BeginNewDay();
        }
    }

    // --- Game Over and Goal Logic ---
    private void CheckForGameOver()
    {
        if (!isGameReady) return;
        if (currentDay % 7 != 0)
        {
            if (player.Survival <= 0 || player.Happiness <= 0) GameOver();
        }
        else
        {
            if (player.Wealth <= 0 || player.Survival <= 0 || player.Happiness <= 0)
            {
                GameOver();
            }
            else
            {
                bool success = CheckGoal(goals[currentGoal]);
                if (success)
                {
                    goals[currentGoal].value += 5;
                    week++;
                    currentGoal = week % goals.Count;
                    isWaitingSplash = true;
                    StartCoroutine(EndWeekEvent());
                }
            }
        }
    }

    private bool CheckGoal(Goal goal)
    {
        switch (goal.type.ToLower())
        {
            case "wealth":
                if (player.Wealth >= goal.value) { ChangePlayerState(0, 0, -goal.value); return true; }
                break;
            case "happiness":
                if (player.Happiness > goal.value) return true;
                break;
            case "survival":
                if (player.Survival > goal.value) return true;
                break;
            default:
                Debug.Log("Unknown goal type: " + goal.type);
                break;
        }
        GameOver();
        return false;
    }

    private void GameOver()
    {
        isGameReady = false;
        OnGameOver?.Invoke();
    }

    public void ChangePlayerState(int survival, int happiness, int wealth)
    {
        Player.StatChange finalOutcome = new Player.StatChange();
        finalOutcome.survivalChange = survival;
        finalOutcome.happinessChange = happiness;
        finalOutcome.wealthChange = wealth;
        player.UpdateStats(finalOutcome);
        OnStatsUpdated?.Invoke(player);
    }

    private IEnumerator EndWeekEvent()
    {
        yield return new WaitUntil(() => !isWaitingSplash);
    }
}
