using DG.Tweening;
using UnityEngine;

public class EnemyHover : MonoBehaviour
{
    [SerializeField] private float cycleLength = 2;
    [SerializeField] private float distance = 10; 

    void Start()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y + distance, startPosition.z);
        transform.DOMove(targetPosition, cycleLength).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
