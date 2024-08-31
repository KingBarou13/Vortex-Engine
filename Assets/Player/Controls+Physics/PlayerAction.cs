using UnityEngine;

public abstract class PlayerAction : MonoBehaviour
{
    [SerializeField] protected PlayerPhysics playerPhysics;

    protected Rigidbody RB => playerPhysics.RB;

    protected PlayerPhysics.GroundInfo groundInfo =>  playerPhysics.groundInfo;
}
