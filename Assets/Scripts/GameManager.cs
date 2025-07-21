using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // --- EVENTS (The Observer Pattern) ---

    /// <summary>
    /// Broadcasts when a new day begins. The integer payload is the new day number.
    /// </summary>
    public static event Action<int> OnDayChanged;

    /// <summary>
    /// Broadcasts whenever the player's stats have been modified. The payload is the Player object itself.
    /// </summary>
    public static event Action<Player> OnStatsUpdated;

    /// <summary>
    /// Broadcasts when a new event/question has been selected and is ready to be displayed.
    /// </summary>
    public static event Action<GameEvent> OnNewEvent;

    /// <summary>
    /// Broadcasts when the game's lose condition has been met.
    /// </summary>
    public static event Action OnGameOver;

    /// <summary>
    /// A private event used internally to trigger the choice logic.
    /// This is invoked by both keyboard input and UI button presses.
    /// </summary>
    private event Action<bool> OnChoiceMade;


    [Header("Component References")]
    [Tooltip("A reference to the Player script in the scene.")]
    public Player player;

    [Tooltip("A reference to the EventController script in the scene.")]
    public EventController eventController;

    // FIX: Added the missing public reference for the UIManager.
    [Tooltip("A reference to the UIManager script in the scene.")]
    public UIManager uiManager;

    [Header("Game Configuration")]
    [Tooltip("The number of questions the player must answer before a day ends.")]
    public int questionsPerDay = 3;

    // --- Private Fields ---
    private PlayerControls gameControls;
    private GameEvent currentEvent;
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;
    private bool isGameOver = false;
    private int week = 1;

    public List<int> goals;
    /// <summary>
    /// Called when the script instance is being loaded. Used for initialization.
    /// </summary>
    void Awake()
    {
        gameControls = new PlayerControls();
        // FIX: Added a check for the uiManager reference.
        if (player == null || eventController == null || uiManager == null)
        {
            Debug.LogError("GameManager is missing one or more references! Assign Player, EventController, and UIManager in the Inspector.");
            this.enabled = false;
        }
    }

    /// <summary>
    /// Called when the object becomes enabled and active. Used to subscribe to events.
    /// </summary>
    private void OnEnable()
    {
        OnChoiceMade += MakeChoice;

        gameControls.Gameplay.Enable();
        gameControls.Gameplay.ChooseYes.performed += ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed += ReportChoiceFromInput;

        UIManager.OnChoiceButtonPressed += ReportChoice;
    }

    /// <summary>
    /// Called when the object becomes disabled or inactive. Used to unsubscribe from events to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        OnChoiceMade -= MakeChoice;

        gameControls.Gameplay.Disable();
        gameControls.Gameplay.ChooseYes.performed -= ReportChoiceFromInput;
        gameControls.Gameplay.ChooseNo.performed -= ReportChoiceFromInput;

        UIManager.OnChoiceButtonPressed -= ReportChoice;
    }

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    void Start() => StartGame();

    // --- Input Handling ---

    /// <summary>
    /// Receives the callback from the Input System (keyboard/gamepad) and determines the choice.
    /// </summary>
    /// <param name="context">The context provided by the Input Action.</param>
    private void ReportChoiceFromInput(InputAction.CallbackContext context)
    {
        bool choice = context.action == gameControls.Gameplay.ChooseYes;
        ReportChoice(choice);
    }

    /// <summary>
    /// A universal method that receives a choice from any source (keyboard or UI) and invokes the internal game logic event.
    /// </summary>
    /// <param name="choice">True if the player chose 'Yes', false if 'No'.</param>
    private void ReportChoice(bool choice)
    {
        OnChoiceMade?.Invoke(choice);
    }


    // --- Core Game Logic ---

    /// <summary>
    /// Initializes the game state and starts the first day.
    /// </summary>
    private void StartGame()
    {
        isGameOver = false;
        currentDay = 1;
        questionsAnsweredToday = 0;
        OnStatsUpdated?.Invoke(player);
        BeginNewDay();
    }

    /// <summary>
    /// Sets up the start of a new day and broadcasts the relevant events.
    /// </summary>
    private void BeginNewDay()
    {
        OnDayChanged?.Invoke(currentDay);
        eventController.StartNewDay();
        SelectNewEvent();
    }

    /// <summary>
    /// Fetches a new unique event from the EventController and broadcasts it.
    /// </summary>
    private void SelectNewEvent()
    {
        currentEvent = eventController.GetUniqueEventForDay();
        OnNewEvent?.Invoke(currentEvent);
    }

    /// <summary>
    /// The main logic handler for a player's choice. Calculates outcomes and advances the game state.
    /// </summary>
    /// <param name="choseYes">The choice made by the player.</param>
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
            /*if (!isGameOver)
            {
                currentDay++;
                questionsAnsweredToday = 0;
                BeginNewDay();
            }*/
        }
        else
        {
            SelectNewEvent();
        }
    }

    /// <summary>
    /// Checks if any player stat has fallen below zero, ending the game if necessary.
    /// </summary>
    private void CheckForGameOver()
    {
        
        if (currentDay < 7)
        {
            if (player.Survival <= 0 || player.Happiness <= 0)
            {
                GameOver();
             
            }
            currentDay++;
            BeginNewDay();
        }
        else
        {
            if (player.Wealth < goals[week-1])
            {
                GameOver();
            }
            week++;
            currentDay = 1;
            BeginNewDay();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        isGameOver = true;
        OnGameOver?.Invoke();
    }
}
