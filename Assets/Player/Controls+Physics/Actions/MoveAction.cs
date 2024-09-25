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
    [SerializeField] float maxSpeed; 
    [SerializeField] float minTurnSpeed;
    [SerializeField] float maxTurnSpeed;
    [SerializeField, Range(0, 1)] float turnDeceleration;
    [SerializeField] float uphillDeceleration;
    [SerializeField] float downhillAcceleration;

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
