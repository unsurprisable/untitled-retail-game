using Unity.Netcode;
using UnityEngine;

public class BuildModeController : NetworkBehaviour
{

    private bool isActive;

    private BuildObjectSO buildObjectSO;
    private BuildModePreviewObject buildObjectPreview;

    [Header("Positioning")]
    [SerializeField] private float buildDistance;
    // [SerializeField] private float nudgeDistanceRange;
    // [SerializeField] private float nudgeDistanceStep;
    // [SerializeField] private int defaultNudgePercent;
    // [SerializeField] private float nudgeSprintBoost;
    // [SerializeField] private float nudgeDistance;

    [Header("Rotating")]
    [SerializeField] private float defaultRotationOffset;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationSprintBoost;
    private float rotationOffset;
    
    [Header("Technical")]
    [SerializeField] private LayerMask buildCollisionLayerMask;
    [SerializeField] private LayerMask buildSurfaceLayerMask;
    [SerializeField] private Material canBuildMaterial;
    [SerializeField] private Material invalidBuildMaterial;
    private bool isSurfaced;




    public void Start()
    {
        // LMB to ask the server to build the object
        GameInput.Instance.MainAction += (sender, args) => {
            if (!isActive) return;
            if (buildObjectSO == null) return;

            TryBuildServerRpc(buildObjectSO.Id, buildObjectPreview.transform.position, buildObjectPreview.transform.rotation);

            Deactivate();
        };

        // RMB to cancel build mode
        GameInput.Instance.SecondaryAction += (sender, args) => {
            if (!isActive) return;

            Deactivate();
        };

        // Scroll to nudge build object
        // GameInput.Instance.OnScroll += (sender, args) => {
        //     if (!isActive) return;

        //     int scrollDirection = GameInput.Instance.GetScrollDirection();

        //     float nudgeAmount = nudgeDistanceStep * scrollDirection;
        //     if (GameInput.Instance.GetIsSprinting()) nudgeAmount *= nudgeSprintBoost;
        //     nudgeDistance = Mathf.Clamp(nudgeDistance + nudgeAmount, 0, nudgeDistanceRange);
        // };
    }

    [Rpc(SendTo.Server)]
    private void TryBuildServerRpc(int buildObjectID, Vector3 pos, Quaternion rot)
    {
        BuildObjectSO buildObjectSO = BuildObjectSO.FromId(buildObjectID);

        if (CanBuild()) {
            GameManager.Instance.RemoveFromBalance(buildObjectSO.price);
            NetworkObject buildObject = NetworkManager.SpawnManager.InstantiateAndSpawn(buildObjectSO.prefab.GetComponent<NetworkObject>(), position: pos, rotation: rot);

            BuildClientRpc(buildObject.GetComponent<BuildObject>());
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BuildClientRpc(NetworkBehaviourReference buildObjectReference)
    {
        if (buildObjectReference.TryGet(out BuildObject buildObject)) {
            buildObject.Place();
        }
    }

    private void LateUpdate()
    {
        if (!isActive) return;

        if (CanBuild()) {
            buildObjectPreview.GetComponent<BuildModePreviewObject>().SetMaterial(canBuildMaterial);
        } else {
            buildObjectPreview.GetComponent<BuildModePreviewObject>().SetMaterial(invalidBuildMaterial);
        }

        Vector3 camPos = PlayerController.LocalInstance.cameraAnchor.position;
        float distance = buildDistance/* + nudgeDistance*/;

        Vector3 previewLoc;
        if (Physics.Raycast(camPos, PlayerController.LocalInstance.orientation.forward, out RaycastHit hit, distance, buildSurfaceLayerMask)) {
            // TODO: confirm that this mesh's normal is pointing up before setting isSurfaced (otherwise you can place stuff on walls)
            isSurfaced = true;
            previewLoc = hit.point;
        } else {
            isSurfaced = false;
            previewLoc = camPos + PlayerController.LocalInstance.orientation.forward * distance;
        }

        int rotateDirection = GameInput.Instance.GetRotateDirection();
        if (rotateDirection != 0) {
            float rotateAmount = rotationSpeed * rotateDirection * Time.deltaTime;
            if (GameInput.Instance.GetIsSprinting()) rotateAmount *= rotationSprintBoost;
            rotationOffset += rotateAmount;
        }

        buildObjectPreview.transform.SetPositionAndRotation(previewLoc, Quaternion.Euler(0f, PlayerController.LocalInstance.orientation.rotation.eulerAngles.y + rotationOffset, 0f));
    }


    public void SetBuildObject(BuildObjectSO buildObjectSO)
    {
        // nudgeDistance = nudgeDistanceRange * defaultNudgePercent / 100;
        rotationOffset = defaultRotationOffset;

        if (buildObjectPreview != null) {
            Destroy(buildObjectPreview.gameObject);
        }

        this.buildObjectSO = buildObjectSO;
        buildObjectPreview = Instantiate(buildObjectSO.buildModePrefab).GetComponent<BuildModePreviewObject>();
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

    private bool CanBuild() {
        if (buildObjectSO == null) return false;
        if (!GameManager.Instance.CanAfford(buildObjectSO.price)) return false;
        if (!isSurfaced) return false;

        Collider[] overlappingBuildBounds = Physics.OverlapBox(buildObjectPreview.buildBounds.position, buildObjectPreview.buildBounds.localScale / 2, buildObjectPreview.buildBounds.rotation, buildCollisionLayerMask);
        foreach (Collider collider in overlappingBuildBounds) {
            if (!collider.transform.Equals(buildObjectPreview.buildBounds.transform)) return false;
        }

        return true;
    }

}
