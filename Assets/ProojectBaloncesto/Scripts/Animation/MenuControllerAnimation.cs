using UnityEngine;

public class MenuControllerAnimation : MonoBehaviour
{

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimationtTrue(string animationName) {

        animator.SetBool(animationName, true);
    }

    public void ChangeAnimationtFalse(string animationName) {
        animator.SetBool(animationName, false);
    }
}
