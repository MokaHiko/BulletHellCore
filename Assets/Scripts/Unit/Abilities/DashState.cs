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

    public override void OnFrameTick(Unit unit, float dt)
    {
        TimeElapsed += dt;
        if (TimeElapsed >= duration)
        {
            StateMachine.QueueRemoveState(this);
        }
    }

    //void AbortDash()
    //{
    //    Owner.RemoveStatus(dash_status);
    //    Owner.RemoveState(UnitStateFlags.ManagedMovement);

    //    float start_speed = Owner.BaseStats.movement_speed;
    //    float start_agility = Owner.BaseStats.agility;
    //    Owner.movement_speed = start_speed;
    //    Owner.agility = start_agility;

    //    m_dash_routine = null;

    //    // TODO: Move to ability state parent class
    //    StateMachine.QueueRemoveState(this);
    //}

    //private IEnumerator DashEffect(bool burst = false, Vector3 direction = new Vector3())
    //{
    //    float start_speed = Owner.BaseStats.movement_speed;

    //    if (m_trail == null)
    //    {
    //        m_trail = Instantiate(trail_renderer, Owner.transform);
    //        Destroy(m_trail.gameObject, duration);
    //    }

    //    if (true)
    //    {
    //        Owner.ApplyStatus(dash_status);

    //        // TP
    //        Owner.transform.position += Owner.transform.forward * teleport_distance;

    //        // TODO: Transition To Parry state if space

    //        yield return new WaitForSeconds(duration);
    //        AbortDash();
    //    }
    //    else
    //    {
    //        Rigidbody rb = Owner.GetComponent<Rigidbody>();
    //        rb.AddForce(direction * dash_multiplier, ForceMode.Impulse);
    //        Owner.movement_speed *= dash_multiplier;

    //        yield return new WaitForSeconds(duration);

    //        AbortDash();
    //    }
    //}

    //// ~ Handles
    //private Coroutine m_dash_routine;
    private TrailRenderer m_trail;
}

