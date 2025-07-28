using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

// --- (Your data structures remain the same) ---
[System.Serializable] public struct StatRange { public int min; public int max; }
[System.Serializable] public struct EventOutcome { public StatRange survivalChange; public StatRange happinessChange; public StatRange wealthChange; }
[System.Serializable] public class GameEvent { public string type; public string question; public EventOutcome yesOutcome; public EventOutcome noOutcome; }
[System.Serializable] public class GameEventList { public List<GameEvent> events; }
[System.Serializable] public class ComponentQuestion { public string type; public string name; }
[System.Serializable] public class ComponentQuestionList { public List<ComponentQuestion> components; }


public class EventController : MonoBehaviour
{
    [Header("Data Loading")]
    [Tooltip("The name of the main events JSON file in StreamingAssets.")]
    [SerializeField] private string eventsFileName = "events.json";
    [Tooltip("The name of the components JSON file in StreamingAssets.")]
    [SerializeField] private string componentsFileName = "components.json"; // Renamed for clarity

    public bool IsReady { get; private set; } = false;

    private List<GameEvent> allEvents;
    private List<GameEvent> eventsUsedThisDay = new List<GameEvent>();
    private List<ComponentQuestion> detailQuestions;

    void Awake()
    {
        StartCoroutine(LoadGameData());
    }

    /// <summary>
    /// REFACTORED: Master coroutine that now calls specific loading methods.
    /// </summary>
    private IEnumerator LoadGameData()
    {
        // Initialize lists to prevent errors if loading fails.
        allEvents = new List<GameEvent>();
        detailQuestions = new List<ComponentQuestion>();

        // Load both files. The order doesn't matter.
        yield return StartCoroutine(LoadJsonData<GameEventList>(eventsFileName, (data) => allEvents = data.events));
        yield return StartCoroutine(LoadJsonData<ComponentQuestionList>(componentsFileName, (data) => detailQuestions = data.components));

        IsReady = true;
        Debug.Log($"EventController is ready. Loaded {allEvents.Count} events and {detailQuestions.Count} components.");
    }

    /// <summary>
    /// NEW: A generic, reusable method to load any JSON file into a specified data structure.
    /// </summary>
    /// <typeparam name="T">The type of the list wrapper class (e.g., GameEventList).</typeparam>
    /// <param name="fileName">The name of the JSON file in StreamingAssets.</param>
    /// <param name="onSuccess">An action to perform with the loaded data.</param>
    private IEnumerator LoadJsonData<T>(string fileName, System.Action<T> onSuccess)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonString = request.downloadHandler.text;
            T data = JsonUtility.FromJson<T>(jsonString);
            onSuccess?.Invoke(data);
        }
        else
        {
            Debug.LogError($"Failed to load {fileName} at: {path}. Error: {request.error}");
        }
    }

    // --- (The rest of your methods like GetUniqueEventForDay are unchanged) ---
    public GameEvent GetUniqueEventForDay()
    {
        if (allEvents == null || allEvents.Count == 0)
        {
            Debug.LogError("GetUniqueEventForDay called, but no events are loaded!");
            return null;
        }
        if (eventsUsedThisDay.Count >= allEvents.Count) { eventsUsedThisDay.Clear(); }
        GameEvent newEvent;
        do { newEvent = allEvents[Random.Range(0, allEvents.Count)]; }
        while (eventsUsedThisDay.Contains(newEvent));
        eventsUsedThisDay.Add(newEvent);
        if (newEvent.type != "unique") { return GenerateRandomQuestion(newEvent); }
        else { return newEvent; }
    }
    public void StartNewDay() { eventsUsedThisDay.Clear(); }
    public GameEvent GenerateRandomQuestion(GameEvent ge)
    {
        List<ComponentQuestion> matchingDetails = detailQuestions.FindAll(e => e.type == ge.type);
        if (matchingDetails != null && matchingDetails.Count > 0)
        {
            ComponentQuestion randomComponent = matchingDetails[Random.Range(0, matchingDetails.Count)];
            // Create a copy to avoid modifying the original event object in the list
            GameEvent clonedEvent = JsonUtility.FromJson<GameEvent>(JsonUtility.ToJson(ge));
            clonedEvent.question = clonedEvent.question.Replace("{component}", randomComponent.name);
            return clonedEvent;
        }
        else { return ge; }
    }
}
