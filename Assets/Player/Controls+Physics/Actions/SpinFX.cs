using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpinFX : MonoBehaviour
{
    [SerializeField] private float spinSpeed;
    [SerializeField] private Transform referenceObject; // Reference to the object to match y-rotation
    [SerializeField] private Animator animator;

    private Tween spinTween;

    void Start()
    {
        // Start spinning the object around the x-axis
        spinTween = transform.DORotate(new Vector3(360, 0, 0), spinSpeed * 0.5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    void Update()
    {
        // Update y-rotation to match the reference object's y-rotation
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, referenceObject.rotation.eulerAngles.y, currentRotation.z);

        if (animator.GetBool("IsJumping") == false)
        {
            gameObject.SetActive(false);
        }
    }
}
