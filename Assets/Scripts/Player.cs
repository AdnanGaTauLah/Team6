using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // --- ATTRIBUTES ---
    // These are public so you can see and edit their starting values in the Unity Inspector.
    [Header("Player Resources")]
    [Tooltip("Represents the player's health and physical well-being. Game over if this drops below 0.")]
    [SerializeField] private int _survival = 50;

    [Tooltip("Represents the player's mental state and joy. Game over if this drops below 0.")]
    [SerializeField] private int _happiness = 50;

    [Tooltip("Represents the player's financial resources. Game over if this drops below 0.")]
    [SerializeField] private int _wealth = 50;

    // --- PUBLIC PROPERTIES (The "Getters") ---
    // We expose the data through public "properties" that can only be read, not set, from the outside.
    // This is the public-facing, safe way to access the stats.
    public int Survival => _survival;
    public int Happiness => _happiness;
    public int Wealth => _wealth;

    // --- METHODS ---

    /// <summary>
    /// A simple data structure to hold the changes for each stat.
    /// This makes it easy to pass around the consequences of a player's choice.
    /// </summary>
    [System.Serializable]
    public struct StatChange
    {
        public int survivalChange;
        public int happinessChange;
        public int wealthChange;
    }

    /// <summary>
    /// Updates the player's stats based on the outcome of an event choice.
    /// This method will be called by the GameManager after the player makes a decision.
    /// </summary>
    /// <param name="outcome">A StatChange object containing the values to add to the current stats.</param>
    public void UpdateStats(StatChange outcome)
    {
        _survival += outcome.survivalChange;
        _happiness += outcome.happinessChange;
        _wealth += outcome.wealthChange;

        // Optional: You can add logging to see the changes in the console for debugging.
        Debug.Log($"Stats Updated: Survival={Survival}, Happiness={Happiness}, Wealth={Wealth}");
    }

    // --- UNITY LIFECYCLE (for testing) ---

    // Example of how you might test this script in the editor.
    // You can press the 'U' key to simulate an event outcome.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            // Create a sample outcome for testing purposes.
            StatChange testOutcome = new StatChange
            {
                survivalChange = -5,
                happinessChange = 10,
                wealthChange = -2
            };

            // Call the UpdateStats method
            UpdateStats(testOutcome);
        }
    }
}
