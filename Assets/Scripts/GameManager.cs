using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// A new, simple data structure to hold the results of a day.
// This is cleaner than passing three separate integers in an event.
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

    [Header("Component References")]
    public EventController eventController;

    [Header("Game Configuration")]
    public int questionsPerDay = 1;
    [Tooltip("The delay in seconds before showing a new event question.")]
    public float eventDelay = 1.5f;

    // --- Private Fields ---
    private Player player;
    private PlayerControls gameControls;
    private GameEvent currentEvent;
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;
    private bool isGameReady = false;
    private bool isWaitingForSummary;
    private int survivalAtDayStart, happinessAtDayStart, wealthAtDayStart;

    // NEW: A flag to control the coroutine's waiting state.
    private bool isWaitingForChoice = false;

    void Awake()
    {
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
        OnGameStarted?.Invoke();
        BeginNewDay();
    }

    private void BeginNewDay()
    {
        OnDayChanged?.Invoke(currentDay);
        eventController.StartNewDay();

        survivalAtDayStart = player.Survival;
        happinessAtDayStart = player.Happiness;
        wealthAtDayStart = player.Wealth;

        OnStatsUpdated?.Invoke(player);

        StartCoroutine(DailyEventRoutine());
    }

    /// <summary>
    /// This coroutine manages the sequence of presenting questions for a single day.
    /// </summary>
    private IEnumerator DailyEventRoutine()
    {
        while (questionsAnsweredToday < questionsPerDay)
        {
            yield return new WaitForSeconds(eventDelay);

            currentEvent = eventController.GetUniqueEventForDay();
            OnNewEvent?.Invoke(currentEvent);

            // --- FIX: Wait for a choice ---
            // 1. Set the flag to indicate we are now waiting.
            isWaitingForChoice = true;
            // 2. Pause the coroutine until the flag is set back to false (by MakeChoice).
            yield return new WaitUntil(() => !isWaitingForChoice);
        }

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
        // FIX: Add a check to ensure we only process a choice when we are waiting for one.
        if (!isGameReady || isWaitingForSummary || currentEvent == null || !isWaitingForChoice) return;

        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        Player.StatChange finalOutcome = new Player.StatChange();
        finalOutcome.survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);

        player.UpdateStats(finalOutcome);
        OnStatsUpdated?.Invoke(player);

        OnEventConcluded?.Invoke();

        questionsAnsweredToday++;

        // FIX: Un-pause the coroutine by setting the flag to false.
        isWaitingForChoice = false;
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

    private void CheckForGameOver()
    {
        if (player.Survival <= 0 || player.Happiness <= 0)
        {
            isGameReady = false;
            OnGameOver?.Invoke();
        }
    }
}
