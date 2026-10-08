using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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


    [Header("Cambio de escena")]
    [SerializeField]
    private float esperaAntesDeCambiar = 0.25f;

    [SerializeField]
    private string nombreEscenaSiguiente =
        "EscenaSiguiente";


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
    }


    // =====================================================
    // REGISTRO
    // =====================================================

    public async void RegistrarEstudiante()
    {
        if (procesandoSolicitud ||
            cambiandoEscena)
        {
            return;
        }


        LimpiarErrorRegistro();


        string nombre =
            inputNombre != null
                ? inputNombre.text.Trim()
                : string.Empty;


        string codigo =
            inputCodigoRegistro != null
                ? inputCodigoRegistro.text.Trim()
                : string.Empty;


        if (string.IsNullOrEmpty(nombre))
        {
            MostrarErrorRegistro(
                "Ingresa tu nombre."
            );

            return;
        }


        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorRegistro(
                "Ingresa tu código universitario."
            );

            return;
        }


        procesandoSolicitud = true;


        try
        {
            bool exists =
                await FirestoreService.Instance
                    .StudentCodeExists(
                        codigo
                    );


            if (exists)
            {
                MostrarErrorRegistro(
                    "Este código ya está registrado. " +
                    "Inicia sesión."
                );

                return;
            }


            bool created =
                await FirestoreService.Instance
                    .CreateStudentIfNotExists(
                        nombre,
                        codigo
                    );


            if (!created)
            {
                MostrarErrorRegistro(
                    "No fue posible registrar el estudiante."
                );

                return;
            }


            // Guardar estudiante actual.
            FirestoreService.Instance
                .SetCurrentStudent(
                    codigo
                );


            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[REGISTRO] Error: " +
                e
            );


            MostrarErrorRegistro(
                "Ocurrió un error al registrar. " +
                "Intenta nuevamente."
            );
        }
        finally
        {
            procesandoSolicitud = false;
        }
    }


    // =====================================================
    // LOGIN
    // =====================================================

    public async void IniciarSesion()
    {
        if (procesandoSolicitud ||
            cambiandoEscena)
        {
            return;
        }


        LimpiarErrorLogin();


        string codigo =
            inputCodigoLogin != null
                ? inputCodigoLogin.text.Trim()
                : string.Empty;


        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorLogin(
                "Ingresa tu código universitario."
            );

            return;
        }


        procesandoSolicitud = true;


        try
        {
            bool exists =
                await FirestoreService.Instance
                    .StudentCodeExists(
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


            // Guardar estudiante actual.
            FirestoreService.Instance
                .SetCurrentStudent(
                    codigo
                );


            CambiarDeEscena();
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[LOGIN] Error: " +
                e
            );


            MostrarErrorLogin(
                "Ocurrió un error al iniciar sesión. " +
                "Intenta nuevamente."
            );
        }
        finally
        {
            procesandoSolicitud = false;
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
                Mathf.RoundToInt(
                    esperaAntesDeCambiar * 1000f
                )
            );
        }


        SceneManager.LoadScene(
            nombreEscenaSiguiente
        );
    }


    // =====================================================
    // ERRORES REGISTRO
    // =====================================================

    private void MostrarErrorRegistro(
        string mensaje)
    {
        if (mensajeErrorRegistro != null)
        {
            mensajeErrorRegistro.text =
                mensaje;

            mensajeErrorRegistro.gameObject
                .SetActive(true);
        }
    }


    private void LimpiarErrorRegistro()
    {
        if (mensajeErrorRegistro != null)
        {
            mensajeErrorRegistro.text =
                string.Empty;

            mensajeErrorRegistro.gameObject
                .SetActive(false);
        }
    }


    // =====================================================
    // ERRORES LOGIN
    // =====================================================

    private void MostrarErrorLogin(
        string mensaje)
    {
        if (mensajeErrorLogin != null)
        {
            mensajeErrorLogin.text =
                mensaje;

            mensajeErrorLogin.gameObject
                .SetActive(true);
        }
    }


    private void LimpiarErrorLogin()
    {
        if (mensajeErrorLogin != null)
        {
            mensajeErrorLogin.text =
                string.Empty;

            mensajeErrorLogin.gameObject
                .SetActive(false);
        }
    }
}