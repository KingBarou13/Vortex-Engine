using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAction : PlayerAction
{
    Vector2 move;
    public CameraManager cameraManager;

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        if (!controlLockActive)
        {
            move = callbackContext.ReadValue<Vector2>();
        }
        else
        {
            move.x = 0;
        }
    }

    void OnEnable()
    {
        playerPhysics.onPlayerPhysicsUpdate += Move;
    }

    void OnDisable()
    {
        playerPhysics.onPlayerPhysicsUpdate -= Move;
    }

    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float minTurnSpeed;
    [SerializeField] float maxTurnSpeed;
    [SerializeField, Range(0, 1)] float turnDeceleration;
    [SerializeField] float uphillDeceleration;
    [SerializeField] float downhillAcceleration;

    [SerializeField] bool controlLockActive;
    [SerializeField] float controlLockTimer;

    [SerializeField] float brakeSpeed;
    [SerializeField, Range(0, 1)] float softBrakeThreshold;
    [SerializeField] float brakeThreshold;
    [SerializeField] float brakeTime;

    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem speedLines;
    [SerializeField] private float speedLineThreshold;
    [SerializeField] private float targetFOV = 90f;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 2f;

    bool braking;
    float brakeTimer;

    private void FixedUpdate()
    {
        if (controlLockActive)
        {
            controlLockTimer -= Time.deltaTime;
            if (controlLockTimer <= 0)
            {
                controlLockActive = false;
            }
        }
    }

    public bool IsBraking()
    {
        return braking;
    }

    public Vector3 GetMoveVector()
    {
        return GetMoveVector(cameraManager.GetActiveCamera().transform, groundInfo.normal, move);
    }

    public void TriggerControlLock(float duration)
    {
        controlLockActive = true;
        controlLockTimer = duration;
    }

    void Move()
    {
        Vector3 moveVector = GetMoveVector();
        float currentSpeed = playerPhysics.speed;

        animator.SetFloat("Speed", currentSpeed);
        animator.speed = Mathf.Clamp(currentSpeed / maxSpeed, 0.5f, 2f);

        if (currentSpeed >= speedLineThreshold && !speedLines.isPlaying)
        {
            speedLines.Play();
        }
        else if (currentSpeed < speedLineThreshold && speedLines.isPlaying)
        {
            speedLines.Stop();
        }

        // Adjust the FOV based on speed
        Camera activeCamera = cameraManager.GetActiveCamera();
        if (currentSpeed >= speedLineThreshold)
        {
            activeCamera.fieldOfView = Mathf.Lerp(activeCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }
        else
        {
            activeCamera.fieldOfView = Mathf.Lerp(activeCamera.fieldOfView, baseFOV, fovChangeSpeed * Time.deltaTime);
        }

        bool wasBraking = braking;

        braking = groundInfo.ground && playerPhysics.speed > RB.sleepThreshold &&
                  ((braking && brakeTimer > 0) || Vector3.Dot(moveVector.normalized, playerPhysics.horizontalVelocity) < -brakeThreshold);

        if (braking)
        {
            brakeTimer -= Time.deltaTime;
        }

        if (braking && !wasBraking)
        {
            brakeTimer = brakeTime;
        }

        if (braking)
        {
            animator.SetBool("IsBraking", true);
            Decelerate(brakeSpeed);
        }
        else
        {
            animator.SetBool("IsBraking", false);
            if (move.magnitude > 0)
            {
                if (Vector3.Dot(moveVector.normalized, playerPhysics.horizontalVelocity.normalized) >= (groundInfo.ground ? -softBrakeThreshold : 0))
                {
                    Accelerate(acceleration);
                }
                else
                {
                    Decelerate(brakeSpeed);
                }
            }
            else
            {
                Decelerate(deceleration);
            }
        }

        void Accelerate(float speed)
        {
            float maxRadDelta = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, playerPhysics.speed / maxSpeed) * Mathf.PI * Time.deltaTime;
            float maxDistDelta = speed * Time.deltaTime;

            Vector3 velocity = Vector3.RotateTowards(playerPhysics.horizontalVelocity, moveVector * maxSpeed, maxRadDelta, maxDistDelta);
            
            velocity -= velocity * (Vector3.Angle(playerPhysics.horizontalVelocity, velocity) / 180 * turnDeceleration);
            
            float dot = Vector3.Dot(velocity.normalized, Vector3.up); 
            float slopeFactor = dot > 0 ? -uphillDeceleration : -downhillAcceleration; 
            velocity += velocity.normalized * dot * slopeFactor; 

            RB.velocity = velocity + playerPhysics.verticalVelocity;
        }

        void Decelerate(float speed)
        {
            RB.velocity = Vector3.MoveTowards(playerPhysics.horizontalVelocity, Vector3.zero, speed * Time.deltaTime)
                          + playerPhysics.verticalVelocity;
        }
    }

    Vector3 GetMoveVector(Transform relativeTo, Vector3 groundNormal, Vector2 moveInput)
    {
        Vector3 cameraRight = relativeTo.right;
        Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up);
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 cameraRelativeMove = inputDirection.x * cameraRight + inputDirection.z * cameraForward;
        Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Vector3 finalMoveVector = groundRotation * cameraRelativeMove;

        Debug.DrawRay(RB.transform.position, finalMoveVector * 10, Color.blue);

        return finalMoveVector;
    }
}
