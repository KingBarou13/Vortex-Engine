using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float launchForce;
    [Tooltip("The direction to launch the player. Only used if the object has the 'Ramp' tag.")]
    [SerializeField] private Vector3 launchDirection = Vector3.up;

    [SerializeField] private AudioClip rampLaunchSound;
    [SerializeField] private AudioClip springLaunchSound;

    [SerializeField] private float dashRingAnimationDuration = 0.333f;
    [SerializeField] private Animator animator;
    [SerializeField] private RisingAndFalling risingAndFalling;
    [SerializeField] private RotationHandler rotationHandler;

    private void OnDrawGizmos()
    {
        if (CompareTag("Ramp"))
        {
            Gizmos.color = Color.red;
            // Ensure the gizmo direction respects the object's rotation
            Vector3 worldLaunchDirection = transform.TransformDirection(launchDirection.normalized);
            // Arrow length is proportional to launchForce. Adjust the divisor for scaling.
            float arrowLength = launchForce / 5f; 
            Gizmos.DrawLine(transform.position, transform.position + worldLaunchDirection * arrowLength);
            Gizmos.DrawSphere(transform.position + worldLaunchDirection * arrowLength, 0.25f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerPhysics = other.GetComponent<PlayerPhysics>();

            if (playerPhysics != null)
            {
                LaunchPlayer(playerPhysics);
            }
        }
    }

    private void LaunchPlayer(PlayerPhysics playerPhysics)
    {
        StartCoroutine(LaunchRoutine(playerPhysics));
    }

    private IEnumerator LaunchRoutine(PlayerPhysics playerPhysics)
    {
        playerPhysics.DisableGroundCheck = true;

        Vector3 finalLaunchDirection;

        if (CompareTag("Ramp"))
        {
            // Use the specified direction, interpreted relative to the ramp's orientation
            finalLaunchDirection = transform.TransformDirection(launchDirection.normalized);
        }
        else if (CompareTag("Dash Ring"))
        {
            finalLaunchDirection = transform.forward;
            if (animator != null)
            {
                // Reset other animation states to prevent immediate transitions
                animator.SetBool("IsRising", false);
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsJumping", false);
                animator.SetTrigger("DashRing");
            }
            if (risingAndFalling != null)
            {
                risingAndFalling.IsAnimationOverridden = true;
            }
            if (rotationHandler != null)
            {
                rotationHandler.SetPlayerRotation(transform.forward);
            }
        }
        else
        {
            finalLaunchDirection = transform.up;
        }

        playerPhysics.RB.velocity = finalLaunchDirection * launchForce;
        if (CompareTag("Ramp") && rampLaunchSound != null)
        {
            AudioSource playerAudio = playerPhysics.GetComponent<AudioSource>();
            if (playerAudio != null)
            {
                playerAudio.PlayOneShot(rampLaunchSound);
            }
        }
        else if (CompareTag("Spring") && springLaunchSound != null)
        {
            AudioSource playerAudio = playerPhysics.GetComponent<AudioSource>();
            if (playerAudio != null)
            {
                playerAudio.PlayOneShot(springLaunchSound);
            }
        }

        Debug.Log("Bounce!");

        var jumpAction = playerPhysics.GetComponent<JumpAction>();
        if (jumpAction != null)
        {
            jumpAction.currentJumps = 0; // Reset jumps after launch
        }

        if (CompareTag("Dash Ring"))
        {
            // Wait for the dash ring animation to finish before re-enabling normal controls
            yield return new WaitForSeconds(dashRingAnimationDuration);
            if (risingAndFalling != null)
            {
                risingAndFalling.IsAnimationOverridden = false;
            }
        }

        animator.SetBool("IsJumping", false);

        // Wait for a short moment before re-enabling the ground check
        yield return new WaitForSeconds(0.1f); 

        playerPhysics.DisableGroundCheck = false;
    }
}