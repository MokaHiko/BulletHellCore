using System;
using System.Collections;
using UnityEngine;

public class UnitAbilityState : UnitState
{
    [SerializeField]
    public float duration;

    [SerializeField]
    public float cooldown;

    [SerializeField]
    public float energy_cost = 15.0f;

    [SerializeField]
    private int stacks = 1;

    [SerializeField]
    private int max_stacks = 1;

    public AbilityCallback ability_begin_callback;
    public AbilityCallback ability_end_callback;

    public Vector3 Direction { get {return m_direction; }  set { m_direction = value; } }
    public bool IsBurst { get {return m_is_burst; }  set { m_is_burst = value; } }

    // Default skill tick
    public override void OnFrameTick(Unit unit, float dt)
    {
        TimeElapsed += dt;
        if (TimeElapsed >= duration)
        {
            StateMachine.QueueRemoveState(this);
        }
    }

    protected float TimeElapsed { get { return m_time_elapsed; } set { m_time_elapsed = value; } }
    IEnumerator IncrementStack()
    {
        yield return new WaitForSeconds(duration);
        stacks = Mathf.Clamp(stacks + 1, 0, max_stacks);

    }

    // Returns whether ability was used succesefully
    public bool UseWithCost(bool burst, Vector3 direction)
    {
        // Burst overrides cooldowns
        if (!burst)
        {
            if (stacks > 0)
            {
                stacks--;
            }
            else
            {
                return false;
            }
        }

        Owner.StartCoroutine(IncrementStack());
        if (StateMachine.Owner.SpendEnergy(energy_cost))
        {
            Use(burst, direction);
            return true;
        }

        return false;
    }
    public virtual void Use(bool burst = false, Vector3 direction = new Vector3()) { }

    // ~ Cooldowns
    protected float m_time_elapsed;

    private Vector3 m_direction;
    private bool m_is_burst = false;
}
