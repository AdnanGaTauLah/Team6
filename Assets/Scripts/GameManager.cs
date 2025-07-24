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

    //Goal
    [System.Serializable]
    public class Goal
    {
        public string type;
        public string narasi;
        public int value;
    }
    public List<Goal> goals;

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
        OnWeekChanged?.Invoke(week);
        eventController.StartNewDay();
        SelectNewEvent();
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
            if (!isGameReady) return;
            currentDay++;
            questionsAnsweredToday = 0;
            BeginNewDay();
        }
        else
        {
            SelectNewEvent();
        }
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
                CheckGoal(goals[week]);
            }
        }
        
    }

    private void CheckGoal(Goal goal)
    {
        /*switch (goal.type)
        {
            case "wealth":
                if (goal.value <= player.Wealth)
                {
                    GameOver();
                }
                else
                {
                    
                }
                return
            case "happy":
                return currentHp >= goal.value;

            case "survival":
                return currentHappy >= goal.value;

            default:
                return false;
        }*/
    }

    private void GameOver()
    {
        isGameReady = false;
        OnGameOver?.Invoke();
    }
}
