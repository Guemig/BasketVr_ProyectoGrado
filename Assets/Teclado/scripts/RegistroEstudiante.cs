using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegistroEstudiante : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputCodigo;

    [Header("Mensaje")]
    public TMP_Text mensajeError;

    [Header("Cambio de escena")]
    [SerializeField] private float esperaAntesDeCambiar = 0.25f;

    private bool cambiandoEscena = false;

    public void GuardarYContinuar()
    {
        if (cambiandoEscena)
            return;

        string nombre = inputNombre.text.Trim();
        string codigo = inputCodigo.text.Trim();

        // NOMBRE OBLIGATORIO
        if (string.IsNullOrWhiteSpace(nombre))
        {
            MostrarError("Debes ingresar tu nombre.");
            inputNombre.ActivateInputField();
            return;
        }

        // CÓDIGO OBLIGATORIO
        if (string.IsNullOrWhiteSpace(codigo))
        {
            MostrarError("Debes ingresar tu código estudiantil.");
            inputCodigo.ActivateInputField();
            return;
        }

        // Si llegó aquí, ambos campos tienen datos
        if (mensajeError != null)
            mensajeError.text = "";

        PlayerPrefs.SetString("NombreEstudiante", nombre);
        PlayerPrefs.SetString("CodigoEstudiante", codigo);
        PlayerPrefs.Save();

        StartCoroutine(CambiarDeEscena());
    }

    private IEnumerator CambiarDeEscena()
    {
        cambiandoEscena = true;

        yield return new WaitForSecondsRealtime(esperaAntesDeCambiar);

        SceneManager.LoadScene("MenuPrincipal");
    }

    private void MostrarError(string mensaje)
    {
        Debug.LogWarning(mensaje);

        if (mensajeError != null)
            mensajeError.text = mensaje;
    }
}