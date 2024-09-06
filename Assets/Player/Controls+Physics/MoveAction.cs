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
    [SerializeField] float brakeSpeed;
    [SerializeField, Range(0, 1)] float softBrakeThreshold;
    [SerializeField] float brakeThreshold;
    [SerializeField] float brakeTime;

    [SerializeField] private Animator animator;

    bool braking;
    float brakeTimer;

    // Public method to check if braking
    public bool IsBraking()
    {
        return braking;
    }

    // Reference for external scripts to get the move vector
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
       
        bool wasBraking = braking;

        // Check if the player is braking
        braking = groundInfo.ground && playerPhysics.speed > RB.sleepThreshold &&
                  ((braking && brakeTimer > 0) || Vector3.Dot(moveVector.normalized, playerPhysics.horizontalVelocity) < -brakeThreshold);

        if (braking)
        {
            brakeTimer -= Time.deltaTime;
        }

        // Reset brake timer if we start braking
        if (braking && !wasBraking)
        {
            brakeTimer = brakeTime;
        }

        // Apply braking or acceleration logic
        if (braking)
        {
            Decelerate(brakeSpeed);
        }
        else if (move.magnitude > 0)
        {
            // Check the direction of movement and only accelerate if we're moving in the same direction or stopping soft-braking.
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
