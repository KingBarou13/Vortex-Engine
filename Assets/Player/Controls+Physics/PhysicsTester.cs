using UnityEngine;
using UnityEngine.UI;

public class PhysicsTester : MonoBehaviour
{
    public float range = 5f;
    private float upDownAngle;
    public Text displayText; // Reference to the UI Text component

    void Start()
    {
        if (displayText == null)
        {
            Debug.LogError("Text component is not assigned to the PhysicsTester script!");
        }
    }

    void Update()
    {
        CheckDirection();
        CalculateUpDownAngle();
        UpdateDisplayText();
    }

    void CheckDirection()
    {
        Vector3 direction = Vector3.forward;
        Ray theRay = new Ray(transform.position, transform.TransformDirection(direction * range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range), Color.red);
    }

    void CalculateUpDownAngle()
    {
        // Calculate the dot product between the forward vector and the projected forward vector on the XZ plane
        Vector3 forward = transform.forward;
        Vector3 projectedForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

        // Use Vector3.Dot to get the cosine of the angle between the forward vector and the projected forward vector
        float cosAngle = Vector3.Dot(projectedForward, forward.normalized);

        // Calculate the angle in degrees using acos (inverse cosine) and then determine the sign
        float angleInDegrees = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

        // Determine the sign based on whether we're looking up or down
        float sign = forward.y > 0 ? 1 : -1;

        // Multiply by sign to get the final up/down angle
        upDownAngle = angleInDegrees * sign;
    }

    void UpdateDisplayText()
    {
        if (displayText != null)
        {
            displayText.text = $"Up/Down Angle: {upDownAngle:F2} degrees";
        }
    }
}
