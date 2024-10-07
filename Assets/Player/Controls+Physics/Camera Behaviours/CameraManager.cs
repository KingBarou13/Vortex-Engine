using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera[] cameras;
    public Camera activeCamera;

    void Start()
    {
        if (cameras.Length > 0)
        {
            activeCamera = cameras[0];
            SwitchCamera(activeCamera);
        }
    }

    public void SwitchCamera(Camera newCamera)
    {
        foreach (var cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }
        newCamera.gameObject.SetActive(true);
        activeCamera = newCamera;
    }

    public Camera GetActiveCamera()
    {
        return activeCamera;
    }
}
