using UnityEngine;
using System.Collections.Generic;
using System.IO;

// --- DATA STRUCTURES TO MATCH JSON ---

/// <summary>
/// Represents a range for a single stat change.
/// </summary>
[System.Serializable]
public struct StatRange
{
    public int min;
    public int max;
}

/// <summary>
/// Represents the outcome of a choice, with ranges for each stat.
/// </summary>
[System.Serializable]
public struct EventOutcome
{
    public StatRange survivalChange;
    public StatRange happinessChange;
    public StatRange wealthChange;
}

/// <summary>
/// Represents a single event loaded from JSON, with outcome ranges.
/// I've named it GameEvent to avoid conflicts with Unity's built-in Event class.
/// </summary>
[System.Serializable]
public class GameEvent
{
    public string question;
    public EventOutcome yesOutcome;
    public EventOutcome noOutcome;
}

/// <summary>
/// A helper class that matches the top-level structure of our JSON file.
/// </summary>
[System.Serializable]
public class GameEventList
{
    public List<GameEvent> events;
}


/// <summary>
/// Manages loading the list of all possible game events from a JSON file.
/// </summary>
public class EventController : MonoBehaviour
{
    // --- ATTRIBUTES ---
    [Header("Data Loading")]
    [Tooltip("The name of the JSON file in the StreamingAssets folder.")]
    public string jsonFileName = "events.json";

    private List<GameEvent> allEvents;
    private List<GameEvent> eventsUsedThisDay = new List<GameEvent>();

    // --- UNITY LIFECYCLE ---
    void Awake()
    {
        LoadEventsFromJSON();
    }

    // --- METHODS ---
    private void LoadEventsFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            GameEventList eventData = JsonUtility.FromJson<GameEventList>(jsonString);
            allEvents = eventData.events;
            Debug.Log(allEvents.Count + " events loaded successfully from JSON.");
        }
        else
        {
            Debug.LogError("Cannot find JSON file at: " + path);
            allEvents = new List<GameEvent>();
        }
    }

    /// <summary>
    /// Gets a random event that has not yet been used today.
    /// </summary>
    public GameEvent GetUniqueEventForDay()
    {
        if (allEvents == null || allEvents.Count == 0) return null;

        // If we've used up all available events, reset the daily list to avoid an infinite loop.
        if (eventsUsedThisDay.Count >= allEvents.Count)
        {
            eventsUsedThisDay.Clear();
        }

        GameEvent newEvent;
        do
        {
            newEvent = allEvents[Random.Range(0, allEvents.Count)];
        } while (eventsUsedThisDay.Contains(newEvent)); // Keep picking until we find one not used today

        eventsUsedThisDay.Add(newEvent); // Add the new event to the list of used events for this day
        return newEvent;
    }

    /// <summary>
    /// Clears the list of used events. Called by the GameManager at the start of a new day.
    /// </summary>
    public void StartNewDay()
    {
        eventsUsedThisDay.Clear();
    }
}
