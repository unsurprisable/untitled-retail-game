using UnityEngine;

[CreateAssetMenu()]
public class BuildObjectListSO : ScriptableObject
{
    [Tooltip("Place all BuildObjectSO's into this list to register them (order doesn't matter).")]
    public BuildObjectSO[] list;
}
