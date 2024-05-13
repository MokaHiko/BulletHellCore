using UnityEngine;


[CreateAssetMenu(fileName = "DashState", menuName = "UnitDashState")]
public class DashState : UnitAbilityState
{
    [SerializeField]
    TrailRenderer trail_renderer;

    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    DashState()
    {
        m_exclusive = false;
    }

    public override void OnEnter(Unit unit)
    {
        TimeElapsed = 0.0f;
        Use(IsBurst, Direction);
    }

    public override void Use(bool burst, Vector3 direction)
    {
        if (m_trail == null)
        {
            m_trail = Instantiate(trail_renderer, Owner.transform);
            Destroy(m_trail.gameObject, duration);
        }

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        rb.AddForce(direction * dash_multiplier, ForceMode.Impulse);
        Owner.movement_speed *= dash_multiplier;
    }

    public override void OnExit(Unit unit)
    {
        Owner.movement_speed = Owner.BaseStats.movement_speed;
    }

    // ~ Handles
    private TrailRenderer m_trail;
}

