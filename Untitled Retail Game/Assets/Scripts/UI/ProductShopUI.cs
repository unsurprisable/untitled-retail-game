using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductShopUI : MonoBehaviour
{
    public static ProductShopUI Instance { get; private set; }

    public enum ProductCategory { BASIC_PRODUCTS, DAIRY_PRODUCTS, }

    [SerializeField] private StoreItemListSO itemList;
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject[] categoryButtons;
    [SerializeField] private Button placeOrderButton;
    [SerializeField] private Transform productButtonTemplate;
    [SerializeField] private Transform productButtonParent;

    private HashSet<GameObject> activeProductButtons;
    private Dictionary<ProductCategory, List<StoreItemSO>> categoryDict;

    private bool isEnabled;


    private void Awake()
    {
        Instance = this;

        activeProductButtons = new HashSet<GameObject>();
        categoryDict = new Dictionary<ProductCategory, List<StoreItemSO>>();
        foreach (StoreItemSO storeItemSO in itemList.list)
        {
            if (!categoryDict.ContainsKey(storeItemSO.category)) {
                categoryDict[storeItemSO.category] = new List<StoreItemSO>();
            }
            categoryDict[storeItemSO.category].Add(storeItemSO);
        }

        categoryButtons[0].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(ProductCategory.BASIC_PRODUCTS); });

        placeOrderButton.onClick.AddListener(()=>{
            PlaceOrder();
            Hide();
         });
    }

    private void Start()
    {
        GameInput.Instance.OnCloseMenu += (sender, args) => {
            if (isEnabled) Hide();
        };
    }

    private void PlaceOrder()
    {
        Debug.Log("placed order");
    }

    private void ShowCategory(ProductCategory category)
    {
        if (activeProductButtons.Count != 0) {
            foreach (GameObject productButton in activeProductButtons) {
                Destroy(productButton);
            }
            activeProductButtons.Clear();
        }

        if (!categoryDict.ContainsKey(category)) {
            Debug.LogWarning("That category does not contain any items!");
            return;
        }

        List<StoreItemSO> storeItems = categoryDict[category];
        foreach (StoreItemSO storeItemSO in storeItems) {
            activeProductButtons.Add(CreateProductButton(storeItemSO));
        }
    }

    private GameObject CreateProductButton(StoreItemSO storeItemSO)
    {
        Transform productButton = Instantiate(productButtonTemplate, productButtonParent);
        productButton.GetComponent<ProductButtonSingleUI>().SetStoreItemSO(storeItemSO);
        return productButton.gameObject;
    }

    public void AddItemToOrder(StoreItemSO storeItemSO)
    {
        Debug.Log("added " + storeItemSO.name + " to order.");
    }

    public void Show()
    {
        visual.SetActive(true);
        isEnabled = true;
        PlayerController.Instance.OnMenuOpened();
    }
    public void Hide()
    {
        visual.SetActive(false);
        isEnabled = false;
        PlayerController.Instance.OnMenuClosed();
    }
}
