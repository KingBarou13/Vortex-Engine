using UnityEngine;
using UnityEngine.InputSystem;

public class HomingAttackAction : PlayerAction
{
    [SerializeField] private NearestTargetFinder targetFinder;
    [SerializeField] private float homingSpeed = 20f;     
    [SerializeField] private float rotationSpeed = 200f;       
    [SerializeField] private float bounceUpwardForce = 10f;      

    private bool isHoming = false;

    public bool IsHoming => isHoming;

    private Transform target;

    public void OnHomingAttack(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && !playerPhysics.groundInfo.ground)
        {
            AttemptHomingAttack();
        }
    }

    private void AttemptHomingAttack()
    {
        if (isHoming || targetFinder.nearestTarget == null)
        {
            Debug.LogWarning("No target found for homing attack.");
            return;
        }

        target = targetFinder.nearestTarget;
        Debug.Log($"Target found at position: {target.position}");
        isHoming = true;

        playerPhysics.enabled = false; // Disable player physics temporarily
        playerPhysics.RB.isKinematic = true; // Set to kinematic to avoid physics interactions
    }

    void Update()
    {
        if (isHoming)
        {
            MoveTowardsTarget();
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null)
        {
            isHoming = false;
            return;
        }

        // Calculate new position and move towards the target
        Vector3 newPosition = Vector3.MoveTowards(playerPhysics.RB.position, target.position, homingSpeed * Time.deltaTime);
        playerPhysics.RB.MovePosition(newPosition);

        Vector3 direction = (target.position - RB.transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            RB.MoveRotation(newRotation);
        }

        if (Vector3.Distance(playerPhysics.RB.position, target.position) < 0.5f)
        {
            OnHomingAttackComplete();
        }
    }

    private void OnHomingAttackComplete()
    {
        playerPhysics.RB.isKinematic = false;
        playerPhysics.enabled = true;
        Vector3 currentVelocity = playerPhysics.RB.velocity;
        playerPhysics.RB.velocity = new Vector3(0, currentVelocity.y, 0);
        RB.AddForce(Vector3.up * bounceUpwardForce, ForceMode.Impulse);
        isHoming = false; 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isHoming && collision.transform == target)
        {
            OnHomingAttackComplete();
        }
    }
}
