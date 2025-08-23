using System.Collections;
using UnityEngine;

public class DashPanel : MonoBehaviour
{
    [SerializeField] private float boostSpeed = 40f;
    [SerializeField] private float boostDuration = 1f;

    [SerializeField] private PlayerPhysics playerPhysics;
    [SerializeField] private BounceAction bounceAction;
    [SerializeField] private RotationHandler rotationHandler;
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip dashSound;

    private Coroutine dashCoroutine;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 boostDirection = transform.forward;
        // Arrow length is proportional to boostSpeed. Adjust the divisor for scaling.
        float arrowLength = boostSpeed / 5f; 
        Gizmos.DrawLine(transform.position, transform.position + boostDirection * arrowLength);
        Gizmos.DrawSphere(transform.position + boostDirection * arrowLength, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ensure the references are set before using them
            if (playerPhysics != null)
            {
                Vector3 verticalVelocity = Vector3.Project(playerPhysics.RB.velocity, Vector3.up);
                playerPhysics.RB.velocity = transform.forward * boostSpeed + verticalVelocity;
            }

            if (rotationHandler != null)
            {
                rotationHandler.SetPlayerRotation(-transform.forward);
            }

            if (bounceAction != null)
            {
                bounceAction.CancelBounce();
            }

            if (moveAction != null)
            {
                moveAction.TriggerControlLock(boostDuration);
            }

            if (animator != null)
            {
                if (dashCoroutine != null)
                {
                    StopCoroutine(dashCoroutine);
                }
                dashCoroutine = StartCoroutine(DashAnimation(boostDuration));
            }

            if (dashSound != null)
            {
                AudioSource playerAudio = other.GetComponent<AudioSource>();
                if (playerAudio != null)
                {
                    playerAudio.PlayOneShot(dashSound);
                }
            }
        }
    }

    private IEnumerator DashAnimation(float duration)
    {
        animator.SetBool("IsJumping", true);
        yield return new WaitForSeconds(duration);
        animator.SetBool("IsJumping", false);
        dashCoroutine = null;
    }
}