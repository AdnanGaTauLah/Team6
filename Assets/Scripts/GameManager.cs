using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("Component References")]
    public Player player;
    public EventController eventController;

    // --- INPUT ---
    // Renamed to PlayerControls to avoid conflicts with Unity's PlayerInput component.
    private PlayerControls gameControls;

    // --- GAME STATE ---
    private GameEvent currentEvent; // This now holds the event with stat ranges
    private bool isGameOver = false;

    // --- UNITY LIFECYCLE ---
    void Awake()
    {
        // Initialize with the new, non-conflicting class name.
        gameControls = new PlayerControls();

        if (player == null || eventController == null)
        {
            Debug.LogError("GameManager is missing references!");
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        gameControls.Gameplay.Enable();
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
        SelectNewEvent();
    }

    private void SelectNewEvent()
    {
        currentEvent = eventController.GetRandomEvent();
        Debug.Log("NEW EVENT: " + currentEvent.question);
        Debug.Log("Press 'Y' for Yes, 'N' for No.");
    }

    /// <summary>
    /// Processes the player's choice, randomizes the outcome, and applies it.
    /// </summary>
    public void MakeChoice(bool choseYes)
    {
        if (isGameOver) return;

        // 1. Get the correct outcome containing the min/max ranges
        EventOutcome outcomeWithRanges = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;

        // 2. Create a new StatChange object to hold the final, randomized values
        Player.StatChange finalOutcome = new Player.StatChange();

        // 3. Randomize each stat and assign it to our final outcome object.
        // Note: Random.Range for integers has an exclusive upper bound, so we add 1 to the max value.
        finalOutcome.survivalChange = Random.Range(outcomeWithRanges.survivalChange.min, outcomeWithRanges.survivalChange.max + 1);
        finalOutcome.happinessChange = Random.Range(outcomeWithRanges.happinessChange.min, outcomeWithRanges.happinessChange.max + 1);
        finalOutcome.wealthChange = Random.Range(outcomeWithRanges.wealthChange.min, outcomeWithRanges.wealthChange.max + 1);

        Debug.Log($"Outcome: Survival({finalOutcome.survivalChange}), Happiness({finalOutcome.happinessChange}), Wealth({finalOutcome.wealthChange})");

        // 4. Apply the single, randomized stats to the player
        player.UpdateStats(finalOutcome);

        // 5. Check for game over and select the next event
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
            Debug.LogWarning("--- GAME OVER ---");
            Debug.LogWarning($"Final Stats: Survival={player.survival}, Happiness={player.happiness}, Wealth={player.wealth}");
        }
    }
}
