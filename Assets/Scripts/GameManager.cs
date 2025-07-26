using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// The central controller and "brain" of the game. This version is fully decoupled
/// from the UIManager and communicates only through events.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- EVENTS (The Observer Pattern) ---

    /// <summary>
    /// FIX: Added the missing event. This broadcasts once when the game logic officially begins.
    /// </summary>
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
    // The direct reference to UIManager is no longer needed for a pure observer pattern.
    // However, since your current file has it, I will leave it but it is unused for initialization.
    public UIManager uiManager;

    [Header("Game Configuration")]
    [Tooltip("The number of questions the player must answer before a day ends.")]
    public int questionsPerDay = 1;

    private Player player;
    private PlayerControls gameControls;
    private GameEvent currentEvent;
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;
    private int week = 0;
    private int previousWeek = -1;
    private int currentGoal = 0;

    //Goal
    [System.Serializable]
    public class Goal
    {
        public string type;
        public string narasi;
        public int value;
    }
    public List<Goal> goals;
    public static bool isStartEvent = false; //for check start event
    public static bool isEndDay= false; //for check if you can end the day
    public static bool isEventRunning = false;

    private bool isGameReady = false;


    void Awake()
    {
        gameControls = new PlayerControls();
        if (eventController == null || uiManager == null) // uiManager check kept for other potential uses
        {
            Debug.LogError("GameManager is missing EventController or UIManager reference!");
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
    }

    private void OnDisable()
    {
        CharacterSpawner.OnPlayerSpawned -= InitializePlayer;
        OnChoiceMade -= MakeChoice;
        gameControls.Gameplay.Disable();
        gameControls.Gameplay.ChooseYes.performed -= ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed -= ReportChoiceFromInput;
        UIManager.OnChoiceButtonPressed -= ReportChoice;
    }

    private void InitializePlayer(Player spawnedPlayer)
    {
        this.player = spawnedPlayer;
        Debug.Log("GameManager has received the player reference: " + spawnedPlayer.name);
        StartGame();
    }

    private void ReportChoiceFromInput(InputAction.CallbackContext context)
    {
        bool choice = context.action == gameControls.Gameplay.ChooseYes;
        ReportChoice(choice);
    }

    private void ReportChoice(bool choice) => OnChoiceMade?.Invoke(choice);

    private void StartGame()
    {
        isGameReady = true;
        currentDay = 1;
        questionsAnsweredToday = 0;
        currentGoal = week % goals.Count;
        // FIX: Instead of a direct call to the UIManager, broadcast the OnGameStarted event.
        // The UIManager is listening for this and will set its own initial state.
        OnGameStarted?.Invoke();

        OnStatsUpdated?.Invoke(player);
        BeginNewDay();
    }

    private void BeginNewDay()
    {
        if (!isGameReady) return;
        OnDayChanged?.Invoke(currentDay);
        if (week != previousWeek)
        {
            OnWeekChanged?.Invoke(week);
            DisplayGoal?.Invoke(goals[currentGoal]);
            previousWeek = week;
        }
        eventController.StartNewDay();
        SelectNewEvent();
        uiManager.DisplayQuestion(isStartEvent);
    }

    private void SelectNewEvent()
    {
        if (!isGameReady) return;
        currentEvent = eventController.GetUniqueEventForDay();
        OnNewEvent?.Invoke(currentEvent);
        
    }

    private void MakeChoice(bool choseYes)
    {
        if (!isGameReady || currentEvent == null) return;
        if (!isStartEvent) return;

        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        /*Player.StatChange finalOutcome = new Player.StatChange();
        finalOutcome.survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);
        player.UpdateStats(finalOutcome);
        OnStatsUpdated?.Invoke(player);*/
        int survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        int happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        int wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);
        ChangePlayerState(survivalChange, happinessChange, wealthChange);
        questionsAnsweredToday++;

        if (questionsAnsweredToday >= questionsPerDay)
        {
            CheckForGameOver();
            if (!isGameReady) return;
        }
        else
        {
            SelectNewEvent();
        }
        isStartEvent = false;
        isEndDay = true;
        uiManager.DisplayQuestion(isStartEvent);

    }

    public void StartEvent()
    {
        currentDay++;
        questionsAnsweredToday = 0;
        isStartEvent = true;
        isEndDay = false;
        BeginNewDay();
    }

    private void CheckForGameOver()
    {
        if (!isGameReady) return;
        //Check Day and Week
        if(currentDay%7 != 0)
        {
            if (player.Survival <= 0 || player.Happiness <= 0)
            {
                GameOver();
            }
        }
        else
        {
            if (player.Wealth <= 0 || player.Survival <= 0 || player.Happiness <= 0)
            {
                GameOver();
            }
            else
            {
                bool success=CheckGoal(goals[currentGoal]);
                if (success)
                {
                    goals[currentGoal].value += 5;
                    week++;
                    currentGoal = week % goals.Count;
                }
            }
        }
        
    }

    private bool CheckGoal(Goal goal)
    {
        switch (goal.type.ToLower())
        {
            case "wealth":
                if (player.Wealth >= goal.value)
                {
                    ChangePlayerState(0, 0, -goal.value);
                    return true;
                }
                break;
            case "happiness":
                if (player.Happiness > goal.value)
                {
                    return true;
                }
                break;
            case "survival":
                if(player.Survival> goal.value)
                {
                    return true;
                }
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

    public void ChangePlayerState(int survival,int happiness,int wealth)
    {
        Player.StatChange finalOutcome = new Player.StatChange();
        finalOutcome.survivalChange = survival;
        finalOutcome.happinessChange = happiness;
        finalOutcome.wealthChange = wealth;
        player.UpdateStats(finalOutcome);
        OnStatsUpdated?.Invoke(player);
    }
}
