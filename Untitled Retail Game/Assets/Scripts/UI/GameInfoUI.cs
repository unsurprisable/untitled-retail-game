using UnityEngine;

public class GameInfoUI : MonoBehaviour
{
    public static GameInfoUI Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI balanceText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnBalanceChanged += (sender, args) => {
            UpdateVisual(((GameManager)sender).GetBalance());
        };
        
        UpdateVisual(GameManager.Instance.initialStoreBalance);
    }

    private void UpdateVisual(float newBalance) {
        balanceText.text = "$ " + newBalance.ToString("0,000,000.00");
    }
}
