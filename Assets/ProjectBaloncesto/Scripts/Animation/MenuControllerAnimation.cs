using UnityEngine;

public class MenuControllerAnimation : MonoBehaviour
{

    private Animator animator;

    [SerializeField] private GameObject MenuPrincipal;
    [SerializeField] private GameObject MenuOption;
    [SerializeField] private GameObject MenuCredits;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimationtTrue(string animationName)
    {

        animator.SetBool(animationName, true);
    }

    public void ChangeAnimationtFalse(string animationName)
    {
        animator.SetBool(animationName, false);
    }

    public void MenuPrincipalActive()
    {
        MenuPrincipal.SetActive(true);

    }

    public void MenuOptionActive()
    {
        MenuOption.SetActive(true);
    }

    public void MenuCreditsActive()
    {

        MenuCredits.SetActive(true);
    }

    public void MenuOptionCreditsDissable()
    {

        MenuOption.SetActive(false);
        MenuCredits.SetActive(false);

    }
    public void MenuPrincipalCreditsDissable()
    {
        MenuPrincipal.SetActive(false);
        MenuCredits.SetActive(false);
    }

    public void MenuPrincipalOptionDissable()
    {
        MenuPrincipal.SetActive(false);
        MenuOption.SetActive(false);
    }
}
