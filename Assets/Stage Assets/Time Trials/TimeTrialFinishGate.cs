using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeTrialFinishGate : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TimeTrialManager.Instance.TryFinishTimeTrial(this);
        }
    }
}