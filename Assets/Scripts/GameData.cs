using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    /// <summary>
    /// The static instance of this class, allowing easy access from any script.
    /// </summary>
    public static GameData Instance { get; private set; }

    /// <summary>
    /// The mentor figure chosen by the player in the character selection scene.
    /// </summary>
    public Player.MentorFigure selectedMentor;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern to ensure only one instance ever exists.
    /// </summary>
    private void Awake()
    {
        // If an instance of this already exists and it's not this one, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        // Otherwise, set this as the one and only instance and make it survive scene loads.
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
