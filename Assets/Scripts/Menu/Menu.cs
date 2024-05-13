using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField]
    public new string name;

    [SerializeField]
    public List<Menu> sub_menu;

    public void Activate()
    {
        if (isActiveAndEnabled)
        {
            Debug.Log("Menu already active!");
            return;
        }

        OnActivate();
        gameObject.SetActive(true);

        foreach(Menu menu in sub_menu)
        {
            menu.Activate();
        }
    }

    public void Deactivate()
    {
        if (!isActiveAndEnabled)
        {
            Debug.Log("Menu already deactive!");
            return;
        }

        OnDeactivate();
        gameObject.SetActive(false);

        foreach(Menu menu in sub_menu)
        {
            menu.Deactivate();
        }
    }

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }
}
