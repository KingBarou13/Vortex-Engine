using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAction : PlayerAction
{
    // On Move
    Vector2 move;

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        move = callbackContext.ReadValue<Vector2>();
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
    [SerializeField] float maxSpeed; //Max speed without the help of slopes
    [SerializeField] private float slopeMaxSpeed; // Maximum speed using slopes
    [SerializeField] float minTurnSpeed;
    [SerializeField] float maxTurnSpeed;
    [SerializeField, Range(0, 1)] float turnDeceleration; 
    [SerializeField] float brakeSpeed;
    [SerializeField, Range(0, 1)] float softBrakeThreshold;
    [SerializeField] float brakeThreshold;
    [SerializeField] float brakeTime;

    [Header("Slope Variables")]
    [SerializeField] private float slopeRotation; // Angle of the slope
    [SerializeField] private float slopeAssistance; // How much speed Sonic gains downhill
    [SerializeField] private float slopeDrag; // How much speed Sonic loses uphill
    [SerializeField] private bool isGoingUphill; //Checks whether Sonic is moving uphill or downhill

    [SerializeField] private Animator animator;

    [SerializeField] private ParticleSystem speedLines; // Reference to the speed lines particle system
    [SerializeField] private float speedLineThreshold;  // Speed at which speed lines appear

    [SerializeField] private Camera mainCamera;         // Reference to the main camera
    [SerializeField] private float targetFOV = 90f;     // Target FOV when speed is high
    [SerializeField] private float baseFOV = 60f;       // Base FOV when speed is low
    [SerializeField] private float fovChangeSpeed = 2f; // Speed of FOV transition

    bool braking;
    float brakeTimer;

    public bool IsBraking()
    {
        return braking;
    }

    public Vector3 GetMoveVector()
    {
        return GetMoveVector(cameraTransform, groundInfo.normal, move);
    }

    // Move function
    void Move()
    {
        Vector3 moveVector = GetMoveVector(cameraTransform, groundInfo.normal, move);

        float currentSpeed = playerPhysics.speed;

        slopeRotation = Vector3.Angle(Vector3.up, groundInfo.normal); // Get the slope's angle relative to the upward direction
        isGoingUphill = Vector3.Dot(moveVector.normalized, groundInfo.normal) > 0; // True if going uphill, false if going downhill

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
    Vector3 GetMoveVector(Transform relativeTo, Vector3 upNormal, Vector2 move)
    {
        Vector3 rightNormal = Vector3.Cross(upNormal, relativeTo.forward);
        Vector3 forwardNormal = Vector3.Cross(relativeTo.right, upNormal);

        Vector3.OrthoNormalize(ref upNormal, ref forwardNormal, ref rightNormal);

        Debug.DrawRay(RB.transform.position, rightNormal * 10, Color.red);
        Debug.DrawRay(RB.transform.position, forwardNormal * 10, Color.green);

        return (rightNormal * move.x) + (forwardNormal * move.y);
    }
}
