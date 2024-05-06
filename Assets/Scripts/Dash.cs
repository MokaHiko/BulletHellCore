using System.Collections;
using UnityEngine;

public class Dash : Ability
{
    [SerializeField]
    TrailRenderer trail_renderer;

    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public float teleport_distance;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    public void Start()
    {
        // Handles
        Debug.Assert(trail_renderer != null);
    }
    public override void Use(bool burst, Vector3 direction)
    {
        if (m_dash_routine != null)
        {
            StopCoroutine(m_dash_routine);
            AbortDash();
        }

        m_dash_routine = StartCoroutine(DashEffect(burst, direction));
    }

    void AbortDash()
    {
        m_unit.RemoveStatus(dash_status);
        m_unit.RemoveState(UnitStateFlags.ManagedMovement);

        float start_speed = m_unit.BaseStats.movement_speed;
        float start_agility = m_unit.BaseStats.agility;
        m_unit.movement_speed = start_speed;
        m_unit.agility = start_agility;

        m_dash_routine = null;
    }

    private IEnumerator DashEffect(bool burst = false, Vector3 direction = new Vector3())
    {
        float start_speed = m_unit.BaseStats.movement_speed;

        if (m_trail == null)
        {
            m_trail = Instantiate(trail_renderer, transform);
            Destroy(m_trail.gameObject, duration);
        }

        if (burst)
        {
            m_unit.ApplyStatus(dash_status);

            // TP
            m_unit.transform.position += m_unit.transform.forward * teleport_distance;
            if (m_unit.TryGetComponent(out PlayerCombat combat))
            {
                ability_end_callback?.Invoke();
                // TODO: Make parry modifier
                //combat.Parry();
            }

            yield return new WaitForSeconds(duration);
            AbortDash();
        }
        else
        {
            //m_unit.ApplyState(UnitStateFlags.ManagedMovement);

            Rigidbody rb = m_unit.GetComponent<Rigidbody>();
            rb.AddForce(direction * dash_multiplier, ForceMode.Impulse);
            m_unit.movement_speed *= dash_multiplier;

            yield return new WaitForSeconds(duration);

            AbortDash();
        }
    }

    // ~ Handles
    private Coroutine m_dash_routine;
    private TrailRenderer m_trail;
}
