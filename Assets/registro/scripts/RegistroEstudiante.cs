
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

    [Header("Modo Test")]
    [SerializeField]
    private bool modoTest = false;

    [SerializeField]
    private string nombreTest = "Sebastian Villamil";

    [SerializeField]
    private string codigoRegistroTest = "TEST001";

    [SerializeField]
    private string codigoLoginTest = "TEST001";

    [Header("Cambio de escena")]
    [SerializeField]
    private float esperaAntesDeCambiar = 0.25f;

    [SerializeField]
    private string nombreEscenaSiguiente = "EscenaSiguiente";

    private bool cambiandoEscena = false;
    private bool procesandoSolicitud = false;

    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        if (botonRegistrar != null)
        {
            botonRegistrar.onClick.AddListener(
                RegistrarEstudiante
            );
        }

        if (botonIniciarSesion != null)
        {
            botonIniciarSesion.onClick.AddListener(
                IniciarSesion
            );
        }

        LimpiarErrorRegistro();
        LimpiarErrorLogin();

        if (modoTest)
        {
            RellenarCamposTest();
        }
    }

    // =====================================================
    // MODO TEST
    // =====================================================

    private void RellenarCamposTest()
    {
        if (inputNombre != null)
        {
            inputNombre.text = nombreTest;
        }

        if (inputCodigoRegistro != null)
        {
            inputCodigoRegistro.text = codigoRegistroTest;
        }

        if (inputCodigoLogin != null)
        {
            inputCodigoLogin.text = codigoLoginTest;
        }
    }

    // =====================================================
    // REGISTRO
    // =====================================================

    public async void RegistrarEstudiante()
    {
        if (procesandoSolicitud || cambiandoEscena)
        {
            return;
        }

        LimpiarErrorRegistro();

        string nombre = inputNombre != null
            ? inputNombre.text.Trim()
            : string.Empty;

        string codigo = inputCodigoRegistro != null
            ? inputCodigoRegistro.text.Trim()
            : string.Empty;

        if (string.IsNullOrEmpty(nombre))
        {
            MostrarErrorRegistro("Ingresa tu nombre.");
            return;
        }

        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorRegistro(
                "Ingresa tu código universitario."
            );
            return;
        }

        if (FirestoreService.Instance == null)
        {
            MostrarErrorRegistro(
                "El servicio de base de datos no está disponible."
            );
            return;
        }

        procesandoSolicitud = true;

        try
        {
            bool exists =
                await FirestoreService.Instance.StudentCodeExists(
                    codigo
                );

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

            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError("[REGISTRO] Error: " + e);

            MostrarErrorRegistro(
                ObtenerMensajeError(e)
            );
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
        {
            return;
        }

        LimpiarErrorLogin();

        string codigo = inputCodigoLogin != null
            ? inputCodigoLogin.text.Trim()
            : string.Empty;

        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorLogin(
                "Ingresa tu código universitario."
            );
            return;
        }

        if (FirestoreService.Instance == null)
        {
            MostrarErrorLogin(
                "El servicio de base de datos no está disponible."
            );
            return;
        }

        procesandoSolicitud = true;

        try
        {
            bool exists =
                await FirestoreService.Instance.StudentCodeExists(
                    codigo
                );

            if (!exists)
            {
                MostrarErrorLogin(
                    "No existe un estudiante con este código. " +
                    "Regístrate primero."
                );
                return;
            }

            FirestoreService.Instance.SetCurrentStudent(codigo);

            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError("[LOGIN] Error: " + e);

            MostrarErrorLogin(
                ObtenerMensajeError(e)
            );
        }
        finally
        {
            procesandoSolicitud = false;
        }
    }

    // =====================================================
    // INTERPRETACIÓN DE ERRORES
    // =====================================================

    private string ObtenerMensajeError(Exception e)
    {
        string error = e.ToString().ToLowerInvariant();

        if (error.Contains("unavailable") ||
            error.Contains("network") ||
            error.Contains("connection"))
        {
            return "No hay conexión con el servidor. " +
                   "Verifica tu conexión a internet.";
        }

        if (error.Contains("permission-denied") ||
            error.Contains("permission denied"))
        {
            return "No tienes permiso para realizar esta operación.";
        }

        if (error.Contains("deadline-exceeded") ||
            error.Contains("timed out") ||
            error.Contains("timeout"))
        {
            return "La solicitud tardó demasiado. " +
                   "Intenta nuevamente.";
        }

        if (error.Contains("unauthenticated"))
        {
            return "La sesión no está autorizada. " +
                   "Intenta nuevamente.";
        }

        if (error.Contains("failed-precondition"))
        {
            return "La base de datos no está configurada " +
                   "correctamente para esta operación.";
        }

        if (error.Contains("already-exists") ||
            error.Contains("already exists"))
        {
            return "Este registro ya existe.";
        }

        if (error.Contains("cancelled") ||
            error.Contains("canceled"))
        {
            return "La operación fue cancelada.";
        }

        if (error.Contains("not-found") ||
            error.Contains("not found"))
        {
            return "No se encontró la información solicitada.";
        }

        return "No fue posible completar la operación. " +
               "Intenta nuevamente.";
    }

    // =====================================================
    // CAMBIO DE ESCENA
    // =====================================================

    private async void CambiarDeEscena()
    {
        if (cambiandoEscena)
        {
            return;
        }

        cambiandoEscena = true;

        if (esperaAntesDeCambiar > 0f)
        {
            await Task.Delay(
                Mathf.RoundToInt(esperaAntesDeCambiar * 1000f)
            );
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScene(
                nombreEscenaSiguiente
            );
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
