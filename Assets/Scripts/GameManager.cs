using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("Component References")]
    [Tooltip("A reference to the Player script in the scene.")]
    public Player player;

    [Tooltip("A reference to the EventController script in the scene.")]
    public EventController eventController;

    // This would be a reference to your UI manager script
    // public UIManager uiManager;

    // --- GAME STATE ---
    private Event currentEvent;
    private bool isGameOver = false;

    // --- UNITY LIFECYCLE ---

    /// <summary>
    /// Called when the script instance is being loaded.
    /// This is the main entry point for the game.
    /// </summary>
    void Start()
    {
        // It's good practice to check if references are set in the editor.
        if (player == null || eventController == null)
        {
            Debug.LogError("GameManager is missing references! Assign Player and EventController in the Inspector.");
            // Disable the component to prevent further errors.
            this.enabled = false;
            return;
        }

        StartGame();
    }

    /// <summary>
    /// Contains temporary logic for testing the game flow using keyboard input.
    /// </summary>
    void Update()
    {
        // Don't do anything if the game is over.
        if (isGameOver)
        {
            return;
        }

        // --- TEST INPUT ---
        // Press 'Y' to simulate choosing "Yes"
        if (Input.GetKeyDown(KeyCode.Y))
        {
            MakeChoice(true);
        }

        // Press 'N' to simulate choosing "No"
        if (Input.GetKeyDown(KeyCode.N))
        {
            MakeChoice(false);
        }
    }


    // --- CORE GAME LOGIC ---

    /// <summary>
    /// Initializes the game state and starts the first event.
    /// </summary>
    private void StartGame()
    {
        Debug.Log("Game Started!");
        isGameOver = false;
        // In a full game, you might reset player stats here.
        SelectNewEvent();
    }

    /// <summary>
    /// Gets a new random event from the EventController and displays it.
    /// </summary>
    private void SelectNewEvent()
    {
        currentEvent = eventController.GetRandomEvent();

        // Log the new question to the console for now.
        // Later, you would pass this to the UIManager.
        Debug.Log("NEW EVENT: " + currentEvent.question);
        Debug.Log("Press 'Y' for Yes, 'N' for No.");
    }

    /// <summary>
    /// Processes the player's choice for the current event.
    /// </summary>
    /// <param name="choseYes">True if the player chose 'Yes', false for 'No'.</param>
    public void MakeChoice(bool choseYes)
    {
        if (isGameOver) return; // Safety check

        // Determine which outcome to use based on the choice
        Player.StatChange outcome = choseYes ? currentEvent.yesOutcome : currentEvent.noOutcome;

        // Apply the outcome to the player's stats
        player.UpdateStats(outcome);

        // Check if the game should end
        CheckForGameOver();

        // If the game is not over, get the next event
        if (!isGameOver)
        {
            SelectNewEvent();
        }
    }

    /// <summary>
    /// Checks if any of the player's resources have dropped below zero.
    /// If so, it triggers the game over state.
    /// </summary>
    private void CheckForGameOver()
    {
        if (player.survival < 0 || player.happiness < 0 || player.wealth < 0)
        {
            isGameOver = true;
            // In a real game, you would call a method in your UIManager to show a game over screen.
            Debug.LogWarning("--- GAME OVER ---");
            Debug.LogWarning($"Final Stats: Survival={player.survival}, Happiness={player.happiness}, Wealth={player.wealth}");
        }
    }
}
