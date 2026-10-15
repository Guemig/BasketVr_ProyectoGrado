using UnityEngine;

public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DoubleHandPassBall ball =
            other.GetComponent<DoubleHandPassBall>();

        if (ball == null)
            return;

        ball.Missed();
    }
}