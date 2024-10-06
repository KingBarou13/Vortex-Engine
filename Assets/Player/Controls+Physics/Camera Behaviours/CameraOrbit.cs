using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5.0f;
    public float distance = 5.0f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float yMinLimit = -40f;
    private float yMaxLimit = 80f;

    [SerializeField] private InputActionReference move;

    void OnEnable()
    {
        move.action.Enable();
    }

    void OnDisable()
    {
        move.action.Disable();
    }

    void Update()
    {
        Vector2 input = move.action.ReadValue<Vector2>();
        currentX += input.x * rotationSpeed * Time.deltaTime;
        currentY -= input.y * rotationSpeed * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 direction = new Vector3(0, 0, -distance);
            Vector3 position = rotation * direction + target.position;

            transform.position = position;
            transform.LookAt(target);
        }
    }
}
