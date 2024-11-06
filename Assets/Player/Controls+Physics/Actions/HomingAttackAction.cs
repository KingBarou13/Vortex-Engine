using UnityEngine;
using UnityEngine.InputSystem;

public class HomingAttackAction : PlayerAction
{
    [SerializeField] private NearestTargetFinder targetFinder;
    [SerializeField] private JumpAction jumpAction;
    [SerializeField] private Animator animator;
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

        playerPhysics.enabled = false;
        playerPhysics.RB.isKinematic = true;
    }

    void FixedUpdate()
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

        animator.SetBool("IsJumping", true);
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

        playerPhysics.RB.velocity = Vector3.zero;

        playerPhysics.RB.AddForce(Vector3.up * bounceUpwardForce, ForceMode.VelocityChange);
        jumpAction.currentJumps = 1;

        Debug.Log($"Target at position: {target.position} was hit!");
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
