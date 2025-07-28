using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Defines the possible mentor figures the player can choose.
    /// This enum is what the CharacterSpawner script is looking for.
    /// </summary>
    public enum MentorFigure
    {
        None,
        Father,
        Mother
    }

    // --- PRIVATE FIELDS ---
    [Header("Player Resources")]
    [Tooltip("Represents the player's health and physical well-being. Game over if this drops below 0.")]
    [SerializeField] private int _survival = 15;
    [Tooltip("Represents the player's mental state and joy. Game over if this drops below 0.")]
    [SerializeField] private int _happiness = 15;
    [Tooltip("Represents the player's financial resources. Game over if this drops below 0.")]
    [SerializeField] private int _wealth = 15;

    // This field will store the choice made in the menu.
    private MentorFigure _chosenMentor = MentorFigure.None;

    // --- PUBLIC PROPERTIES (Getters) ---
    public int Survival => _survival;
    public int Happiness => _happiness;
    public int Wealth => _wealth;
    public MentorFigure ChosenMentor => _chosenMentor;


    /// <summary>
    /// A simple data structure used to pass stat modifications between scripts.
    /// </summary>
    [System.Serializable]
    public struct StatChange
    {
        public int survivalChange;
        public int happinessChange;
        public int wealthChange;
    }

    /// <summary>
    /// The sole public method for modifying the player's stats.
    /// </summary>
    /// <param name="outcome">A StatChange object containing the values to add to the current stats.</param>
    public void UpdateStats(StatChange outcome)
    {
        _survival += outcome.survivalChange;
        _happiness += outcome.happinessChange;
        _wealth += outcome.wealthChange;
    }

    /// <summary>
    /// Sets the chosen mentor figure for the player. This is called by the GameManager
    /// after the player is spawned.
    /// </summary>
    /// <param name="mentor">The mentor figure chosen by the player.</param>
    public void SetMentor(MentorFigure mentor)
    {
        _chosenMentor = mentor;
        Debug.Log("Mentor has been set to: " + mentor);
    }
}
