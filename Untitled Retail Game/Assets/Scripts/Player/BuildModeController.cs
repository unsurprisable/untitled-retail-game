using UnityEngine;

public class BuildModeController : MonoBehaviour
{

    private bool isActive;

    [SerializeField] private BuildObjectSO buildObjectSO;
    [SerializeField] private float buildDistance;
    [SerializeField] private LayerMask buildCollisionLayerMask;
    [Space]
    [SerializeField] private Material canBuildMaterial;
    [SerializeField] private Material invalidBuildMaterial;




    public void Activate()
    {
        isActive = true;
    }
    public void Deactivate()
    {
        isActive = false;
    }
    public bool IsActive()
    {
        return isActive;
    }
}
