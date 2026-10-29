using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegistroEstudiante : MonoBehaviour
{
    [Header("Registro")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputCodigoRegistro;
    public Button botonRegistrar;

    [Header("Error Registro")]
    public TMP_Text mensajeErrorRegistro;

    [Header("Inicio de sesión")]
    public TMP_InputField inputCodigoLogin;
    public Button botonIniciarSesion;

    [Header("Error Inicio de sesión")]
    public TMP_Text mensajeErrorLogin;

    [Header("Diagnóstico técnico Firebase")]
    [SerializeField] private TMP_Text mensajeErrorTecnico;

    [Header("Modo Test")]
    [SerializeField] private bool modoTest = false;
    [SerializeField] private string nombreTest = "Sebastian Villamil";
    [SerializeField] private string codigoRegistroTest = "TEST001";
    [SerializeField] private string codigoLoginTest = "TEST001";

    [Header("Cambio de escena")]
    [SerializeField] private float esperaAntesDeCambiar = 0.25f;
    [SerializeField] private string nombreEscenaSiguiente = "EscenaSiguiente";

    private bool cambiandoEscena = false;
    private bool procesandoSolicitud = false;

    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        if (botonRegistrar != null)
            botonRegistrar.onClick.AddListener(RegistrarEstudiante);

        if (botonIniciarSesion != null)
            botonIniciarSesion.onClick.AddListener(IniciarSesion);

        LimpiarErrorRegistro();
        LimpiarErrorLogin();
        LimpiarErrorTecnico();

        if (modoTest)
            RellenarCamposTest();
    }

    // =====================================================
    // MODO TEST
    // =====================================================

    private void RellenarCamposTest()
    {
        if (inputNombre != null)
            inputNombre.text = nombreTest;

        if (inputCodigoRegistro != null)
            inputCodigoRegistro.text = codigoRegistroTest;

        if (inputCodigoLogin != null)
            inputCodigoLogin.text = codigoLoginTest;
    }

    // =====================================================
    // REGISTRO
    // =====================================================

    public async void RegistrarEstudiante()
    {
        if (procesandoSolicitud || cambiandoEscena)
            return;

        LimpiarErrorRegistro();
        LimpiarErrorTecnico();

        string nombre = inputNombre != null
            ? inputNombre.text.Trim()
            : string.Empty;

        string codigo = inputCodigoRegistro != null
            ? inputCodigoRegistro.text.Trim()
            : string.Empty;

        // Validaciones normales del formulario.

        if (string.IsNullOrEmpty(nombre))
        {
            MostrarErrorRegistro("Ingresa tu nombre.");
            return;
        }

        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorRegistro("Ingresa tu código universitario.");
            return;
        }

        // Diagnóstico técnico: instancia de Firebase.

        if (FirestoreService.Instance == null)
        {
            MostrarErrorTecnico(
                "ERROR TÉCNICO: FirestoreService.Instance es NULL.\n\n" +
                "El servicio de Firebase no está disponible o no fue inicializado."
            );
            return;
        }

        procesandoSolicitud = true;

        try
        {
            bool exists =
                await FirestoreService.Instance.StudentCodeExists(codigo);

            if (exists)
            {
                MostrarErrorRegistro(
                    "Este código ya está registrado. Inicia sesión."
                );
                return;
            }

            bool created =
                await FirestoreService.Instance.CreateStudentIfNotExists(
                    nombre,
                    codigo
                );

            if (!created)
            {
                MostrarErrorRegistro(
                    "No fue posible registrar el estudiante. " +
                    "Es posible que el código ya esté registrado."
                );
                return;
            }

            FirestoreService.Instance.SetCurrentStudent(codigo);

            AudioManager.Instance?.PlayUI(UIEvent.Confirm);

            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError("[REGISTRO] ERROR TÉCNICO COMPLETO:\n" + e);

            MostrarErrorTecnico(e.ToString());
        }
        finally
        {
            procesandoSolicitud = false;
        }
    }

    // =====================================================
    // INICIO DE SESIÓN
    // =====================================================

    public async void IniciarSesion()
    {
        if (procesandoSolicitud || cambiandoEscena)
            return;

        LimpiarErrorLogin();
        LimpiarErrorTecnico();

        string codigo = inputCodigoLogin != null
            ? inputCodigoLogin.text.Trim()
            : string.Empty;

        // Validación normal del formulario.

        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorLogin("Ingresa tu código universitario.");
            return;
        }

        // Diagnóstico técnico: instancia de Firebase.

        if (FirestoreService.Instance == null)
        {
            MostrarErrorTecnico(
                "ERROR TÉCNICO: FirestoreService.Instance es NULL.\n\n" +
                "El servicio de Firebase no está disponible o no fue inicializado."
            );
            return;
        }

        procesandoSolicitud = true;

        try
        {
            bool exists =
                await FirestoreService.Instance.StudentCodeExists(codigo);

            if (!exists)
            {
                MostrarErrorLogin(
                    "No existe un estudiante con este código. " +
                    "Regístrate primero."
                );
                return;
            }

            FirestoreService.Instance.SetCurrentStudent(codigo);

            AudioManager.Instance?.PlayUI(UIEvent.Confirm);

            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError("[LOGIN] ERROR TÉCNICO COMPLETO:\n" + e);

            MostrarErrorTecnico(e.ToString());
        }
        finally
        {
            procesandoSolicitud = false;
        }
    }

    // =====================================================
    // DIAGNÓSTICO TÉCNICO
    // =====================================================

    private void MostrarErrorTecnico(string detalle)
    {
        AudioManager.Instance?.PlayUI(UIEvent.Error);

        if (mensajeErrorTecnico != null)
        {
            mensajeErrorTecnico.text = detalle;
            mensajeErrorTecnico.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "[DIAGNÓSTICO] No hay TMP_Text asignado para errores técnicos.\n" +
                detalle
            );
        }
    }

    private void LimpiarErrorTecnico()
    {
        if (mensajeErrorTecnico != null)
        {
            mensajeErrorTecnico.text = string.Empty;
            mensajeErrorTecnico.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // CAMBIO DE ESCENA
    // =====================================================

    private async void CambiarDeEscena()
    {
        if (cambiandoEscena)
            return;

        cambiandoEscena = true;

        if (esperaAntesDeCambiar > 0f)
        {
            await Task.Delay(
                Mathf.RoundToInt(esperaAntesDeCambiar * 1000f)
            );
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScene(nombreEscenaSiguiente);
        }
        else
        {
            Debug.LogError(
                "[REGISTRO] GameManager no está disponible."
            );

            cambiandoEscena = false;
        }
    }

    // =====================================================
    // ERRORES DE REGISTRO
    // =====================================================

    private void MostrarErrorRegistro(string mensaje)
    {
        AudioManager.Instance?.PlayUI(UIEvent.Error);

        if (mensajeErrorRegistro != null)
        {
            mensajeErrorRegistro.text = mensaje;
            mensajeErrorRegistro.gameObject.SetActive(true);
        }
    }

    private void LimpiarErrorRegistro()
    {
        if (mensajeErrorRegistro != null)
        {
            mensajeErrorRegistro.text = string.Empty;
            mensajeErrorRegistro.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // ERRORES DE INICIO DE SESIÓN
    // =====================================================

    private void MostrarErrorLogin(string mensaje)
    {
        AudioManager.Instance?.PlayUI(UIEvent.Error);

        if (mensajeErrorLogin != null)
        {
            mensajeErrorLogin.text = mensaje;
            mensajeErrorLogin.gameObject.SetActive(true);
        }
    }

    private void LimpiarErrorLogin()
    {
        if (mensajeErrorLogin != null)
        {
            mensajeErrorLogin.text = string.Empty;
            mensajeErrorLogin.gameObject.SetActive(false);
        }
    }
}
