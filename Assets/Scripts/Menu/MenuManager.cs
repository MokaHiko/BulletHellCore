using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    public List<Menu> menus;
    void ActivateMenu(string menu_name)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name != menu_name)
            {
                menu.Deactivate();
            }

            menu.Activate();
        }
    }

    void DeactivateMenu(string menu_name)
    {
        foreach (Menu menu in menus) 
        { 
            if (menu.name == menu_name)
            {
                menu.Deactivate();
                return;
            }
        }
    }
}
