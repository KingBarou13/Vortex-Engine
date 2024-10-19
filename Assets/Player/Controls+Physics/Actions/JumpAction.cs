using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    [SerializeField] private RisingAndFalling risingAndFalling;
    [SerializeField] int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;
    [SerializeField] private RailGrindTrigger railGrindTrigger;
    public int currentJumps;

    void Start()
    {
        if (risingAndFalling == null)
        {
            risingAndFalling = GetComponent<RisingAndFalling>();
        }

        spinBall.SetActive(false);
        spinFX.SetActive(false);
    }

    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            Jump();
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

    void OnGroundEnter()
    {
        currentJumps = jumps;
        animator.SetBool("IsJumping", false);
        spinBall.SetActive(false);
        spinFX.SetActive(false);
        risingAndFalling.OnJumpEnded();
    }

    void Jump()
    {
        if (currentJumps <= 0 || (railGrindTrigger != null && railGrindTrigger.isGrinding))
        {
            return;
        }

        currentJumps--;
        float appliedJumpForce = playerPhysics.groundInfo.ground ? jumpForce : airJumpForce;
        playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * appliedJumpForce) + playerPhysics.horizontalVelocity;
        animator.SetBool("IsJumping", true);
        spinBall.SetActive(true);
        spinFX.SetActive(true);
        risingAndFalling.OnJumpInitiated();
    }
}
