using UnityEngine;
using System.Collections.Generic; // Required for using Lists
using System.IO;                  // Required for file operations

/// <summary>
/// A helper class that matches the structure of our JSON file.
/// It contains a list of events.
/// </summary>
[System.Serializable]
public class EventList
{
    public List<Event> events;
}

/// <summary>
/// Defines a single game event, including the question and the outcomes for each choice.
/// This is a data container, so it doesn't need to be a MonoBehaviour.
/// [System.Serializable] allows us to see and edit this in the Unity Inspector and use it with JSON.
/// </summary>
[System.Serializable]
public class Event
{
    [Tooltip("The question or scenario text presented to the player.")]
    [TextArea(3, 10)] // Makes the text box bigger in the Inspector for easier editing.
    public string question;

    [Tooltip("The stat changes that will occur if the player chooses 'Yes'.")]
    public Player.StatChange yesOutcome;

    [Tooltip("The stat changes that will occur if the player chooses 'No'.")]
    public Player.StatChange noOutcome;
}

/// <summary>
/// Manages the list of all possible game events. It now loads them from a JSON file
/// and provides a random event to the GameManager when requested.
/// </summary>
public class EventController : MonoBehaviour
{
    // --- ATTRIBUTES ---
    [Header("Data Loading")]
    [Tooltip("The name of the JSON file in the StreamingAssets folder.")]
    public string jsonFileName = "events.json";

    // This list will now be populated from the JSON file instead of the Inspector.
    private List<Event> allEvents;

    private Event currentEvent;

    // --- UNITY LIFECYCLE ---

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// We use this to load our data before the game starts.
    /// </summary>
    void Awake()
    {
        LoadEventsFromJSON();
    }

    // --- METHODS ---

    /// <summary>
    /// Reads the specified JSON file from the StreamingAssets folder,
    /// parses it, and populates the allEvents list.
    /// </summary>
    private void LoadEventsFromJSON()
    {
        // Construct the full path to the file.
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (File.Exists(path))
        {
            // Read the entire file into a string.
            string jsonString = File.ReadAllText(path);

            // Deserialize the JSON string into our EventList object.
            EventList eventData = JsonUtility.FromJson<EventList>(jsonString);

            // Assign the loaded events to our list.
            allEvents = eventData.events;
            Debug.Log(allEvents.Count + " events loaded successfully from JSON.");
        }
        else
        {
            Debug.LogError("Cannot find JSON file at: " + path);
            allEvents = new List<Event>(); // Initialize with an empty list to prevent errors.
        }
    }

    /// <summary>
    /// Selects a random event from the 'allEvents' list.
    /// It ensures that the same event isn't picked twice in a row if there are multiple options.
    /// </-summary>
    /// <returns>A random Event object.</returns>
    public Event GetRandomEvent()
    {
        if (allEvents == null || allEvents.Count == 0)
        {
            Debug.LogError("Event list is empty! Cannot get a random event. Was the JSON loaded correctly?");
            return null;
        }

        if (allEvents.Count == 1)
        {
            return allEvents[0];
        }

        // Simple logic to avoid picking the same event twice in a row
        Event newEvent = allEvents[Random.Range(0, allEvents.Count)];
        while (newEvent == currentEvent)
        {
            newEvent = allEvents[Random.Range(0, allEvents.Count)];
        }

        currentEvent = newEvent;
        return currentEvent;
    }
}
