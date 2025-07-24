using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's animation states by controlling Animator parameters.
/// This script creates a natural idle loop with randomized delays.
/// It is designed to be placed on a character prefab that has an Animator component.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Timing")]
    [Tooltip("The minimum time the character will wait before starting the idle animation again.")]
    [SerializeField] private float minWaitTime = 2.0f;

    [Tooltip("The maximum time the character will wait before starting the idle animation again.")]
    [SerializeField] private float maxWaitTime = 5.0f;

    private Animator animator;

    // A "hash" is a more efficient way to reference an Animator parameter than using a string.
    // This converts the "IsIdling" string into an integer ID for faster access.
    private readonly int isIdlingHash = Animator.StringToHash("IsIdling");

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Called on the frame when a script is enabled. Starts the animation logic.
    /// </summary>
    void Start()
    {
        // Start the coroutine that manages the animation cycle.
        StartCoroutine(IdleRoutine());
    }

    /// <summary>
    /// A coroutine that cycles between the 'Idle' and 'Wait' animation states.
    /// </summary>
    private IEnumerator IdleRoutine()
    {
        // This loop will run for the entire lifetime of the object.
        while (true)
        {
            // --- Wait Phase ---
            // 1. Tell the Animator to go to the 'Wait' state by setting IsIdling to false.
            animator.SetBool(isIdlingHash, false);

            // 2. Calculate a random delay.
            float randomDelay = Random.Range(minWaitTime, maxWaitTime);

            // 3. Wait for that random amount of time. The coroutine pauses here.
            yield return new WaitForSeconds(randomDelay);

            // --- Idle Phase ---
            // 4. Tell the Animator to transition to the active idle state by setting IsIdling to true.
            animator.SetBool(isIdlingHash, true);

            // 5. Wait until the idle animation has finished playing once.
            // We wait for the end of the frame to ensure the state has fully transitioned,
            // then we wait for the length of the currently playing animation clip.
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
