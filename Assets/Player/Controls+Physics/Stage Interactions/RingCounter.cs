using UnityEngine;
using UnityEngine.UI;

public class RingCounter : MonoBehaviour
{
    public static RingCounter Instance;

    [SerializeField] private Text ringText;
    private int ringCount = 0;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddRing(int amount = 1)
    {
        ringCount += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ringText != null)
            ringText.text = "Rings: " + ringCount.ToString();
    }
}