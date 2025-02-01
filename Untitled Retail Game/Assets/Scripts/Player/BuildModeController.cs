using Unity.Netcode;
using UnityEngine;

public class BuildModeController : NetworkBehaviour
{

    private bool isActive;

    private BuildObjectSO buildObjectSO;
    private Transform buildObjectPreview;

    [SerializeField] private float buildDistance;
    [SerializeField] private float nudgeDistanceRange;
    [SerializeField] private float nudgeDistanceStep;
    private float nudgeDistance;
    [SerializeField] private int defaultNudgePercent;
    [SerializeField] private LayerMask buildCollisionLayerMask;
    [Space]
    [SerializeField] private Material canBuildMaterial;
    [SerializeField] private Material invalidBuildMaterial;




    public void Start()
    {
        GameInput.Instance.MainAction += (sender, args) => {
            if (buildObjectSO == null) return;

            Transform buildObject = Instantiate(buildObjectSO.prefab, buildObjectPreview.position, buildObjectPreview.rotation);
            buildObject.GetComponent<NetworkObject>().Spawn();
            buildObject.GetComponent<BuildObject>().Place();

            Deactivate();
        };

        // RMB to cancel build mode
        GameInput.Instance.SecondaryAction += (sender, args) => {
            Deactivate();
        };

        // Scroll to nudge build object
        GameInput.Instance.OnScroll += (sender, args) => {
            int scrollDirection = GameInput.Instance.GetScrollDirection();

            nudgeDistance = Mathf.Clamp(nudgeDistance + (scrollDirection * nudgeDistanceStep), 0, nudgeDistanceRange);
        };
    }

    private void LateUpdate()
    {
        if (!isActive) return;

        Vector3 previewLocation = PlayerController.LocalInstance.cameraAnchor.position;
        previewLocation += PlayerController.LocalInstance.orientation.forward * (buildDistance + nudgeDistance);

        buildObjectPreview.position = previewLocation;
    }


    public void SetBuildObject(BuildObjectSO buildObjectSO)
    {
        nudgeDistance = nudgeDistanceRange * defaultNudgePercent / 100;

        if (buildObjectPreview != null) {
            Destroy(buildObjectPreview.gameObject);
        }

        this.buildObjectSO = buildObjectSO;
        buildObjectPreview = Instantiate(buildObjectSO.buildModePrefab);
        buildObjectPreview.GetComponent<BuildModePreviewObject>().SetMaterial(canBuildMaterial);
    }


    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;

        buildObjectSO = null;
        if (buildObjectPreview != null) {
            Destroy(buildObjectPreview.gameObject);
        }
    }

    public bool IsActive()
    {
        return isActive;
    }
    
}
