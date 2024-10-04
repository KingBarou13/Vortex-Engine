using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAction : PlayerAction
{
    // On Move
    Vector2 move;

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        // If control lock is active, ignore left/right input
        if (!controlLockActive)
        {
            move = callbackContext.ReadValue<Vector2>();
        }
        else
        {
            // Prevent left and right input during the control lock period
            move.x = 0; // Only vertical movement allowed
        }
    }

    // On Enable
    void OnEnable()
    {
        playerPhysics.onPlayerPhysicsUpdate += Move;
    }

    // On Disable
    void OnDisable()
    {
        playerPhysics.onPlayerPhysicsUpdate -= Move;
    }

    // Movement Settings
    [SerializeField] Transform cameraTransform;
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

    [SerializeField] private ParticleSystem speedLines; // Reference to the speed lines particle system
    [SerializeField] private float speedLineThreshold;  // Speed at which speed lines appear

    [SerializeField] private Camera mainCamera;         // Reference to the main camera
    [SerializeField] private float targetFOV = 90f;     // Target FOV when speed is high
    [SerializeField] private float baseFOV = 60f;       // Base FOV when speed is low
    [SerializeField] private float fovChangeSpeed = 2f; // Speed of FOV transition

    bool braking;
    float brakeTimer;

    private void FixedUpdate()
    {
        if(controlLockActive)
        {
            controlLockTimer -= Time.deltaTime;
            if(controlLockTimer <= 0)
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
        return GetMoveVector(cameraTransform, groundInfo.normal, move);
    }

    //Control Lock Trigger
    public void TriggerControlLock(float duration)
    {
        controlLockActive = true;
        controlLockTimer = duration;
    }

    // Move function
    void Move()
    {
        Vector3 moveVector = GetMoveVector(cameraTransform, groundInfo.normal, move);

        float currentSpeed = playerPhysics.speed;

        animator.SetFloat("Speed", currentSpeed);
        animator.speed = Mathf.Clamp(currentSpeed / maxSpeed, 0.5f, 2f);

        // Control the visibility of the speed lines based on the custom speed threshold
        if (currentSpeed >= speedLineThreshold && !speedLines.isPlaying)
        {
            speedLines.Play();  // Start speed lines when the speed exceeds the threshold
        }
        else if (currentSpeed < speedLineThreshold && speedLines.isPlaying)
        {
            speedLines.Stop();  // Stop speed lines when speed falls below the threshold
        }

        // Adjust the camera's FOV when speed exceeds the threshold
        if (currentSpeed >= speedLineThreshold)
        {
            // Smoothly transition to the target FOV
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly return to the base FOV
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, baseFOV, fovChangeSpeed * Time.deltaTime);
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

        // Acceleration function
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


        // Deceleration function
        void Decelerate(float speed)
        {
            RB.velocity = Vector3.MoveTowards(playerPhysics.horizontalVelocity, Vector3.zero, speed * Time.deltaTime)
                          + playerPhysics.verticalVelocity;
        }
    }

    // Get Move Vector function
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
