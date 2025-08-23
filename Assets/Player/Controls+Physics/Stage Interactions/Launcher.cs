using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float launchForce;

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

        // Apply vertical launch velocity + current horizontal velocity
        playerPhysics.RB.velocity = (transform.up * launchForce) + playerPhysics.horizontalVelocity;
        Debug.Log("Bounce!");

        var jumpAction = playerPhysics.GetComponent<JumpAction>();
        if (jumpAction != null)
        {
            jumpAction.currentJumps = 0; // Reset jumps after launch
        }

        // Wait for a short moment before re-enabling the ground check
        yield return new WaitForSeconds(0.1f); 

        playerPhysics.DisableGroundCheck = false;
    }
}
