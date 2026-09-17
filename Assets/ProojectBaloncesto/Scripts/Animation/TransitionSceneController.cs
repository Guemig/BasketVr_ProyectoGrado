using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionSceneController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private float transitionDuration = 1f;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public IEnumerator LoadSceneAfterTransition(string sceneName)
    {
        animator.SetTrigger("Transition");

        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(sceneName);
    }

    public void PlayTransition()
    {
        animator.SetTrigger("Transition");
    }
}