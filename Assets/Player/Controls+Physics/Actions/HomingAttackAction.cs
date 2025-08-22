using UnityEngine;
using UnityEngine.InputSystem;

public class HomingAttackAction : PlayerAction
{
    [Header("References")]
    [SerializeField] private NearestTargetFinder targetFinder;
    [SerializeField] private JumpAction jumpAction;
    [SerializeField] private Animator animator;
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private Transform player;

    [Header("Homing Attack")]
    [SerializeField] private float homingSpeed = 20f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float bounceUpwardForce = 10f;

    [Header("Air Dash")]
    [SerializeField] private float airdashSpeed = 30f;
    [SerializeField] private float airdashDuration = 0.3f;
    [SerializeField] private int airdashesAvailable = 1;

    private int currentAirdashes;
    private bool isHoming = false;

    public bool IsHoming => isHoming;

    private Transform target;

    void OnEnable()
    {
        playerPhysics.onGroundEnter += ResetAirdashes;
    }

    private void OnDisable()
    {
        playerPhysics.onGroundEnter -= ResetAirdashes;
    }

    private void ResetAirdashes()
    {
        currentAirdashes = airdashesAvailable;
    }

    public void OnHomingAttack(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && !playerPhysics.groundInfo.ground)
        {
            AttemptHomingAttack();
        }
    }

    private void AttemptHomingAttack()
    {
        if (isHoming) return;

        if (targetFinder.nearestTarget != null)
        {
            target = targetFinder.nearestTarget;
            Debug.Log($"Target found at position: {target.position}");
            isHoming = true;

            playerPhysics.enabled = false;
            playerPhysics.RB.isKinematic = true;
        }
        else if (currentAirdashes > 0)
        {
            // Perform Air Dash
            currentAirdashes--;
            Vector3 dashDirection = player.transform.forward;
            RB.velocity = dashDirection * airdashSpeed + playerPhysics.verticalVelocity;
            moveAction.TriggerControlLock(airdashDuration);
            animator.SetBool("IsJumping", true);
        }
        else
        {
            Debug.LogWarning("No target found for homing attack and no airdashes available.");
        }
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
            playerPhysics.RB.isKinematic = false;
            playerPhysics.enabled = true;
            return;
        }

        animator.SetBool("IsJumping", true);
        Vector3 newPosition = Vector3.MoveTowards(playerPhysics.RB.position, target.position, homingSpeed * Time.deltaTime);
        playerPhysics.RB.MovePosition(newPosition);

        Vector3 direction = (target.position - RB.transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.RotateTowards(RB.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        ResetAirdashes();

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