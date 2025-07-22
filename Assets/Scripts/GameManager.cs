using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The central controller and "brain" of the game. This version is fully decoupled
/// from the UIManager and communicates only through events.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- EVENTS (The Observer Pattern) ---

    /// <summary>
    /// Broadcasts once when the game logic officially begins.
    /// </summary>
    public static event Action OnGameStarted;

    public static event Action<int> OnDayChanged;
    public static event Action<Player> OnStatsUpdated;
    public static event Action<GameEvent> OnNewEvent;
    public static event Action OnGameOver;
    private event Action<bool> OnChoiceMade;


    [Header("Component References")]
    public Player player;
    public EventController eventController;
    // The direct reference to UIManager has been removed for full decoupling.

    [Header("Game Configuration")]
    public int questionsPerDay = 3;

    private PlayerControls gameControls;
    private GameEvent currentEvent;
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;
    private bool isGameOver = false;

    void Awake()
    {
        gameControls = new PlayerControls();
        // The check for uiManager is no longer needed.
        if (player == null || eventController == null)
        {
            Debug.LogError("GameManager is missing Player or EventController reference!");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        OnChoiceMade += MakeChoice;
        gameControls.Gameplay.Enable();
        gameControls.Gameplay.ChooseYes.performed += ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed += ReportChoiceFromInput;
        UIManager.OnChoiceButtonPressed += ReportChoice;
    }

    private void OnDisable()
    {
        OnChoiceMade -= MakeChoice;
        gameControls.Gameplay.Disable();
        gameControls.Gameplay.ChooseYes.performed -= ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed -= ReportChoiceFromInput;
        UIManager.OnChoiceButtonPressed -= ReportChoice;
    }

    void Start() => StartGame();

    private void ReportChoiceFromInput(InputAction.CallbackContext context)
    {
        bool choice = context.action == gameControls.Gameplay.ChooseYes;
        ReportChoice(choice);
    }

    private void ReportChoice(bool choice)
    {
        OnChoiceMade?.Invoke(choice);
    }

    private void StartGame()
    {
        isGameOver = false;
        currentDay = 1;
        questionsAnsweredToday = 0;

        // REFACTOR: Instead of a direct call, broadcast that the game has started.
        // Any system that cares (like the UI) can listen for this.
        OnGameStarted?.Invoke();

        OnStatsUpdated?.Invoke(player);
        BeginNewDay();
    }

    private void BeginNewDay()
    {
        OnDayChanged?.Invoke(currentDay);
        eventController.StartNewDay();
        SelectNewEvent();
    }

    private void SelectNewEvent()
    {
        currentEvent = eventController.GetUniqueEventForDay();
        OnNewEvent?.Invoke(currentEvent);
    }

    private void MakeChoice(bool choseYes)
    {
        if (isGameOver || currentEvent == null) return;

        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        Player.StatChange finalOutcome = new Player.StatChange();

        finalOutcome.survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);

        player.UpdateStats(finalOutcome);
        OnStatsUpdated?.Invoke(player);
        questionsAnsweredToday++;

        if (questionsAnsweredToday >= questionsPerDay)
        {
            CheckForGameOver();
            if (!isGameOver)
            {
                currentDay++;
                questionsAnsweredToday = 0;
                BeginNewDay();
            }
        }
        else
        {
            SelectNewEvent();
        }
    }

    private void CheckForGameOver()
    {
        if (player.Survival < 0 || player.Happiness < 0 || player.Wealth < 0)
        {
            isGameOver = true;
            OnGameOver?.Invoke();
        }
    }
}
