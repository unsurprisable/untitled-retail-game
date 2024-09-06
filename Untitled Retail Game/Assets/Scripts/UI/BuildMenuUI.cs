using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUI : MonoBehaviour
{
    public static BuildMenuUI Instance { get; private set; }

    public enum BuildCategory { ITEM_DISPLAYS, CUSTOMERS, DECORATION, IMPORTANT_DO_NOT_ASSIGN }

    [SerializeField] private BuildObjectListSO objectList;
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject[] categoryButtons;
    [SerializeField] private Transform buildButtonTemplate;
    [SerializeField] private Transform buildButtonParent;

    private HashSet<GameObject> activeBuildButtons;
    private Dictionary<BuildCategory, List<BuildObjectSO>> categoryDict;

    private bool isEnabled = false;

    private void Awake()
    {
        Instance = this;

        activeBuildButtons = new HashSet<GameObject>();
        categoryDict = new Dictionary<BuildCategory, List<BuildObjectSO>>();
        foreach (BuildObjectSO buildObject in objectList.list)
        {
            if (buildObject.isImportant) {
                if (!categoryDict.ContainsKey(BuildCategory.IMPORTANT_DO_NOT_ASSIGN)) {
                    categoryDict[BuildCategory.IMPORTANT_DO_NOT_ASSIGN] = new List<BuildObjectSO>();
                }
                categoryDict[BuildCategory.IMPORTANT_DO_NOT_ASSIGN].Add(buildObject);
            }
            if (!categoryDict.ContainsKey(buildObject.category)) {
                categoryDict[buildObject.category] = new List<BuildObjectSO>();
            }
            categoryDict[buildObject.category].Add(buildObject);
        }

        // manual assignment for now (it's not too inconvenient, so this will probably stay)
        categoryButtons[0].GetComponent<Button>().onClick.AddListener(()=>{ShowCategory(BuildCategory.IMPORTANT_DO_NOT_ASSIGN);});
        categoryButtons[1].GetComponent<Button>().onClick.AddListener(()=>{ShowCategory(BuildCategory.ITEM_DISPLAYS);});
        categoryButtons[2].GetComponent<Button>().onClick.AddListener(()=>{ShowCategory(BuildCategory.CUSTOMERS);});
        categoryButtons[3].GetComponent<Button>().onClick.AddListener(()=>{ShowCategory(BuildCategory.DECORATION);});
    }

    private void Start()
    {
        GameInput.Instance.OnBuildMenu += (sender, args) => {
            if (isEnabled) Hide();
            else Show();
        };
    }


    private void ShowCategory(BuildCategory category) 
    {
        if (activeBuildButtons.Count != 0) {
            foreach (GameObject buildButton in activeBuildButtons) {
                Destroy(buildButton);
            }
            activeBuildButtons.Clear();
        }

        if (!categoryDict.ContainsKey(category)) {
            Debug.LogWarning("That category does not contain any items!");
            return;
        }

        List<BuildObjectSO> buildObjects = categoryDict[category];
        foreach (BuildObjectSO buildObjectSO in buildObjects) {
            activeBuildButtons.Add(CreateBuildButton(buildObjectSO));
        }
    }

    private GameObject CreateBuildButton(BuildObjectSO buildObjectSO)
    {
        Transform buildButton = Instantiate(buildButtonTemplate);
        buildButton.GetComponent<BuildButtonSingleUI>().SetBuildObjectSO(buildObjectSO);
        buildButton.SetParent(buildButtonParent);
        return buildButton.gameObject;
    }

    public void Show()
    {
        visual.SetActive(true);
        isEnabled = true;
        PlayerController.Instance.GetFirstPersonCamera().Disable();
    }
    public void Hide()
    {
        visual.SetActive(false);
        isEnabled = false;
        PlayerController.Instance.GetFirstPersonCamera().Enable();
    }
}
