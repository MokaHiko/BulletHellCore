using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitController))]
public class Ability : MonoBehaviour
{
    [SerializeField]
    public float duration;

    [SerializeField]
    public float cooldown;

    [SerializeField]
    public float energy_cost = 15.0f;

    void Awake()
    {
        m_unit = GetComponent<Unit>(); 
        m_unit_controller = GetComponent<UnitController>(); 
    }

    // Returns whether ability was used succesefully
    public bool UseWithCost(bool burst)
    {
        // Burst overrides cooldowns
        if (!burst)
        {
            if (time_elapsed < cooldown)
            {
                return false;
            }
            time_elapsed = 0;
        }

        if (m_unit.SpendEnergy(energy_cost))
        {
            Use(burst);
            return true;
        }

        return false;
    }

    public void Update()
    {
        time_elapsed += Time.deltaTime;
    }

    public virtual void Use(bool burst) { }

    // ~ Cooldowns
    protected float time_elapsed;

    // ~ Handles
    protected Unit m_unit;
    protected UnitController m_unit_controller;

}
