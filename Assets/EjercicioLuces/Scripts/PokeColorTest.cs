using UnityEngine;

public class PokeColorTest : MonoBehaviour
{
    [SerializeField] private Renderer cubeRenderer;

    public void ChangeColor()
    {
        cubeRenderer.material.color = Color.green;
    }
}