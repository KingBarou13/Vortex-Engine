using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CameraManager cameraManager;
    public int cameraIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SwitchCamera(cameraIndex);
        }
    }

    private void SwitchCamera(int index)
    {
        Camera[] cameras = cameraManager.cameras;

        if (index >= 0 && index < cameras.Length)
        {
            cameraManager.SwitchCamera(cameras[index]);
        }
        else
        {
            Debug.LogWarning("Camera index out of bounds.");
        }
    }
}
