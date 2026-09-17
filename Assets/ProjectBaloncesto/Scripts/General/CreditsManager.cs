using UnityEngine;
using TMPro;

[System.Serializable]
public class CreditData
{
    [Header("Container")]
    public GameObject container;

    [Header("Text")]
    [TextArea(2, 5)]
    public string text;
}

public class CreditsManager : MonoBehaviour
{
    [Header("Credits")]
    [SerializeField] private CreditData[] credits;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI creditText;

    private int currentIndex = 0;

    private void Start()
    {
        ShowCurrent();
    }

    public void Next()
    {
        if (credits.Length == 0)
            return;

        currentIndex++;

        if (currentIndex >= credits.Length)
            currentIndex = 0;

        ShowCurrent();
    }

    public void Preview()
    {
        if (credits.Length == 0)
            return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = credits.Length - 1;

        ShowCurrent();
    }

    private void ShowCurrent()
    {
        for (int i = 0; i < credits.Length; i++)
        {
            if (credits[i].container != null)
                credits[i].container.SetActive(i == currentIndex);
        }

        if (creditText != null)
            creditText.text = credits[currentIndex].text;
    }
}