using System.Collections.Generic;
using UnityEngine;

public delegate void OnMenuOpen(Menu menu);
public delegate void OnMenuClose(Menu menu);

public class Menu : MonoBehaviour
{
    [SerializeField]
    public new string name;

    public OnMenuOpen menu_open_callback;
    public OnMenuClose menu_close_callback;

    public void Activate()
    {
        if (isActiveAndEnabled)
        {
            return;
        }

        OnActivate();
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        OnDeactivate();
        gameObject.SetActive(false);
    }

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }
}
