using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeTrialStartGate : MonoBehaviour
{
    [Tooltip("The designated finish gate for this time trial.")]
    public TimeTrialFinishGate finishGate;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (finishGate == null)
            {
                Debug.LogError("Finish Gate is not assigned on this Start Gate.", this);
                return;
            }
            TimeTrialManager.Instance.TryStartTimeTrial(this, finishGate);
        }
    }
}