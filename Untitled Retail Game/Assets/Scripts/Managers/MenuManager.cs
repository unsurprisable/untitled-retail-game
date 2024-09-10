using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private bool inMenu;
    [SerializeField] private Menu activeMenu;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseMenu += (sender, args) => {
            if (inMenu) activeMenu.Hide();
            else PauseMenuUI.Instance.Show();
        };
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
