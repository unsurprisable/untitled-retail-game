using Unity.Netcode;
using UnityEngine;

public abstract class NetworkMenu : NetworkBehaviour, IMenu
{
    [SerializeField] protected GameObject visual;
    protected bool isEnabled;

    protected virtual void OnShow(){}
    protected virtual void OnHide(){}

    public void Show() 
    {
        visual.SetActive(true);
        isEnabled = true;
        MenuManager.Instance.OnMenuOpen(this);
        OnShow();
    }
    public void Hide()
    {
        visual.SetActive(false);
        isEnabled = false;
        MenuManager.Instance.OnMenuClose();
        OnHide();
    }
}
