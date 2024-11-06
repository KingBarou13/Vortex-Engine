using UnityEngine;
using UnityEngine.UI;

public class HomingReticle : MonoBehaviour
{
    public NearestTargetFinder targetFinder;
    public PlayerPhysics playerPhysics; 
    public Image reticleImage;
    public Camera mainCamera;
    public Vector3 offset;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetFinder.nearestTarget != null)
        {
            // Get the screen position of the nearest target
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetFinder.nearestTarget.position + offset);

            // Check if the target is in front of the camera
            if (screenPos.z > 0 && !playerPhysics.groundInfo.ground)
            {
                reticleImage.transform.position = screenPos; // Set the reticle position to follow the target
                reticleImage.enabled = true; // Show the reticle
            }
            else
            {
                reticleImage.enabled = false; // Hide the reticle if the target is behind the camera
            }
        }
        else
        {
            reticleImage.enabled = false; // Hide the reticle if no target is found
        }
    }
}
