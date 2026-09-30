using UnityEngine;
using TMPro;

public class QuestKeyboardInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    private TouchScreenKeyboard keyboard;

    private void Start()
    {
        Invoke(nameof(OpenKeyboard), 2f);
    }

    public void OpenKeyboard()
    {
        string currentText = nameInputField != null
            ? nameInputField.text
            : "";

        keyboard = TouchScreenKeyboard.Open(
            currentText,
            TouchScreenKeyboardType.Default,
            false,
            false,
            false,
            false,
            "Ingresa tu nombre"
        );
    }

    private void Update()
    {
        if (keyboard == null)
            return;

        if (nameInputField != null)
            nameInputField.text = keyboard.text;

        if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            Debug.Log("Nombre ingresado: " + keyboard.text);
            keyboard = null;
        }
        else if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            keyboard = null;
        }
    }
}