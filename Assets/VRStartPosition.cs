using UnityEngine;

public class VRStartPosition : MonoBehaviour
{
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform centerEye;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        Invoke(nameof(SetStartPosition), 0.2f);
    }

    private void SetStartPosition()
    {
        // Corregir posición horizontal
        Vector3 eyeOffset = centerEye.position - cameraRig.position;

        eyeOffset.y = 0f;

        cameraRig.position = spawnPoint.position - eyeOffset;

        // Corregir orientación
        float currentYaw = centerEye.eulerAngles.y;
        float targetYaw = spawnPoint.eulerAngles.y;

        float yawDifference = targetYaw - currentYaw;

        cameraRig.RotateAround(
            centerEye.position,
            Vector3.up,
            yawDifference
        );
    }
}