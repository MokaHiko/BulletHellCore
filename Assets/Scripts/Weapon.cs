using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // ~ Weapon
    [SerializeField]
    public float base_damage;

    [SerializeField]
    public float attack_speed = 5f;

    [SerializeField]
    public float scan_radius = 1.0f;

    [SerializeField]
    public float range = 1000.0f;

    [SerializeField]
    public LayerMask damageable_layers;

    void Awake()
    {
        OnEquip();
    }

    public virtual void Attack() { }

    void OnEquip()
    {
        m_unit = GetComponentInParent<Unit>();
        m_player_controller = GetComponentInParent<PlayerController>();
    }

    // ~ Handles
    protected Unit m_unit;
    protected PlayerController m_player_controller;
}

