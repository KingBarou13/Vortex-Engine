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
        // Apply vertical launch velocity + current horizontal velocity
        playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * launchForce) + playerPhysics.horizontalVelocity;
        Debug.Log("Bounce!");

        var jumpAction = playerPhysics.GetComponent<JumpAction>();
        if (jumpAction != null)
        {
            jumpAction.currentJumps = 0; // Reset jumps after launch
        }
    }
}
