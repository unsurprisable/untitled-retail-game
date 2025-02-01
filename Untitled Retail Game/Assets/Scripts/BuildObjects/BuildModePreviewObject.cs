using UnityEngine;

public class BuildModePreviewObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public void SetMaterial(Material material)
    {
        meshRenderer.material = material;
    }
}
