using UnityEngine;

[CreateAssetMenu()]
public class BuildObjectSO : ScriptableObject
{
    [Tooltip("The name of this object.")]
    public new string name;
    [Tooltip("The cost of building this object.")]
    public int price;
    [Tooltip("The prefab of the object itself.")]
    public Transform prefab;
    [Tooltip("Preview of the object, shown in the build menu.")]
    public Sprite preview;
    [Tooltip("The category that this object will be displayed in.")]
    public BuildMenuUI.BuildCategory category;
    [Tooltip("Add this object to the \"Important Objects\" category.")]
    public bool isImportant;
}
