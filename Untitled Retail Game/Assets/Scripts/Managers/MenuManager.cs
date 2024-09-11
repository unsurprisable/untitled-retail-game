using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private bool inMenu;
    [SerializeField] private Stack<SubMenu> openSubMenus;
    [SerializeField] private Menu activeMenu;

    private void Awake()
    {
        Instance = this;

        openSubMenus = new Stack<SubMenu>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseMenu += (sender, args) => {
            if (openSubMenus.Count > 0) {
                openSubMenus.Peek().Hide();
            } else if (inMenu) {
                activeMenu.Hide();
            } else {
                PauseMenuUI.Instance.Show();
            }
        };
    }

    public void OnSubMenuOpen(SubMenu subMenu)
    {
        openSubMenus.Push(subMenu);
    }

    public void OnSubMenuClose()
    {
        openSubMenus.Pop();
    }

    public void OnMenuOpen(Menu menu) {
        inMenu = true;
        activeMenu = menu;
        PlayerController.Instance.DisableControls();
    }
    public void OnMenuClose() {
        inMenu = false;
        activeMenu = null;
        PlayerController.Instance.EnableControls();
    }
    public bool IsInMenu() {
        return inMenu;
    }

}
