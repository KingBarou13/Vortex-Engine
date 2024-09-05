using UnityEngine;

public class CinematicPOV : MonoBehaviour
{
    Transform FirstCollidingTransform;
    bool CollidingCinematic = false;
    int NOfTransforms = 0;
    public string CinematicTag;
    CameraRotator CamScript;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == CinematicTag)
        {
            NOfTransforms++;
            if (NOfTransforms == 1)
            {
                int ChildCount = collision.transform.childCount;
                for (int i = 0; i < ChildCount; i++)
                {
                    if (collision.transform.GetChild(i).gameObject.tag == CinematicTag)
                    {
                        FirstCollidingTransform = collision.transform.GetChild(i);
                        CollidingCinematic = true;
                        return;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == CinematicTag)
        {
            NOfTransforms--;
            if (NOfTransforms == 0)
            {
                CollidingCinematic = false;
            }
        }
    }

    private void Start()
    {
        CamScript = FindFirstObjectByType<CameraRotator>();
    }

    private void Update()
    {
        CamScript.InCinematic = CollidingCinematic;
        CamScript.CinematicObject = FirstCollidingTransform;
        Debug.Log(NOfTransforms);
    }
}
