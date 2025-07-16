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
    public string type;
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
/// Represents a single component(item, food, or activity)
/// loaded from the components JSON file.
/// </summary>
[System.Serializable]
public class ComponentQuestion
{
    public string type;
    public string name;
}

///<summary>
/// containing a list of all possible components.
/// </summary>
[System.Serializable]
public class ComponentQuestionList
{
    public List<ComponentQuestion> components;
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

    public string detailFileJson= "components.json";
    private List<ComponentQuestion> detailQuestions;

    // --- UNITY LIFECYCLE ---
    void Awake()
    {
        LoadEventsFromJSON();
        LoadDetailFromJSON();
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

    void LoadDetailFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, detailFileJson);

        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            ComponentQuestionList detailData = JsonUtility.FromJson<ComponentQuestionList>(jsonString);
            detailQuestions = detailData.components;
            Debug.Log(detailQuestions.Count + " details loaded successfully from JSON.");
        }
        else
        {
            Debug.LogError("Cannot find details JSON file at: " + path);
            detailQuestions = new List<ComponentQuestion>();
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

        //Check type event,  apply detail if event.type != "unique"
        if (newEvent.type != "unique")
        {
            return GenerateRandomQuestion(newEvent);
        }
        else
        {
            return newEvent;
        }
    }

    /// <summary>
    /// Clears the list of used events. Called by the GameManager at the start of a new day.
    /// </summary>
    public void StartNewDay()
    {
        eventsUsedThisDay.Clear();
    }

    /// <summary>
    /// Makes a copy of the event, replaces {component} with random detail based on type.
    /// </summary>
    public GameEvent GenerateRandomQuestion(GameEvent ge)
    {
        List<ComponentQuestion> matchingDetails = detailQuestions.FindAll(e => e.type == ge.type);
        if(matchingDetails != null && matchingDetails.Count > 0)
        {
            ComponentQuestion randomComponent = matchingDetails[Random.Range(0, matchingDetails.Count)];
            ge.question= ge.question.Replace("{component}", randomComponent.name);
            return ge;
        }
        else
        {
            Debug.LogWarning("No matching components found for type: " + ge.type);
            return ge;
        }
    }
}
