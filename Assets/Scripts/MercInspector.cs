using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MercInspector : Menu
{
    [Header("Layout")]
    [SerializeField]
    public CinemachineVirtualCamera virtual_camera;

    [Header("Merc Stats")]
    [SerializeField]
    public TMP_Text health;
    public TMP_Text health_plus;
    [SerializeField]
    public TMP_Text defense;
    public TMP_Text defense_plus;
    [SerializeField]
    public TMP_Text movement_speed;
    public TMP_Text movement_speed_plus;
    [SerializeField]
    public TMP_Text energy;
    public TMP_Text energy_plus;

    [Header("Weapon Stats")]
    public TMP_Text base_damage;
    public TMP_Text base_damage_plus;
    public TMP_Text attack_speed;
    public TMP_Text attack_speed_plus;
    public TMP_Text ammo;
    public TMP_Text ammo_plus;

    [Header("Modifiers")]
    List<RectTransform> modifier_icons = new List<RectTransform>();
    public Transform modifiers_container;

    public void SetReward()
    {
        health_plus.text = $"{Random.Range(0, 1)}";
        defense_plus.text = $"{Random.Range(0, 1)}";
        movement_speed_plus.text = $"{Random.Range(0, 1)}";
        energy_plus.text = $"{Random.Range(0, 1)}";
    }

    public void SetMerc(Merc merc, Transform merc_view_transform)
    {
        m_merc = merc;
        m_merc_view_transform = merc_view_transform;

        // Update unit stats
        Unit unit = m_merc.GetComponent<Unit>();
        health.text = $"{(int)unit.Health}/{(int)unit.BaseStats.health}";
        defense.text = $"{(int)(1.0f/unit.damage_multiplier * 100.0f)}%";
        energy.text = $"{(int)(1.0f/unit.energy_gain_rate* 100.0f)}%";
        movement_speed.text = $"{(int)unit.movement_speed}m/s";

        Weapon weapon = unit.EquipedWeapon;
        if (weapon != null)
        {
            base_damage.text = $"{(int)weapon.Stats.base_damage}";
            attack_speed.text = $"{(int)weapon.Stats.attack_speed}";
            ammo.text = $"{(int)weapon.Stats.max_bullets}";
        }

        modifier_icons.Clear();
        float base_damage_percent = 1.0f;
        float base_attack_speed_percent = 1.0f;
        foreach(ModifierAttributes modifier in weapon.modifiers)
        {
            modifier_icons.Add(Instantiate(modifier.modifier_icon, modifiers_container));
            base_damage_percent *= modifier.stat_multipliers.base_damage;
            base_attack_speed_percent *= modifier.stat_multipliers.attack_speed;
        }

        base_damage_plus.text = $"{base_damage_percent * 100.0f}%";
        attack_speed_plus.text = $"{base_attack_speed_percent * 100.0f}%";

        if (m_reward)
        {
            health_plus.gameObject.SetActive(true);
            defense_plus.gameObject.SetActive(true);
            movement_speed_plus.gameObject.SetActive(true);
            energy_plus.gameObject.SetActive(true);

            m_reward = false;
        }
    }
    protected override void OnDeactivate()
    {
        // Move back to defaul tlocation
        m_merc_view_transform.position += Vector3.one * 15.0f;

        health_plus.gameObject.SetActive(false);
        defense_plus.gameObject.SetActive(false);
        movement_speed_plus.gameObject.SetActive(false);
        energy_plus.gameObject.SetActive(false);
    }

    protected override void OnActivate()
    {
        Debug.Assert(m_merc != null, "Cannot activate merc insepector without merc");

        // Move to inspect location
        m_merc_view_transform.position -= Vector3.right * 15.0f;

        // Open ShopItem Inspector Menu
        virtual_camera.Follow = m_merc_view_transform;
        virtual_camera.LookAt = m_merc_view_transform;

        // TODO: Start size
        virtual_camera.m_Lens.OrthographicSize = 1.5f;
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(1.8f, 1.5f, -10.0f);
    }

    private Merc m_merc;
    private Transform m_merc_view_transform;
    private bool m_reward = false;
}
