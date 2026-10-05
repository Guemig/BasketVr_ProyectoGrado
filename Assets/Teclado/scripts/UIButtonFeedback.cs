using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFeedback : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Animación")]
    [SerializeField] private float escalaPresionada = 1.08f;
    [SerializeField] private float velocidad = 15f;

    private Vector3 escalaOriginal;
    private Vector3 escalaObjetivo;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        escalaObjetivo = escalaOriginal;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            escalaObjetivo,
            Time.unscaledDeltaTime * velocidad
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * escalaPresionada;

        if (UISounds.Instance != null)
        {
            UISounds.Instance.ReproducirBoton();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal;
    }
}