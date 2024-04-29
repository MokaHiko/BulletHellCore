using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Ability
{
    [SerializeField]
    TrailRenderer trail_renderer;

    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    public void Start()
    {
        // Handles
        Debug.Assert(trail_renderer != null);
    }
    public override void Use(bool burst) 
    {
        if (m_dash_routine != null)
        {
            StopCoroutine(m_dash_routine);
            m_dash_routine = null;
        }

        m_dash_routine = StartCoroutine(DashEffect(burst));
    }

    private IEnumerator DashEffect(bool burst = false)
    {
        float start_speed = m_unit.BaseStats().movement_speed;
        float start_agility = m_unit.BaseStats().agility;

        if (m_trail == null)
        {
            m_trail = Instantiate(trail_renderer, transform);
            Destroy(m_trail.gameObject, duration);
        }

        m_unit.movement_speed =  start_speed * dash_multiplier;
        m_unit.agility = start_agility * dash_multiplier;

        if (burst)
        {
            m_unit.ApplyStatus(dash_status);
        }

        yield return new WaitForSeconds(duration);

        m_unit.RemoveStatus(dash_status);
        m_unit.movement_speed = start_speed;
        m_unit.agility = start_agility;

        m_dash_routine = null;
    }

    // ~ Handles
    private Coroutine m_dash_routine;
    private TrailRenderer m_trail;
}
