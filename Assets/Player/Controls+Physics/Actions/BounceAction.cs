using UnityEngine;
using UnityEngine.InputSystem;

public class BounceAction : PlayerAction
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;
    [SerializeField] private JumpAction jumpAction;

    [Header("Bounce Settings")]
    [SerializeField] private float downwardForce = 30f;
    [SerializeField] private float reboundForce = 25f;
    [SerializeField] private float reboundForceIncrease = 5f;

    private bool isBouncing = false;
    private float currentReboundForce;

    public bool JustRebounded { get; private set; }

    void LateUpdate()
    {
        if (JustRebounded)
        {
            JustRebounded = false;
        }
    }


    void Start()
    {
        spinBall.SetActive(false);
        spinFX.SetActive(false);
        currentReboundForce = reboundForce;
    }

    public void OnBounce(InputAction.CallbackContext context)
    {
        if (context.performed && !groundInfo.ground && !isBouncing)
        {
            StartBounce();
        }
    }

    void OnEnable()
    {
        playerPhysics.onGroundEnter += OnGroundEnter;
    }

    void OnDisable()
    {
        playerPhysics.onGroundEnter -= OnGroundEnter;
    }

    private void StartBounce()
    {
        isBouncing = true;
        animator.SetBool("IsJumping", true);
        spinBall.SetActive(true);
        spinFX.SetActive(true);

        // Apply immediate downward force
        RB.velocity = new Vector3(playerPhysics.horizontalVelocity.x, -downwardForce, playerPhysics.horizontalVelocity.z);
    }

    private void OnGroundEnter()
    {
        if (isBouncing)
        {
            Rebound();
        }
        else
        {
            currentReboundForce = reboundForce;
        }
    }

    private void Rebound()
    {
        RB.velocity = new Vector3(playerPhysics.horizontalVelocity.x, currentReboundForce, playerPhysics.horizontalVelocity.z);

        // Increase force for next rebound
        currentReboundForce += reboundForceIncrease;

        // Allow another jump/bounce
        jumpAction.currentJumps = 1;

        isBouncing = false;
        JustRebounded = true;
    }

    public void CancelBounce()
    {
        isBouncing = false;
        currentReboundForce = reboundForce;
        spinBall.SetActive(false);
        spinFX.SetActive(false);
    }
}