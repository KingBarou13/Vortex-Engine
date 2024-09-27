using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhysicsTester : MonoBehaviour
{
    private float upDownAngle;
    public Text displayText; // Reference to the UI Text component
    public float downRange = 5f; // Range for the downward raycast

    void Start()
    {
        if (displayText == null)
        {
            Debug.LogError("Text component is not assigned to the PhysicsTester script!");
        }
    }

    void Update()
    {
        CalculateUpDownAngle();
        UpdateDisplayText();
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
