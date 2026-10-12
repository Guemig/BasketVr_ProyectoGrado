
using UnityEngine;

public class PlayerDirectionUI : MonoBehaviour
{
    // =========================================================
    // JUGADOR VR
    // =========================================================

    [Header("JUGADOR VR")]
    [SerializeField] private Transform player;

    // =========================================================
    // REFERENCIAS DE DIRECCIÓN DEL ESCENARIO
    // =========================================================

    [Header("PUNTOS DE REFERENCIA")]
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform leftPoint;

    // =========================================================
    // REPRESENTACIÓN EN EL CANVAS
    // =========================================================

    [Header("REPRESENTACIÓN EN EL CANVAS")]
    [SerializeField] private RectTransform playerIcon;

    // =========================================================
    // ACTUALIZACIÓN
    // =========================================================

    private void LateUpdate()
    {
        UpdatePlayerDirection();
    }

    // =========================================================
    // ACTUALIZAR DIRECCIÓN DEL JUGADOR
    // =========================================================

    private void UpdatePlayerDirection()
    {
        if (player == null || playerIcon == null)
            return;

        // Obtener el frente horizontal del jugador.
        Vector3 playerForward = player.forward;
        playerForward.y = 0f;

        if (playerForward.sqrMagnitude < 0.001f)
            return;

        playerForward.Normalize();

        // Calcular el frente de referencia usando los puntos.
        Vector3 referenceForward = GetHorizontalDirection(
            frontPoint,
            backPoint
        );

        Vector3 referenceRight = GetHorizontalDirection(
            rightPoint,
            leftPoint
        );

        if (referenceForward.sqrMagnitude < 0.001f ||
            referenceRight.sqrMagnitude < 0.001f)
            return;

        referenceForward.Normalize();
        referenceRight.Normalize();

        // Calcular la dirección del jugador respecto al escenario.
        float angle = Mathf.Atan2(
            Vector3.Dot(playerForward, referenceRight),
            Vector3.Dot(playerForward, referenceForward)
        ) * Mathf.Rad2Deg;

        // La imagen apunta hacia arriba cuando mira al frente.
        // Las flechas del Canvas no se modifican.
        playerIcon.localRotation = Quaternion.Euler(
            0f,
            0f,
            -angle
        );
    }

    // =========================================================
    // DIRECCIÓN HORIZONTAL ENTRE DOS REFERENCIAS
    // =========================================================

    private Vector3 GetHorizontalDirection(
        Transform positivePoint,
        Transform negativePoint)
    {
        if (positivePoint == null || negativePoint == null)
            return Vector3.zero;

        Vector3 direction =
            positivePoint.position - negativePoint.position;

        direction.y = 0f;

        return direction;
    }
}
