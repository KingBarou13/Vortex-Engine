using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class RailGrindAction : PlayerAction
{
    public bool onRail;
    [SerializeField] float grindSpeed;
    [SerializeField] float heightOffset;
    float timeForFullSpline;
    float elapsedTime;
    [SerializeField] float lerpSpeed = 10f;

    [Header("Scripts")]
    [SerializeField] RailScript currentRailScript;
    private Rigidbody playerRigidbody;
    private MoveAction moveAction;
    private JumpAction jumpAction;

    private void Start()
    {
        playerRigidbody = playerPhysics.RB;
        moveAction = GetComponent<MoveAction>();
        jumpAction = GetComponent<JumpAction>();
    }

    private void OnEnable()
    {
        playerPhysics.onPlayerPhysicsUpdate += PhysicsUpdate;
    }

    private void OnDisable()
    {
        playerPhysics.onPlayerPhysicsUpdate -= PhysicsUpdate;
    }

    void PhysicsUpdate()
    {
        if (onRail)
        {
            MovePlayerAlongRail();
        }
    }

    private void Update()
    {
        if (!onRail)
        {
            // Handle other player movement when not grinding.
        }
    }

    void MovePlayerAlongRail()
    {
        if (currentRailScript != null && onRail)
        {
            float progress = elapsedTime / timeForFullSpline;

            if (progress < 0 || progress > 1)
            {
                ThrowOffRail();
                return;
            }

            float nextTimeNormalised = (elapsedTime + Time.deltaTime) / timeForFullSpline;
            float3 pos, tangent, up;
            float3 nextPosfloat, nextTan, nextUp;
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, progress, out pos, out tangent, out up);
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, nextTimeNormalised, out nextPosfloat, out nextTan, out nextUp);

            Vector3 worldPos = currentRailScript.LocalToWorldConversion(pos);
            Vector3 nextPos = currentRailScript.LocalToWorldConversion(nextPosfloat);

            // Set position and adjust height offset
            playerRigidbody.MovePosition(worldPos + (transform.up * heightOffset));

            // Smoothly rotate the player along the rail direction
            Quaternion targetRotation = Quaternion.LookRotation(nextPos - worldPos);
            playerRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, lerpSpeed * Time.deltaTime));

            // Increment time for next update
            elapsedTime += Time.deltaTime;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Rail"))
        {
            onRail = true;
            currentRailScript = hit.gameObject.GetComponent<RailScript>();
            CalculateAndSetRailPosition();

            // Disable regular movement and braking when grinding
            moveAction.enabled = false;
            playerRigidbody.useGravity = false;
        }
    }

    void CalculateAndSetRailPosition()
    {
        timeForFullSpline = currentRailScript.totalSplineLength / grindSpeed;

        Vector3 splinePoint;
        float normalisedTime = currentRailScript.CalculateTargetRailPoint(transform.position, out splinePoint);
        elapsedTime = timeForFullSpline * normalisedTime;

        float3 pos, forward, up;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, normalisedTime, out pos, out forward, out up);
        currentRailScript.CalculateDirection(forward, transform.forward);

        playerRigidbody.MovePosition(splinePoint + (transform.up * heightOffset));
    }

    void ThrowOffRail()
    {
        onRail = false;
        currentRailScript = null;
        playerRigidbody.useGravity = true;

        // Re-enable movement controls
        moveAction.enabled = true;
        transform.position += transform.forward * 1; // Optional: Add small forward push when leaving rail.
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (onRail && context.performed)
        {
            // Allow the player to jump off the rail while grinding
            ThrowOffRail();
            jumpAction.OnJump(context); // Call jump functionality
        }
    }
}
