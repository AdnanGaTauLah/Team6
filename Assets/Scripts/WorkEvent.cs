using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkEvent : MonoBehaviour
{
    public UIManager uIManager;
    public bool isDoorOpen = false;
    public GameManager gameManager;

    [SerializeField] private Animator anim;

    private void OnMouseUp()
    {
        if (!GameManager.isEndDay) return;
        if (GameManager.isEventRunning) return;
        GameManager.isEventRunning = true;
        isDoorOpen = true;
        anim.Play("door_open");
        uIManager.DisplayEndDayEvent("bekerja");
    }
    private void OnMouseEnter()
    {
        if (!GameManager.isEndDay) return;
        if (!isDoorOpen)
        {
            anim.Play("hovering_door");
        }
    }
   
    private void OnMouseExit()
    {
        if (!isDoorOpen)
        {
            CloseDoor();
        } 
    }
    public void CloseDoor()
    {
        anim.Play("close_door");
    }

    public void Work()
    {
        gameManager.ChangePlayerState(-5, -5, 20);
    }
}
