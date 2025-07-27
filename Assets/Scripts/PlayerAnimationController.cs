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
    private readonly int isIdlingHash = Animator.StringToHash("IsIdling");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        StartCoroutine(IdleRoutine());
    }

    /// <summary>
    /// A coroutine that cycles between the 'Idle' and 'Wait' animation states.
    /// </summary>
    private IEnumerator IdleRoutine()
    {
        while (true)
        {
            // --- Wait Phase ---
            animator.SetBool(isIdlingHash, false);
            float randomDelay = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomDelay);

            // --- Idle Phase ---
            animator.SetBool(isIdlingHash, true);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
