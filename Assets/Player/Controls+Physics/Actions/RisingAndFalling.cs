using UnityEngine;

public class RisingAndFalling : MonoBehaviour
{
    [SerializeField] private PlayerPhysics playerPhysics;
    [SerializeField] private Animator animator;
    [SerializeField] private float risingThreshold = 2f; 
    [SerializeField] private float fallingThreshold = -2f; 
    [SerializeField] private float deadZone = 0.1f; 
    [SerializeField] private float minAirTime = 0.1f; 

    private bool wasGrounded = true;
    private bool wasJumping = false;
    private float airTime = 0f;

    private void Update()
    {
        bool isGrounded = playerPhysics.groundInfo.ground;
        float verticalSpeed = playerPhysics.verticalSpeed;

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            airTime = 0f; 
        }

        if (wasGrounded && !isGrounded && !wasJumping)
        {
            if (verticalSpeed > risingThreshold && airTime > minAirTime)
            {
                animator.SetTrigger("StartRising");
            }
            else if (verticalSpeed < fallingThreshold && airTime > minAirTime)
            {
                animator.SetTrigger("StartFalling");
            }
        }

        else if (!isGrounded)
        {
            if (verticalSpeed > risingThreshold + deadZone && airTime > minAirTime)
            {
                animator.SetBool("IsRising", true);
                animator.SetBool("IsFalling", false);
                ResetTriggers();
            }
            else if (verticalSpeed < fallingThreshold - deadZone && airTime > minAirTime)
            {
                animator.SetBool("IsRising", false);
                animator.SetBool("IsFalling", true);
                ResetTriggers();
            }
            else if (airTime <= minAirTime)
            {
                animator.SetBool("IsRising", false);
                animator.SetBool("IsFalling", false);
                ResetTriggers();
            }
        }

        else if (isGrounded)
        {
            ResetAnimationStates();
        }

        wasGrounded = isGrounded;
    }

    private void ResetAnimationStates()
    {
        animator.SetBool("IsRising", false);
        animator.SetBool("IsFalling", false);
        ResetTriggers(); 
        wasJumping = false; 
    }

    public void OnJumpInitiated()
    {
        wasJumping = true;
        animator.SetBool("IsRising", true); 
        animator.SetBool("IsFalling", false);
        ResetTriggers(); 
    }

    public void OnJumpEnded()
    {
        wasJumping = false;
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger("StartRising");
        animator.ResetTrigger("StartFalling");
    }
}
