
using UnityEngine;

public class LauncherBallSlot : MonoBehaviour
{
    public GameObject CurrentBall { get; private set; }

    public bool IsOccupied => CurrentBall != null;

    public void SetBall(GameObject ball)
    {
        CurrentBall = ball;
    }

    public GameObject RemoveBall()
    {
        GameObject ball = CurrentBall;
        CurrentBall = null;

        return ball;
    }
}
