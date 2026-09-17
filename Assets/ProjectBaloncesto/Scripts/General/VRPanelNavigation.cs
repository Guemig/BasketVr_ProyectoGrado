using UnityEngine;
using System.Collections;

public class VRPanelNavigation : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private float animationDuration = 0.5f;

    private int currentPanel = 0;
    private Coroutine movementCoroutine;

    public void SelectPanel(int index)
    {
        if (panels.Length == 0)
            return;

        if (index < 0 || index >= panels.Length)
            return;

        if (index == currentPanel)
            return;

        currentPanel = index;
        MoveToPanel();
    }

    public void NextPanel()
    {
        if (panels.Length == 0)
            return;

        int nextIndex = (currentPanel + 1) % panels.Length;

        SelectPanel(nextIndex);
    }

    public void PreviousPanel()
    {
        if (panels.Length == 0)
            return;

        int previousIndex = (currentPanel - 1 + panels.Length) % panels.Length;

        SelectPanel(previousIndex);
    }

    private void MoveToPanel()
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MoveCharacter());
    }

    private IEnumerator MoveCharacter()
    {
        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            panels[currentPanel].transform.position.x,
            startPosition.y,
            startPosition.z
        );

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        transform.position = targetPosition;

        movementCoroutine = null;
    }
}