using System;
using UnityEngine;

/// <summary>
/// Responsible for spawning the correct player character prefab into the scene
/// at a character-specific spawn point.
/// </summary>
public class CharacterSpawner : MonoBehaviour
{
    /// <summary>
    /// Broadcasts an event containing a reference to the newly spawned player object.
    /// The GameManager listens for this to know who the player is.
    /// </summary>
    public static event Action<Player> OnPlayerSpawned;

    [Header("Character Prefabs")]
    [Tooltip("The Prefab for the Father character.")]
    [SerializeField] private GameObject fatherPrefab;

    [Tooltip("The Prefab for the Mother character.")]
    [SerializeField] private GameObject motherPrefab;

    [Header("Spawn Points")]
    [Tooltip("An empty GameObject that marks the position where the Father should be spawned.")]
    [SerializeField] private Transform fatherSpawnPoint;

    [Tooltip("An empty GameObject that marks the position where the Mother should be spawned.")]
    [SerializeField] private Transform motherSpawnPoint;

    /// <summary>
    /// Called when the script is first loaded.
    /// </summary>
    void Start()
    {
        SpawnCharacter();
    }

    /// <summary>
    /// Checks the persistent GameData for the player's choice, instantiates the
    /// corresponding prefab at the correct spawn point, and broadcasts an event.
    /// </summary>
    private void SpawnCharacter()
    {
        if (fatherPrefab == null || motherPrefab == null)
        {
            Debug.LogError("Character prefabs are not assigned in the CharacterSpawner!");
            return;
        }
        if (fatherSpawnPoint == null || motherSpawnPoint == null)
        {
            Debug.LogError("One or more Spawn Points are not assigned in the CharacterSpawner!");
            return;
        }

        GameObject prefabToSpawn = null;
        Transform spawnPoint = null;
        Player.MentorFigure chosenMentor = Player.MentorFigure.None;

        if (GameData.Instance != null)
        {
            chosenMentor = GameData.Instance.selectedMentor;
        }

        // --- LOGIC FIX: Inverted Spawning ---
        // The logic has been swapped to meet the new requirement.
        switch (chosenMentor)
        {
            case Player.MentorFigure.Father:
                // When Father is chosen, spawn the MOTHER prefab at the MOTHER's spawn point.
                prefabToSpawn = motherPrefab;
                spawnPoint = motherSpawnPoint;
                break;
            case Player.MentorFigure.Mother:
                // When Mother is chosen, spawn the FATHER prefab at the FATHER's spawn point.
                prefabToSpawn = fatherPrefab;
                spawnPoint = fatherSpawnPoint;
                break;
            default:
                Debug.LogWarning("No mentor chosen or GameData not found. Spawning Mother by default as a fallback.");
                prefabToSpawn = motherPrefab; // Default fallback
                spawnPoint = motherSpawnPoint;
                break;
        }

        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        Player playerComponent = playerInstance.GetComponent<Player>();

        if (playerComponent != null)
        {
            OnPlayerSpawned?.Invoke(playerComponent);
        }
        else
        {
            Debug.LogError($"Spawned character prefab '{playerInstance.name}' does not have a Player component!");
        }
    }
}
