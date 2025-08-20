using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DriftAction : PlayerAction
{
    [Header("References")]
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private Animator animator;

    [Header("Drift Settings")]
    [SerializeField] private float driftTurnPower = 5f;
    [SerializeField] private float driftDeceleration = 2f;
    [SerializeField] private float minDriftSpeed = 10f;

    [Header("Charge & Boost")]
    [SerializeField] private float chargeRate = 1f;
    [SerializeField] private float maxCharge = 3f;
    [SerializeField] private float boostForceMultiplier = 10f;
    [SerializeField] private float boostDuration = 1f;
    [SerializeField] private Transform player;

    private bool isDriftButtonHeld = false;
    private float driftCharge = 0f;

    public bool IsDrifting { get; private set; } = false;

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (context.started && groundInfo.ground && playerPhysics.speed > minDriftSpeed)
        {
            isDriftButtonHeld = true;
        }

        if (context.canceled && isDriftButtonHeld)
        {
            isDriftButtonHeld = false;
            if (IsDrifting)
            {
                ReleaseDrift();
            }
        }
    }

    void OnEnable()
    {
        playerPhysics.onPlayerPhysicsUpdate += HandleDrift;
    }

    void OnDisable()
    {
        playerPhysics.onPlayerPhysicsUpdate -= HandleDrift;
    }

    private void HandleDrift()
    {
        // Check if we should be drifting
        bool canDrift = isDriftButtonHeld && groundInfo.ground && playerPhysics.speed > minDriftSpeed;

        if (canDrift && !IsDrifting)
        {
            // Start Drifting
            IsDrifting = true;
            moveAction.TriggerControlLock(float.MaxValue); // Lock regular movement
            animator.SetBool("IsJumping", true);
        }
        else if (!canDrift && IsDrifting)
        {
            // Stop drifting if conditions are no longer met (e.g., fell off a cliff)
            ReleaseDrift();
        }

        if (IsDrifting)
        {
            PerformDrift();
        }
    }

    private void PerformDrift()
    {
        // Get move direction from MoveAction
        Vector3 moveVector = moveAction.GetMoveVector();
        Vector3 velocity = playerPhysics.horizontalVelocity;
        
        // 1. Apply turning force
        // This rotates the current velocity towards the input direction
        Vector3 targetVelocity = Vector3.RotateTowards(velocity, moveVector * playerPhysics.speed, driftTurnPower * Time.deltaTime, 0.0f);

        // 2. Apply deceleration to the result of the turn
        // This slows the player down over time while drifting
        targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, driftDeceleration * Time.deltaTime);

        // 3. Set the final velocity
        RB.velocity = targetVelocity + playerPhysics.verticalVelocity;

        // 4. Build up charge
        if (driftCharge < maxCharge)
        {
            driftCharge += Time.deltaTime * chargeRate;
        }
    }

    private void ReleaseDrift()
    {
        IsDrifting = false;
        moveAction.TriggerControlLock(0); // Release movement lock

        if (driftCharge > 0.1f) // Only boost if there's some charge
        {
            // 3. Apply boost on release in the direction the player is facing
            Vector3 boostDirection = player.transform.forward;
            float boostMagnitude = driftCharge * boostForceMultiplier;

            // This sets the horizontal velocity directly, overriding sideways momentum.
            RB.velocity = boostDirection * boostMagnitude + playerPhysics.verticalVelocity;
            
            moveAction.TriggerControlLock(boostDuration);
            StartCoroutine(EndBoostAnimation(boostDuration));
        }
        else
        {
            // If there's no boost, turn off the animation immediately.
            animator.SetBool("IsJumping", false);
        }

        // Reset charge
        driftCharge = 0f;
    }

    private IEnumerator EndBoostAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool("IsJumping", false);
    }
}