using System.Collections.Generic;
using UnityEngine;

public class MenuManager : Menu
{
    [SerializeField]
    public List<Menu> menus;

    public Menu start_menu;

    public void CloseAll()
    {
        foreach(Menu menu in menus) 
        {
            DeactivateMenu(menu);
        }

        DeactivateMenu(this);
    }

    public void OnDisable()
    {
        CloseAll();
    }

    public void OnEnable()
    {
        // Deactivate all menus
        foreach (Menu menu in menus)
        {
            DeactivateMenu(menu);
        }

        // Default menu
        if (start_menu != null) 
        {
            ActivateMenu(start_menu);
        }
    }


    private void Awake()
    {
        foreach (Menu menu in menus)
        {
            menu.menu_open_callback += OnMenuOpen;
            menu.menu_close_callback += OnMenuClose;
        }
    }

    public virtual void OnMenuOpen(Menu menu)
    {
    }

    public virtual void OnMenuClose(Menu menu)
    {
    }

    public Menu ActivateMenu(Menu menu, bool deactivate_others = true)
    {
        menu.Activate();

        if (!deactivate_others)
        {
            return menu;
        }

        foreach (Menu other in menus) 
        { 
            if (other.name != menu.name)
            {
                other.Deactivate();
            }
        }

        return menu;
    }

    public Menu ActivateMenu(string menu_name, bool deactivate_others = true)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name != menu_name && deactivate_others)
            {
                menu.Deactivate();
                continue;
            }

            return ActivateMenu(menu);
        }

        return null;
    }

    public void ToggleMenu(string menu_name, bool deactivate_others = true)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name != menu_name)
            {
                continue;
            }

            if (!menu.isActiveAndEnabled)
            {
                ActivateMenu(menu, deactivate_others);
            }
            else
            {
                DeactivateMenu(menu);
            }
        }
    }

    public Menu FindMenu(string menu_name)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name != menu_name)
            {
                continue;
            }
            return menu;
        }

        return null;
    }

    public void DeactivateMenu(Menu menu)
    {
        menu.Deactivate();
    }

    public void DeactivateMenu(string menu_name)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name == menu_name)
            {
                DeactivateMenu(menu);
                return;
            }
        }
    }
}
