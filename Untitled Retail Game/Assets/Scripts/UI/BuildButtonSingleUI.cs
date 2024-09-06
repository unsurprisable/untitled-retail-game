using UnityEngine;
using UnityEngine.UI;

public class BuildButtonSingleUI : MonoBehaviour
{
    [SerializeField] private Image preview;
    [SerializeField] private Button button;
    private BuildObjectSO buildObjectSO;

    public void SetBuildObjectSO(BuildObjectSO buildObjectSO)
    {
        this.buildObjectSO = buildObjectSO;
        preview.sprite = buildObjectSO.preview;
        button.onClick.AddListener(() => { Debug.Log(buildObjectSO.name); });
    }
}
