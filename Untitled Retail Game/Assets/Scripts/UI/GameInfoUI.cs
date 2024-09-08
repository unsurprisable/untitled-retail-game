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
            balanceText.text = "$ " + ((GameManager)sender).GetBalance().ToString("000000.00");
        };
    }
}
