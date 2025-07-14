using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("Component References")]
    public Player player;
    public EventController eventController;
    public UIManager uiManager; // Reference to the UI Manager

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

        if (player == null || eventController == null || uiManager == null)
        {
            Debug.LogError("GameManager is missing references! Assign Player, EventController, and UIManager in the Inspector.");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        gameControls.Gameplay.Enable();
        // We keep keyboard controls for quick testing
        gameControls.Gameplay.ChooseYes.performed += OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed += OnChooseNo;
    }

    private void OnDisable()
    {
        gameControls.Gameplay.ChooseYes.performed -= OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed -= OnChooseNo;
        gameControls.Gameplay.Disable();
    }

    void Start()
    {
        StartGame();
    }

    // --- INPUT HANDLERS ---
    private void OnChooseYes(InputAction.CallbackContext context) => MakeChoice(true);
    private void OnChooseNo(InputAction.CallbackContext context) => MakeChoice(false);

    // --- CORE GAME LOGIC ---
    private void StartGame()
    {
        Debug.Log("Game Started!");
        isGameOver = false;
        currentDay = 1;
        questionsAnsweredToday = 0;

        // Setup UI connections and initial state
        uiManager.SetupButtonListeners(this);
        uiManager.HideGameOverScreen();
        uiManager.UpdateStatsDisplay(player);

        BeginNewDay();
    }

    /// <summary>
    /// Sets up the start of a new day.
    /// </summary>
    private void BeginNewDay()
    {
        Debug.Log("--- Starting Day " + currentDay + " ---");
        uiManager.UpdateDayDisplay(currentDay);
        eventController.StartNewDay(); // Reset the list of used events
        SelectNewEvent();
    }

    private void SelectNewEvent()
    {
        currentEvent = eventController.GetUniqueEventForDay();
        uiManager.DisplayEvent(currentEvent);
    }

    /// <summary>
    /// Processes the player's choice, randomizes the outcome, and applies it.
    /// </summary>
    public void MakeChoice(bool choseYes)
    {
        if (isGameOver) return;

        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        Player.StatChange finalOutcome = new Player.StatChange();

        finalOutcome.survivalChange = Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);

        player.UpdateStats(finalOutcome);

        // Immediately update the UI to show the new stat values
        uiManager.UpdateStatsDisplay(player);

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
        if (player.survival < 0 || player.happiness < 0 || player.wealth < 0)
        {
            isGameOver = true;
            // Show the game over screen
            uiManager.ShowGameOverScreen();
        }
    }
}
