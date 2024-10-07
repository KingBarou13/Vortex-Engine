using UnityEngine;
using UnityEngine.UI;

public class CameraSymbol : MonoBehaviour
{
    public Image cameraImage;
    public CameraManager cameraManager;

    void Update()
    {
        if (cameraManager != null && cameraManager.cameras.Length > 0)
        {
            if (cameraManager.activeCamera == cameraManager.cameras[0])
            {
                cameraImage.enabled = false;
            }
            else
            {
                cameraImage.enabled = true;
            }
        }
    }
}
