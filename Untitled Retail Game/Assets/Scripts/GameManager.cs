using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnBalanceChanged;

    [SerializeField] private float storeBalance;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetBalance() {
        return storeBalance;
    }

    public void SetBalance(float newBalance) {
        storeBalance = newBalance;
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveFromBalance(float cost) {
        storeBalance -= cost;
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanAfford(float price) {
        return storeBalance - price >= 0f;
    }
}
