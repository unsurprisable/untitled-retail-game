using UnityEngine;
using UnityEngine.UI;

public class BuildButtonSingleUI : MonoBehaviour
{
    [SerializeField] private Image preview;
    [SerializeField] private TMPro.TextMeshProUGUI label;
    [SerializeField] private TMPro.TextMeshProUGUI price;
    [SerializeField] private Button button;

    private BuildObjectSO buildObjectSO;

    public void SetBuildObjectSO(BuildObjectSO buildObjectSO)
    {
        this.buildObjectSO = buildObjectSO;
        preview.sprite = buildObjectSO.preview;
        label.text = buildObjectSO.name;
        price.text = "$" + buildObjectSO.price;
        button.onClick.AddListener(() => {
            Debug.Log("enter build mode overlay");
            BuildMenuUI.Instance.Hide();
        });
    }
}
