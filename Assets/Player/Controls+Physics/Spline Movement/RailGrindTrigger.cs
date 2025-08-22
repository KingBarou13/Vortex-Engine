using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections;

public class RailGrindTrigger : MonoBehaviour
{
    public float grindSpeed = 5f;
    public float minGrindSpeed = 0.2f;
    public SplineContainer currentSpline;
    public bool isGrinding = false;
    public float splineProgress = 0f;
    public bool isReversing = false;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerPhysics playerPhysics;
    [SerializeField] private JumpAction jumpAction;
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private BounceAction bounceAction;
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;
    [SerializeField] private Transform playerModel;
    public float grindHeightOffset = 0.2f;

    private static readonly int GrindingHash = Animator.StringToHash("IsGrinding");
    private static readonly int LandOnRailHash = Animator.StringToHash("LandOnRail");

    private Vector3 previousPosition;
    private Vector3 grindVelocity;
    private Vector3 initialModelLocalPosition;

    private void Start()
    {
        if (playerModel != null)
        {
            initialModelLocalPosition = playerModel.localPosition;
        }
    }

    private void Update()
    {
        if (isGrinding)
        {
            moveAction.enabled = false;

            if (jumpAction != null && jumpAction.currentJumps > 0 && Input.GetButtonDown("Jump"))
            {
                jumpAction.Jump();
                ExitGrind();
                return;
            }

            if (isReversing)
            {
                splineProgress -= grindSpeed * Time.deltaTime;
            }
            else
            {
                splineProgress += grindSpeed * Time.deltaTime;
            }

            splineProgress = Mathf.Clamp01(splineProgress);

            Vector3 railPosition = currentSpline.EvaluatePosition(splineProgress);
            playerPhysics.RB.MovePosition(railPosition);

            Vector3 tangent = math.normalize(currentSpline.EvaluateTangent(splineProgress));
            playerPhysics.RB.MoveRotation(Quaternion.LookRotation(tangent));

            grindVelocity = (railPosition - previousPosition) / Time.deltaTime;
            previousPosition = railPosition;

            if (playerModel != null)
            {
                playerModel.localPosition = initialModelLocalPosition + new Vector3(0, grindHeightOffset, 0);
            }

            if (splineProgress <= 0f || splineProgress >= 1f)
            {
                ExitGrind();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SplineContainer spline = other.GetComponent<SplineContainer>();
        if (spline != null && other is MeshCollider)
        {
            grindSpeed = Mathf.Max(playerPhysics.horizontalVelocity.magnitude / 100, minGrindSpeed);
            grindSpeed = Mathf.Min(grindSpeed, 0.5f);
            StartGrind(spline);
        }
    }

    private void StartGrind(SplineContainer spline)
    {
        currentSpline = spline;
        Vector3 playerDirection = playerPhysics.RB.velocity.normalized;
        splineProgress = FindClosestPointOnSpline(currentSpline, transform.position);

        Vector3 splineTangent = math.normalize(currentSpline.EvaluateTangent(splineProgress));
        float dotProduct = Vector3.Dot(playerDirection, splineTangent);

        isReversing = dotProduct < 0;
        isGrinding = true;

        if (bounceAction != null)
        {
            bounceAction.CancelBounce();
        }

        jumpAction.currentJumps = jumpAction.jumps;

        animator.SetTrigger(LandOnRailHash);

        spinBall.SetActive(false);
        spinFX.SetActive(false);

        previousPosition = currentSpline.EvaluatePosition(splineProgress);
        StartCoroutine(TransitionToGrindAnimation());
    }

    private IEnumerator TransitionToGrindAnimation()
    {
        yield return new WaitForSeconds(0.35f);
        animator.SetBool(GrindingHash, true);
    }

    private float FindClosestPointOnSpline(SplineContainer spline, Vector3 position)
    {
        float closestProgress = 0f;
        float closestDistance = float.MaxValue;

        for (float t = 0; t <= 1; t += 0.01f)
        {
            Vector3 splinePosition = spline.EvaluatePosition(t);
            float distance = Vector3.Distance(position, splinePosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestProgress = t;
            }
        }

        return closestProgress;
    }

    public void ExitGrind()
    {
        isGrinding = false;
        currentSpline = null;

        playerPhysics.RB.velocity = grindVelocity + playerPhysics.verticalVelocity;
        moveAction.enabled = true;

        animator.SetBool(GrindingHash, false);
        animator.ResetTrigger(LandOnRailHash);

        if (playerModel != null)
        {
            playerModel.localPosition = initialModelLocalPosition;
        }
    }
}
