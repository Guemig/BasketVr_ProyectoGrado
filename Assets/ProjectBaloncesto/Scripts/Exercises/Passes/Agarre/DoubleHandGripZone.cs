
using UnityEngine;

public class DoubleHandGripZone : MonoBehaviour
{
    [Header("Modelo visual")]
    [SerializeField] private GameObject gripVisual;
    [SerializeField] private Renderer[] visualRenderers;

    [Header("Colores")]
    [SerializeField]
    private Color readyColor =
        new Color(0.1f, 0.9f, 1f, 0.35f);

    [SerializeField]
    private Color caughtColor =
        new Color(0.2f, 1f, 0.3f, 0.55f);

    [SerializeField]
    private Color inactiveColor =
        new Color(1f, 0.25f, 0.2f, 0.25f);

    private Collider zoneCollider;
    private MaterialPropertyBlock propertyBlock;

    private bool gripReady;
    private bool ballCaught;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        propertyBlock = new MaterialPropertyBlock();

        if (zoneCollider == null)
        {
            Debug.LogError(
                "[DoubleHandGripZone] Falta un Collider en la raíz.",
                this
            );
        }
        else
        {
            zoneCollider.isTrigger = true;
        }

        if (visualRenderers == null ||
            visualRenderers.Length == 0)
        {
            if (gripVisual != null)
            {
                visualRenderers =
                    gripVisual.GetComponentsInChildren<Renderer>(true);
            }
        }

        ApplyColor(inactiveColor);

        if (gripVisual != null)
            gripVisual.SetActive(false);
    }

    public void SetGripReady(bool ready)
    {
        gripReady = ready;

        if (!ready)
            ballCaught = false;

        if (gripVisual != null)
            gripVisual.SetActive(ready);

        if (zoneCollider != null)
            zoneCollider.enabled = ready;

        if (ready)
            ApplyColor(ballCaught ? caughtColor : readyColor);
        else
            ApplyColor(inactiveColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gripReady || ballCaught)
            return;

        DoubleHandPassBall ball =
            other.GetComponentInParent<DoubleHandPassBall>();

        if (ball == null)
            return;

        if (ball.TryCatch(this))
        {
            ballCaught = true;
            ApplyColor(caughtColor);
        }
    }

    private void ApplyColor(Color color)
    {
        if (visualRenderers == null)
            return;

        foreach (Renderer targetRenderer in visualRenderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
