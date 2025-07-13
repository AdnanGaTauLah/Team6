using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("Component References")]
    [Tooltip("A reference to the Player script in the scene.")]
    public Player player;

    [Tooltip("A reference to the EventController script in the scene.")]
    public EventController eventController;

    // --- INPUT ---
    private PlayerInput gameControls; // The generated C# class for our input actions

    // --- GAME STATE ---
    private Event currentEvent;
    private bool isGameOver = false;

    // --- UNITY LIFECYCLE ---

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It's the ideal place to initialize systems like input.
    /// </summary>
    void Awake()
    {
        // Initialize the input action asset
        gameControls = new PlayerInput();

        // It's good practice to check if references are set in the editor.
        if (player == null || eventController == null)
        {
            Debug.LogError("GameManager is missing references! Assign Player and EventController in the Inspector.");
            this.enabled = false;
            return;
        }
    }

    /// <summary>
    /// OnEnable is called when the object becomes enabled and active.
    /// This is where we subscribe to our input events.
    /// </summary>
    private void OnEnable()
    {
        // Enable the 'Gameplay' action map
        gameControls.Gameplay.Enable();

        // Subscribe our methods to the 'performed' event of each action
        // The '+=' operator adds a listener to the event.
        gameControls.Gameplay.ChooseYes.performed += OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed += OnChooseNo;
    }

    /// <summary>
    /// OnDisable is called when the object becomes disabled or inactive.
    /// It's crucial to unsubscribe from events to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribe from the events to clean up
        // The '-=' operator removes the listener.
        gameControls.Gameplay.ChooseYes.performed -= OnChooseYes;
        gameControls.Gameplay.ChooseNo.performed -= OnChooseNo;

        // Disable the action map
        gameControls.Gameplay.Disable();
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// This is the main entry point for the game.
    /// </summary>
    void Start()
    {
        StartGame();
    }

    // We no longer need the Update() method for input!

    // --- INPUT HANDLERS ---

    /// <summary>
    /// This method is called automatically by the Input System when the 'ChooseYes' action is performed.
    /// </summary>
    private void OnChooseYes(InputAction.CallbackContext context)
    {
        MakeChoice(true);
    }

    /// <summary>
    /// This method is called automatically by the Input System when the 'ChooseNo' action is performed.
    /// </summary>
    private void OnChooseNo(InputAction.CallbackContext context)
    {
        MakeChoice(false);
    }


    // --- CORE GAME LOGIC (No changes needed here) ---

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

    public void MakeChoice(bool choseYes)
    {
        if (isGameOver) return;

        Player.StatChange outcome = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;
        player.UpdateStats(outcome);
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
