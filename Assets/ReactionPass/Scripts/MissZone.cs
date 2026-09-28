using UnityEngine;

public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ReactionPassBall ball =
            other.GetComponent<ReactionPassBall>();

        if (ball == null)
            return;

        ball.Missed();
    }
}