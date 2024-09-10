using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductShopUI : MonoBehaviour
{
    public static ProductShopUI Instance { get; private set; }

    public enum ProductCategory { BASIC_PRODUCTS, DAIRY_PRODUCTS, }

    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject[] categoryButtons;
    [Space]
    [SerializeField] private Transform productButtonTemplate;
    [SerializeField] private Transform productButtonParent;
    [SerializeField] private Transform productOrderTemplate;
    [SerializeField] private Transform productOrderParent;
    [SerializeField] private int maxOrders;
    private float orderTotalPrice;

    [Space]
    [SerializeField] private Button placeOrderButton;
    [SerializeField] private TMPro.TextMeshProUGUI orderTotalText;
    [SerializeField] private Color orderButtonEnabledColor;
    [SerializeField] private Color orderButtonDisabledColor;

    private Dictionary<StoreItemSO, List<GameObject>> currentOrder;
    private int queuedOrderCount;

    private HashSet<GameObject> activeProductButtons;
    private Dictionary<ProductCategory, List<StoreItemSO>> categoryDict;
    
    [Space]

    [SerializeField] private Transform containerPrefab;
    [SerializeField] private Transform containerSpawnLocation;

    private bool isEnabled;


    private void Awake()
    {
        Instance = this;

        currentOrder = new Dictionary<StoreItemSO, List<GameObject>>();
        activeProductButtons = new HashSet<GameObject>();
        categoryDict = new Dictionary<ProductCategory, List<StoreItemSO>>();
        foreach (StoreItemSO storeItemSO in GameManager.Instance.GetStoreItemSOList())
        {
            if (!categoryDict.ContainsKey(storeItemSO.category)) {
                categoryDict[storeItemSO.category] = new List<StoreItemSO>();
            }
            categoryDict[storeItemSO.category].Add(storeItemSO);
        }

        categoryButtons[0].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(ProductCategory.BASIC_PRODUCTS); });
        categoryButtons[1].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(ProductCategory.DAIRY_PRODUCTS); });

        placeOrderButton.onClick.AddListener(PlaceOrder);
    }

    private void Start()
    {
        GameInput.Instance.OnCloseMenu += (sender, args) => {
            if (isEnabled) Hide();
        };
        GameManager.Instance.OnBalanceChanged += (sender, args) => {
            UpdateOrderButtonState();
        };
        UpdateOrderButtonState();
    }

    private void PlaceOrder()
    {
        if (!GameManager.Instance.CanAfford(orderTotalPrice)) return;

        GameManager.Instance.RemoveFromBalance(orderTotalPrice);

        foreach (StoreItemSO storeItemSO in currentOrder.Keys)
        {
            List<GameObject> activeOrderButtons = currentOrder[storeItemSO];
            // account for ordering multiple of the same item
            foreach (GameObject orderButton in activeOrderButtons) {
                Transform container = Instantiate(containerPrefab, containerSpawnLocation.position, Quaternion.identity);
                container.GetComponent<Container>().SetStoreItemSO(storeItemSO);
                Destroy(orderButton);
            }
        }
        currentOrder.Clear();
        queuedOrderCount = 0;
        orderTotalPrice = 0f;
        UpdateOrderTotalText();
        UpdateOrderButtonState();
        
        Hide();
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
        if (queuedOrderCount == maxOrders) {
            Debug.Log("Cannot add another order (order list is full)!");
            return;
        }
        if (!currentOrder.ContainsKey(storeItemSO)) {
            currentOrder.Add(storeItemSO, new List<GameObject>());
        } 
        currentOrder[storeItemSO].Add(CreateOrderButton(storeItemSO));
        orderTotalPrice += storeItemSO.unitPrice * storeItemSO.containerAmount;
        UpdateOrderTotalText();
        queuedOrderCount++;
        UpdateOrderButtonState();
    }

    private GameObject CreateOrderButton(StoreItemSO storeItemSO)
    {
        Transform orderButton = Instantiate(productOrderTemplate, productOrderParent);
        orderButton.GetComponent<ProductOrderSingleUI>().SetStoreItemSO(storeItemSO);
        return orderButton.gameObject;
    }

    public void RemoveItemFromOrder(ProductOrderSingleUI order)
    {
        Destroy(order.gameObject);
        currentOrder[order.storeItemSO].Remove(order.gameObject);
        orderTotalPrice -= order.storeItemSO.unitPrice * order.storeItemSO.containerAmount;
        UpdateOrderTotalText();
        queuedOrderCount--;
        UpdateOrderButtonState();
    }

    private void UpdateOrderTotalText()
    {
        orderTotalText.text = "Order Total: $" + orderTotalPrice.ToString("0.00");
    }

    private void UpdateOrderButtonState()
    {
        bool canOrder = queuedOrderCount != 0 && GameManager.Instance.CanAfford(orderTotalPrice);
        placeOrderButton.gameObject.GetComponent<Image>().color = canOrder ? orderButtonEnabledColor : orderButtonDisabledColor;
        placeOrderButton.enabled = canOrder;
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
