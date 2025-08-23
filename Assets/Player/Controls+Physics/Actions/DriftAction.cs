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
    private bool isAirCharging = false;

    public bool IsDrifting { get; private set; } = false;

    void Start()
    {
        spinBall.SetActive(false);
        spinFX.SetActive(false);     
    } 

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (context.started)
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
        playerPhysics.onGroundEnter += OnGroundEnter;
    }

    void OnDisable()
    {
        playerPhysics.onPlayerPhysicsUpdate -= HandleDrift;
        playerPhysics.onGroundEnter -= OnGroundEnter;
    }

    private void OnGroundEnter()
    {
        if (isAirCharging && driftCharge > 0.1f)
        {
            ApplyBoost();
        }
        isAirCharging = false;
    }

    private void HandleDrift()
    {
        bool onGround = groundInfo.ground;
        // Prevent drifting if a boost is currently active.
        bool isBoosting = boostAnimationCoroutine != null;
        bool canDrift = isDriftButtonHeld && onGround && !isBoosting;

        if (canDrift && !IsDrifting)
        {
            IsDrifting = true;
            isAirCharging = false; 
            moveAction.TriggerControlLock(float.MaxValue); 
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

        // --- Air Charge Logic ---
        if (isDriftButtonHeld && !onGround)
        {
            if (!isAirCharging) // Check if we just started air charging
            {
                isAirCharging = true;
                animator.SetBool("IsJumping", true);
            }
        }

        if (isAirCharging)
        {
            if (driftCharge < maxCharge)
            {
                driftCharge += Time.deltaTime * chargeRate;
            }
        }

        // If button is released in the air, cancel the charge
        if (!isDriftButtonHeld && isAirCharging)
        {
            isAirCharging = false; 
            driftCharge = 0f;
            animator.SetBool("IsJumping", false); // Revert animation
        }
    }

    private void PerformDrift()
    {
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
        moveAction.TriggerControlLock(0); 
        
        if (boostAnimationCoroutine != null)
        {
            StopCoroutine(boostAnimationCoroutine);
        }

        if (driftCharge > 0.1f) 
        {
            ApplyBoost();
        }
        else
        {
            animator.SetBool("IsJumping", false);
            spinBall.SetActive(false);
            spinFX.SetActive(false);
        }

        driftCharge = 0f;
    }

    private void ApplyBoost()
    {
        Vector3 boostDirection = player.transform.forward;
        float boostMagnitude = driftCharge * boostForceMultiplier;

        RB.velocity = boostDirection * boostMagnitude + playerPhysics.verticalVelocity;

        moveAction.TriggerControlLock(boostDuration);
        animator.SetBool("IsJumping", true);
        boostAnimationCoroutine = StartCoroutine(EndBoostAnimation(boostDuration));
    }

    private IEnumerator EndBoostAnimation(float duration)
    {
        Camera activeCamera = cameraManager.GetActiveCamera();
        float startTime = Time.time;

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
