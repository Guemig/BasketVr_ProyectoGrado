
using System.Collections;
using UnityEngine;

public class ReactionHoopsInicio : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasAdvertencias;
    [SerializeField] private GameObject canvasPrincipal;

    [Header("Audio")]
    [SerializeField] private AudioSource audioAdvertencias;

    [Header("Configuracion")]
    [SerializeField] private float duracionAdvertencias = 41f;
    [SerializeField] private float duracionFadeIn = 1.5f;
    [SerializeField] private float duracionFadeOut = 1.5f;

    private Coroutine secuencia;

    private void Start()
    {
        // Comprobar si las advertencias ya fueron mostradas.
        if (GameManager.advertenciasMostradas)
        {
            canvasAdvertencias.gameObject.SetActive(false);
            canvasPrincipal.SetActive(true);

            if (audioAdvertencias != null)
            {
                audioAdvertencias.Stop();
            }

            return;
        }

        // Registrar que las advertencias ya se van a mostrar.
        GameManager.advertenciasMostradas = true;

        canvasAdvertencias.gameObject.SetActive(true);
        canvasAdvertencias.alpha = 0f;
        canvasAdvertencias.interactable = false;
        canvasAdvertencias.blocksRaycasts = false;

        canvasPrincipal.SetActive(false);

        secuencia = StartCoroutine(SecuenciaInicio());
    }

    private IEnumerator SecuenciaInicio()
    {
        if (audioAdvertencias != null)
        {
            audioAdvertencias.Stop();
            audioAdvertencias.Play();
        }

        yield return StartCoroutine(
            AnimarTransparencia(0f, 1f, duracionFadeIn)
        );

        float espera = Mathf.Max(
            0f,
            duracionAdvertencias - duracionFadeIn - duracionFadeOut
        );

        yield return new WaitForSecondsRealtime(espera);

        yield return StartCoroutine(
            AnimarTransparencia(1f, 0f, duracionFadeOut)
        );

        if (audioAdvertencias != null)
        {
            audioAdvertencias.Stop();
        }

        canvasAdvertencias.gameObject.SetActive(false);
        canvasPrincipal.SetActive(true);

        secuencia = null;
    }

    private IEnumerator AnimarTransparencia(
        float inicio,
        float destino,
        float duracion
    )
    {
        if (duracion <= 0f)
        {
            canvasAdvertencias.alpha = destino;
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);
            progreso = Mathf.SmoothStep(0f, 1f, progreso);

            canvasAdvertencias.alpha = Mathf.Lerp(
                inicio,
                destino,
                progreso
            );

            yield return null;
        }

        canvasAdvertencias.alpha = destino;
    }

    private void OnDisable()
    {
        if (secuencia != null)
        {
            StopCoroutine(secuencia);
            secuencia = null;
        }

        if (audioAdvertencias != null)
        {
            audioAdvertencias.Stop();
        }
    }
}
