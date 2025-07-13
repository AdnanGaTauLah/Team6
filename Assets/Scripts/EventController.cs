using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class EventController : MonoBehaviour
{
    // --- ATTRIBUTES ---
    [Header("Event List")]
    [Tooltip("The list of all possible events that can occur in the game.")]
    public List<Event> allEvents;

    private Event currentEvent;

    // --- METHODS ---

    /// <summary>
    /// Selects a random event from the 'allEvents' list.
    /// It ensures that the same event isn't picked twice in a row if there are multiple options.
    /// </summary>
    /// <returns>A random Event object.</returns>
    public Event GetRandomEvent()
    {
        if (allEvents.Count == 0)
        {
            Debug.LogError("Event list is empty! Cannot get a random event.");
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


    // --- UNITY LIFECYCLE (for testing) ---

    // Example of how you can test this script.
    // Press the 'G' key to get a random event and print its question to the console.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Event randomEvent = GetRandomEvent();
            if (randomEvent != null)
            {
                Debug.Log("New Random Event: " + randomEvent.question);
            }
        }
    }
}
