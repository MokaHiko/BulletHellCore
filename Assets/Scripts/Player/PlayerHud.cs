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
    public List<RectTransform> special_transforms;
    public GameObject special_prefab;

    public void ShowMercSpecial(Merc merc, float duration = 0.5f)
    {
        if (m_free_index < special_transforms.Count)
        {
            StartCoroutine(MercSpecialEffect(merc, duration));
        }
    }

    public void Reward()
    {
        // TODO: Generate Random Rewards
        rewards.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(special_transforms.Count >= 4);
    }

    IEnumerator MercSpecialEffect(Merc merc, float duration)
    {
        GameObject special_animation = Instantiate(special_prefab, special_transforms[m_free_index++].transform);

        yield return new WaitForSeconds(duration);  

        Destroy(special_animation);
        m_free_index--;
    }


    // ~ Specials
    int m_free_index = 0;
}
