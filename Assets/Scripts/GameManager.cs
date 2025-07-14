using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // --- EVENTS (The Observer Pattern) ---
    // These are the "broadcasts" other scripts can listen to.
    public static event Action<int> OnDayChanged;
    public static event Action<Player> OnStatsUpdated;
    public static event Action<GameEvent> OnNewEvent;
    public static event Action OnGameOver;
    public static event Action<bool> OnChoiceMade;

    // --- REFERENCES ---
    [Header("Component References")]
    public Player player;
    public EventController eventController;

    [Header("Game Configuration")]
    public int questionsPerDay = 3;

    // --- INPUT ---
    private PlayerControls gameControls;

    // --- GAME STATE ---
    private GameEvent currentEvent;
    private bool isGameOver = false;

    // --- Day Cycle State ---
    private int currentDay = 1;
    private int questionsAnsweredToday = 0;

    // --- UNITY LIFECYCLE ---
    void Awake()
    {
        gameControls = new PlayerControls();

        if (player == null || eventController == null)
        {
            Debug.LogError("GameManager is missing references! Assign Player and EventController in the Inspector.");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        // Listen to its own internal event
        OnChoiceMade += MakeChoice;

        gameControls.Gameplay.Enable();
        // We keep keyboard controls for quick testing
        gameControls.Gameplay.ChooseYes.performed += OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed += OnChooseNo;

        // Listen for choices coming from the UI's new event
        UIManager.OnChoiceButtonPressed += ReportChoice;
    }

    private void OnDisable()
    {
        OnChoiceMade -= MakeChoice;

        gameControls.Gameplay.ChooseYes.performed -= OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed -= OnChooseNo;
        
        OnChoiceMade -= MakeChoice;
        gameControls.Gameplay.Disable();

        // Stop listening for UI events
        UIManager.OnChoiceButtonPressed -= ReportChoice;
    }

    void Start()
    {
        StartGame();
    }

    // --- Input Handling ---
    private void ReportChoiceFromInput(InputAction.CallbackContext context)
    {
        // This method handles the context from the Input System
        bool choice = context.action == gameControls.Gameplay.ChooseYes;
        ReportChoice(choice);
    }

    private void ReportChoice(bool choice)
    {
        // This single method is called by either keyboard OR UI events
        // It then invokes the private event that the game logic listens to
        OnChoiceMade?.Invoke(choice);
    }
    // Input handlers now just fire the event
    private void OnChooseYes(InputAction.CallbackContext context) => OnChoiceMade?.Invoke(true);
    private void OnChooseNo(InputAction.CallbackContext context) => OnChoiceMade?.Invoke(false);

    // --- CORE GAME LOGIC ---
    private void StartGame()
    {
        Debug.Log("Game Started!");
        isGameOver = false;
        currentDay = 1;
        questionsAnsweredToday = 0;

        // Broadcast the initial state
        OnStatsUpdated?.Invoke(player);

        BeginNewDay();
    }

    /// <summary>
    /// Sets up the start of a new day.
    /// </summary>
    private void BeginNewDay()
    {
        Debug.Log("--- Starting Day " + currentDay + " ---");
        OnDayChanged?.Invoke(currentDay);
        eventController.StartNewDay(); // Reset the list of used events
        SelectNewEvent();
    }

    private void SelectNewEvent()
    {
        currentEvent = eventController.GetUniqueEventForDay();
        OnNewEvent?.Invoke(currentEvent); // Broadcast the new event
    }

    /// <summary>
    /// Processes the player's choice, randomizes the outcome, and applies it.
    /// </summary>
    public void MakeChoice(bool choseYes)
    {
        if (isGameOver) return;

        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        Player.StatChange finalOutcome = new Player.StatChange();

        finalOutcome.survivalChange = UnityEngine.Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = UnityEngine.Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = UnityEngine.Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);

        player.UpdateStats(finalOutcome);

        // Broadcast that stats have been updated
        OnStatsUpdated?.Invoke(player);

        CheckForGameOver();

        // --- New Day Logic ---
        questionsAnsweredToday++;
        Debug.Log("Answered question " + questionsAnsweredToday + " of " + questionsPerDay);

        if (questionsAnsweredToday >= questionsPerDay)
        {
            // End of the day, check for game over
            Debug.Log("End of day. Checking for game over.");
            CheckForGameOver();

            if (!isGameOver)
            {
                // If not game over, start the next day
                currentDay++;
                questionsAnsweredToday = 0;
                BeginNewDay();
            }
        }
        else
        {
            // It's not the end of the day, just get the next question
            SelectNewEvent();
        }
    }

    private void CheckForGameOver()
    {
        if (player.Survival < 0 || player.Happiness < 0 || player.Wealth < 0)
        {
            isGameOver = true;
            OnGameOver?.Invoke(); // Broadcast that the game is over
        }
    }
}
