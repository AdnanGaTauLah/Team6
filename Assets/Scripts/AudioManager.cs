using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A persistent Singleton that manages the background music for the entire game.
/// Ensures that the BGM continues to play smoothly across scene changes.
/// </summary>
[RequireComponent(typeof(AudioSource))] // Ensures this object always has an AudioSource
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of this class, allowing easy access from any script.
    /// </summary>
    public static AudioManager Instance { get; private set; }

    [Header("Audio Configuration")]
    [Tooltip("The audio clip to be used as the background music.")]
    [SerializeField] private AudioClip backgroundMusic;

    private AudioSource audioSource;

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
            return; // Exit early
        }

        // Otherwise, set this as the one and only instance and make it survive scene loads.
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get the AudioSource component that is required to be on this GameObject.
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic; // Assign the BGM clip
            audioSource.loop = true;            // Make sure the music loops
            audioSource.Play();                 // Start playing the music
        }
        else
        {
            Debug.LogWarning("AudioManager: No background music clip has been assigned!");
        }
    }
}
