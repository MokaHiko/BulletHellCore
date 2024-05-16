using System.Collections.Generic;
using UnityEngine;

public class GuildShop : MonoBehaviour
{
    [SerializeField]
    List<ShopItem> items;

    private void Start()
    {
        foreach (ShopItem item in items) 
        {
            item.item_chosen_callback += () =>
            {
                Destroy(gameObject);
            };
        }
    }
}
