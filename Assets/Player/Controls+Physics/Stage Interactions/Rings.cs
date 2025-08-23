using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Rings : MonoBehaviour
{
    [SerializeField] private float cycleLength = 2;
    [SerializeField] private AudioClip ringCollectSound;

    void Start()
    {
        transform.DORotate(new Vector3(0, 360, 0), cycleLength * 0.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ringCollectSound != null)
            {
                AudioSource playerAudio = other.GetComponent<AudioSource>();
                if (playerAudio != null)
                {
                    playerAudio.PlayOneShot(ringCollectSound);
                }
            }

            RingCounter.Instance.AddRing();
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}
