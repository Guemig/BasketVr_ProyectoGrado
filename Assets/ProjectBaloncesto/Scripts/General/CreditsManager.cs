using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CreditData
{
    [Header("Container")]
    public GameObject container;

    [Header("Image")]
    public Sprite image;
}

public class CreditsManager : MonoBehaviour
{
    [Header("Credits")]
    [SerializeField] private CreditData[] credits;

    [Header("UI")]
    [SerializeField] private Image creditImage;

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
        if (credits.Length == 0)
            return;

        for (int i = 0; i < credits.Length; i++)
        {
            if (credits[i].container != null)
                credits[i].container.SetActive(i == currentIndex);
        }

        if (creditImage != null)
            creditImage.sprite = credits[currentIndex].image;
    }
}
