
public interface ILaunchableBall
{
    void Setup(
        PassReactionGameManager manager,
        BallLauncher launcher
    );

    void CancelAndReturnToPool();
}
