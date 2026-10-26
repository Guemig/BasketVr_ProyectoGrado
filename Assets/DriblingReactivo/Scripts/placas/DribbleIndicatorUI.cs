using UnityEngine;
using UnityEngine.UI;

public class DribbleIndicatorUI : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject indicatorCanvas;

    [Header("Placa 1")]
    [SerializeField] private DribblePlate plate1;
    [SerializeField] private Image image1;

    [Header("Placa 2")]
    [SerializeField] private DribblePlate plate2;
    [SerializeField] private Image image2;

    [Header("Placa 3")]
    [SerializeField] private DribblePlate plate3;
    [SerializeField] private Image image3;

    [Header("Placa 4")]
    [SerializeField] private DribblePlate plate4;
    [SerializeField] private Image image4;

    [Header("Placa 5")]
    [SerializeField] private DribblePlate plate5;
    [SerializeField] private Image image5;

    [Header("Colores")]
    [SerializeField] private Color offColor = Color.gray;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color orangeColor =
        new Color(
            1f,
            0.5f,
            0f,
            1f
        );

    private void Awake()
    {
        Subscribe();

        HideIndicator();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (plate1 != null)
            plate1.OnStateChanged +=
                OnPlateStateChanged;

        if (plate2 != null)
            plate2.OnStateChanged +=
                OnPlateStateChanged;

        if (plate3 != null)
            plate3.OnStateChanged +=
                OnPlateStateChanged;

        if (plate4 != null)
            plate4.OnStateChanged +=
                OnPlateStateChanged;

        if (plate5 != null)
            plate5.OnStateChanged +=
                OnPlateStateChanged;
    }

    private void Unsubscribe()
    {
        if (plate1 != null)
            plate1.OnStateChanged -=
                OnPlateStateChanged;

        if (plate2 != null)
            plate2.OnStateChanged -=
                OnPlateStateChanged;

        if (plate3 != null)
            plate3.OnStateChanged -=
                OnPlateStateChanged;

        if (plate4 != null)
            plate4.OnStateChanged -=
                OnPlateStateChanged;

        if (plate5 != null)
            plate5.OnStateChanged -=
                OnPlateStateChanged;
    }

    public void ShowTutorialMode()
    {
        ShowTwoImages();
    }

    public void ShowLevel1Mode()
    {
        ShowTwoImages();
    }

    public void ShowLevel2Mode()
    {
        ShowFourImages();
    }

    public void ShowLevel3Mode()
    {
        ShowFourImages();
    }

    private void ShowTwoImages()
    {
        if (indicatorCanvas != null)
            indicatorCanvas.SetActive(true);

        SetImageVisible(image1, true);
        SetImageVisible(image2, true);
        SetImageVisible(image3, false);
        SetImageVisible(image4, false);
        SetImageVisible(image5, false);

        RefreshAll();
    }

    private void ShowFourImages()
    {
        if (indicatorCanvas != null)
            indicatorCanvas.SetActive(true);

        SetImageVisible(image1, true);
        SetImageVisible(image2, true);
        SetImageVisible(image3, true);
        SetImageVisible(image4, true);
        SetImageVisible(image5, false);

        RefreshAll();
    }

    public void ShowFiveImages()
    {
        if (indicatorCanvas != null)
            indicatorCanvas.SetActive(true);

        SetImageVisible(image1, true);
        SetImageVisible(image2, true);
        SetImageVisible(image3, true);
        SetImageVisible(image4, true);
        SetImageVisible(image5, true);

        RefreshAll();
    }

    public void HideIndicator()
    {
        if (indicatorCanvas != null)
            indicatorCanvas.SetActive(false);
    }

    private void OnPlateStateChanged(
        DribblePlate plate)
    {
        if (plate == plate1)
            UpdateImage(image1, plate1);

        if (plate == plate2)
            UpdateImage(image2, plate2);

        if (plate == plate3)
            UpdateImage(image3, plate3);

        if (plate == plate4)
            UpdateImage(image4, plate4);

        if (plate == plate5)
            UpdateImage(image5, plate5);
    }

    private void RefreshAll()
    {
        UpdateImage(image1, plate1);
        UpdateImage(image2, plate2);
        UpdateImage(image3, plate3);
        UpdateImage(image4, plate4);
        UpdateImage(image5, plate5);
    }

    private void UpdateImage(
        Image image,
        DribblePlate plate)
    {
        if (image == null ||
            plate == null)
        {
            return;
        }

        switch (plate.Type)
        {
            case DribblePlate.PlateType.Off:
                image.color = offColor;
                break;

            case DribblePlate.PlateType.Green:
                image.color = greenColor;
                break;

            case DribblePlate.PlateType.Orange:
                image.color = orangeColor;
                break;
        }
    }

    private void SetImageVisible(
        Image image,
        bool visible)
    {
        if (image == null)
            return;

        image.gameObject.SetActive(
            visible
        );
    }
}