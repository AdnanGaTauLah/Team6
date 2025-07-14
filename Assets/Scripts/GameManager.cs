using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("Component References")]
    public Player player;
    public EventController eventController;
    public UIManager uiManager; // Reference to the UI Manager

    // --- INPUT ---
    private PlayerControls gameControls;

    // --- GAME STATE ---
    private GameEvent currentEvent;
    private bool isGameOver = false;

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

        // Setup UI connections and initial state
        uiManager.SetupButtonListeners(this);
        uiManager.HideGameOverScreen();
        uiManager.UpdateStatsDisplay(player);

        SelectNewEvent();
    }

    private void SelectNewEvent()
    {
        currentEvent = eventController.GetRandomEvent();
        // Update the UI instead of logging to the console
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

        if (!isGameOver)
        {
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
