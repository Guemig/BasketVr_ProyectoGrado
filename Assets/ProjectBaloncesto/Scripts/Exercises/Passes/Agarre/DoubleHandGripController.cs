
using UnityEngine;

public class DoubleHandGripController : MonoBehaviour
{
    [Header("Manos rastreadas")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Zona de agarre")]
    [SerializeField] private DoubleHandGripZone gripZone;

    [Header("Activación por distancia")]
    [SerializeField, Min(0.01f)]
    private float activationDistance = 0.25f;

    [Header("Orientación de las palmas")]
    [Tooltip("Eje local que apunta hacia fuera de la palma.")]
    [SerializeField] private PalmAxis palmAxis = PalmAxis.Forward;

    [Tooltip("Ángulo máximo permitido para que cada palma mire hacia la otra mano.")]
    [SerializeField, Range(0f, 90f)]
    private float maxPalmAngle = 35f;

    [Header("Posición de la zona")]
    [Tooltip("Desplaza la zona hacia delante o atrás en el eje Z local.")]
    [SerializeField] private float offsetZ = 0.05f;

    private bool gripReady;

    private enum PalmAxis
    {
        Up,
        Down,
        Forward,
        Back,
        Right,
        Left
    }

    private void Update()
    {
        if (leftHand == null ||
            rightHand == null ||
            gripZone == null)
        {
            SetGripReady(false);
            return;
        }

        // Comprobar distancia entre ambas manos.
        float distance = Vector3.Distance(
            leftHand.position,
            rightHand.position
        );

        bool handsClose = distance <= activationDistance;

        // Comprobar orientación de ambas palmas.
        bool palmsFacingEachOther = ArePalmsFacingEachOther();

        bool shouldActivate =
            handsClose && palmsFacingEachOther;

        // Actualizar la posición solamente cuando la zona sea válida.
        if (shouldActivate)
        {
            Vector3 center = (
                leftHand.position + rightHand.position
            ) * 0.5f;

            // Aplicar el offset sobre el eje Z local de la mano izquierda.
            Vector3 forward = leftHand.forward;

            gripZone.transform.position = center + forward * offsetZ;
        }

        SetGripReady(shouldActivate);
    }

    private bool ArePalmsFacingEachOther()
    {
        Vector3 directionBetweenHands =
            rightHand.position - leftHand.position;

        if (directionBetweenHands.sqrMagnitude < 0.0001f)
            return false;

        Vector3 leftToRight = directionBetweenHands.normalized;
        Vector3 rightToLeft = -leftToRight;

        Vector3 leftPalmDirection = GetPalmDirection(leftHand);
        Vector3 rightPalmDirection = GetPalmDirection(rightHand);

        float leftAngle = Vector3.Angle(
            leftPalmDirection,
            leftToRight
        );

        float rightAngle = Vector3.Angle(
            rightPalmDirection,
            rightToLeft
        );

        return leftAngle <= maxPalmAngle &&
               rightAngle <= maxPalmAngle;
    }

    private Vector3 GetPalmDirection(Transform hand)
    {
        Vector3 localDirection;

        switch (palmAxis)
        {
            case PalmAxis.Up:
                localDirection = Vector3.up;
                break;

            case PalmAxis.Down:
                localDirection = Vector3.down;
                break;

            case PalmAxis.Forward:
                localDirection = Vector3.forward;
                break;

            case PalmAxis.Back:
                localDirection = Vector3.back;
                break;

            case PalmAxis.Right:
                localDirection = Vector3.right;
                break;

            default:
                localDirection = Vector3.left;
                break;
        }

        return hand.TransformDirection(localDirection).normalized;
    }

    private void SetGripReady(bool ready)
    {
        gripReady = ready;

        if (gripZone != null)
            gripZone.SetGripReady(ready);
    }

    private void OnDisable()
    {
        gripReady = false;

        if (gripZone != null)
            gripZone.SetGripReady(false);
    }

}
