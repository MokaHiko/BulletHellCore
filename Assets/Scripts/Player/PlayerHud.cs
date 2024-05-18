using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    [Header("Unit Properties")]
    public PropertyBar health_bar;
    public PropertyBar energy_bar;
    public PropertyBar exp_bar;

    [Header("Weapons")]
    public RectTransform weapons_container;

    [Header("Rewards")]
    public GameObject rewards;

    [Header("Merc Coms")]
    public RectTransform power_up_container;
    public GameObject special_prefab;

    public void ShowMercSpecial(Merc merc, float duration = 2.0f)
    {
        StartCoroutine(MercSpecialEffect(merc, duration));
    }

    public void Reward()
    {
        // TODO: Generate Random Rewards
        rewards.SetActive(true);
    }

    IEnumerator MercSpecialEffect(Merc merc, float duration)
    {
        GameObject special_animation = Instantiate(special_prefab, power_up_container.transform);

        GameManager.Instance.IndefinedStop();
        yield return new WaitForSecondsRealtime(duration);  
        GameManager.Instance.ContinueTime();

        Destroy(special_animation);
    }
}
