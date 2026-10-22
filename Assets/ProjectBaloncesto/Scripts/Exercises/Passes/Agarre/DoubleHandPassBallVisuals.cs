using UnityEngine;

public class DoubleHandPassBallVisuals : MonoBehaviour
{
    // =====================================================
    // RENDERER DEL BALÓN
    // =====================================================

    [Header("Renderer del balón")]
    [SerializeField] private Renderer ballRenderer;

    // =====================================================
    // COLORES POR ESTADO
    // =====================================================

    [Header("Colores por estado")]
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeCatchColor = Color.green;
    [SerializeField] private Color activeLetPassColor = Color.red;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;

    // =====================================================
    // SONIDOS DE RESULTADO
    // =====================================================

    [Header("Audio de resultados")]
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip errorClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 1f;

    // =====================================================
    // MATERIAL
    // =====================================================

    private Material ballMaterial;

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Awake()
    {
        if (ballRenderer == null)
            ballRenderer = GetComponentInChildren<Renderer>();

        if (ballRenderer == null)
        {
            Debug.LogError(
                "[DoubleHandPassBallVisuals] No se encontró el Renderer.",
                this
            );
        }
        else
        {
            // Instancia propia del material de este balón.
            ballMaterial = ballRenderer.material;

            UpdateColor(DoubleHandPassBall.BallState.Inactive);
        }
    }

    // =====================================================
    // ACTUALIZAR ESTADO VISUAL
    // =====================================================

    public void SetVisualState(DoubleHandPassBall.BallState state)
    {
        UpdateColor(state);
    }

    // =====================================================
    // REPRODUCIR SONIDO SEGÚN RESULTADO
    // =====================================================

    public void PlayCorrectSound()
    {
        PlayResultSound(correctClip);
    }

    public void PlayErrorSound()
    {
        PlayResultSound(errorClip);
    }

    private void PlayResultSound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "[DoubleHandPassBallVisuals] No hay un AudioClip asignado.",
                this
            );

            return;
        }

        // Reproduce el sonido independientemente de la activación
        // o desactivación del balón que pertenece al pool.
        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            volume
        );
    }

    // =====================================================
    // ACTUALIZAR COLOR
    // =====================================================

    private void UpdateColor(DoubleHandPassBall.BallState state)
    {
        if (ballMaterial == null)
            return;

        Color targetColor = GetColorForState(state);

        if (ballMaterial.HasProperty(BaseColorId))
        {
            ballMaterial.SetColor(BaseColorId, targetColor);
        }
        else if (ballMaterial.HasProperty(ColorId))
        {
            ballMaterial.SetColor(ColorId, targetColor);
        }
        else
        {
            Debug.LogWarning(
                "[DoubleHandPassBallVisuals] El shader del balón " +
                "no tiene una propiedad de color compatible.",
                this
            );
        }
    }

    // =====================================================
    // OBTENER COLOR SEGÚN ESTADO
    // =====================================================

    private Color GetColorForState(
        DoubleHandPassBall.BallState state)
    {
        switch (state)
        {
            case DoubleHandPassBall.BallState.Inactive:
                return inactiveColor;

            case DoubleHandPassBall.BallState.ActiveCatch:
                return activeCatchColor;

            case DoubleHandPassBall.BallState.ActiveLetPass:
                return activeLetPassColor;

            case DoubleHandPassBall.BallState.Correct:
                return correctColor;

            case DoubleHandPassBall.BallState.Incorrect:
                return incorrectColor;

            default:
                return inactiveColor;
        }
    }

    // =====================================================
    // LIBERAR MATERIAL
    // =====================================================

    private void OnDestroy()
    {
        if (ballMaterial != null)
            Destroy(ballMaterial);
    }
}