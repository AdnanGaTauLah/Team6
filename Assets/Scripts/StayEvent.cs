using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayEvent : MonoBehaviour
{
    public UIManager uIManager;
    public GameManager gameManager;

    private void OnMouseUp()
    {
        if (!GameManager.isEndDay) return;
        if (GameManager.isEventRunning) return;
        GameManager.isEventRunning = true;
        uIManager.DisplayEndDayEvent("di rumah");
    }

    public void Stay()
    {
        gameManager.ChangePlayerState(5, 5, 0);
    }
}
