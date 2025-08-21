using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DriftAction : PlayerAction
{
    [Header("References")]
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private Animator animator;
    [SerializeField] private CameraManager cameraManager;

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

    [Header("Visual Effects")]
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;

    [Header("Camera FOV")]
    [SerializeField] private float driftFOV = 40f;
    [SerializeField] private float boostFOV = 90f;
    [SerializeField] private float fovChangeSpeed = 5f;

    private bool isDriftButtonHeld = false;
    private float driftCharge = 0f;
    private Coroutine boostAnimationCoroutine;

    public bool IsDrifting { get; private set; } = false;

    void Start()
    {
        spinBall.SetActive(false);
        spinFX.SetActive(false);     
    } 

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (context.started && groundInfo.ground)
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
        bool canDrift = isDriftButtonHeld && groundInfo.ground;

        if (canDrift && !IsDrifting)
        {
            // Start Drifting
            IsDrifting = true;
            moveAction.TriggerControlLock(float.MaxValue); // Lock regular movement
            animator.SetBool("IsJumping", true);
        }
        else if (!canDrift && IsDrifting)
        {
            ReleaseDrift();
        }

        if (IsDrifting)
        {
            PerformDrift();
            Camera activeCamera = cameraManager.GetActiveCamera();
            activeCamera.fieldOfView = Mathf.Lerp(activeCamera.fieldOfView, driftFOV, fovChangeSpeed * Time.deltaTime);
            spinBall.SetActive(true);
            spinFX.SetActive(true);
        }
    }

    private void PerformDrift()
    {
        // Get move direction from MoveAction
        Vector3 moveVector = moveAction.GetMoveVector();
        Vector3 velocity = playerPhysics.horizontalVelocity;
        
        Vector3 targetVelocity = Vector3.RotateTowards(velocity, moveVector * playerPhysics.speed, driftTurnPower * Time.deltaTime, 0.0f);

        targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, driftDeceleration * Time.deltaTime);

        RB.velocity = targetVelocity + playerPhysics.verticalVelocity;

        if (driftCharge < maxCharge)
        {
            driftCharge += Time.deltaTime * chargeRate;
        }
    }

    private void ReleaseDrift()
    {
        IsDrifting = false;
        moveAction.TriggerControlLock(0); // Release movement lock
        
        if (boostAnimationCoroutine != null)
        {
            StopCoroutine(boostAnimationCoroutine);
        }

        if (driftCharge > 0.1f) // Only boost if there's some charge
        {
            // 3. Apply boost on release in the direction the player is facing
            Vector3 boostDirection = player.transform.forward;
            float boostMagnitude = driftCharge * boostForceMultiplier;

            // This sets the horizontal velocity directly, overriding sideways momentum.
            RB.velocity = boostDirection * boostMagnitude + playerPhysics.verticalVelocity;

            moveAction.TriggerControlLock(boostDuration);
            boostAnimationCoroutine = StartCoroutine(EndBoostAnimation(boostDuration));
        }
        else
        {
            animator.SetBool("IsJumping", false);
        }

        // Reset charge
        driftCharge = 0f;
    }

    private IEnumerator EndBoostAnimation(float duration)
    {
        Camera activeCamera = cameraManager.GetActiveCamera();
        float startTime = Time.time;

        // Boost phase: increase FOV
        while (Time.time < startTime + duration)
        {
            activeCamera.fieldOfView = Mathf.Lerp(activeCamera.fieldOfView, boostFOV, fovChangeSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("IsJumping", false);
        spinBall.SetActive(false);
        spinFX.SetActive(false);
        boostAnimationCoroutine = null;
    }
}