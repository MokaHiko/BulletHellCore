using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitStateMachine : MonoBehaviour
{
    [SerializeField] private UnitState idle_state;
    [SerializeField] private UnitState walk_state;
    [SerializeField] private UnitState attack_state;
    public UnitState IdleState { get { return m_idle_state_instance; } }
    public UnitState WalkState { get { return m_walk_state_instance; } }
    public UnitState AttackState { get { return m_attack_state_instance; } }

    public void QueueAddState(UnitState state)
    {
        if(m_activation_queue.Contains(state)) return;
        m_activation_queue.Add(state);
    }

    public void QueueRemoveState(UnitState state)
    {
        if(m_deactivation_queue.Contains(state)) return;
        m_deactivation_queue.Add(state);
    }

    private void ActivateState(UnitState state, bool deactivate_others = false)   
    {
        Debug.Assert(state != null, "Cannot activate null state");

        if(deactivate_others)
        {
            foreach(UnitState active_state in m_activate_states)
            {
                active_state.OnExit(m_unit);
            }
            m_activate_states.Clear();
        }

        if(m_activate_states.Contains(state))
        {
            return;
        }

        state.OnEnter(m_unit);
        m_activate_states.Add(state);
    }
    private void DeactivateState(UnitState state)
    {
        Debug.Assert(state != null, "Cannot deactivate null state");

        state.OnExit(m_unit);
        if(!m_activate_states.Remove(state))
        {
            Debug.Assert(false, "Cannot remove state that is not in active list!");
        }
    }

    void Start()
    {
        m_unit = GetComponent<Unit>();
        Debug.Assert(m_unit != null);

        m_activate_states = new List<UnitState>();
        m_activation_queue = new HashSet<UnitState>();
        m_deactivation_queue = new HashSet<UnitState>();

        m_walk_state_instance = Instantiate(walk_state);
        m_attack_state_instance = Instantiate(attack_state);

        ActivateState(m_walk_state_instance);
    }

    void Update()
    {
        if(m_activate_states.Count <= 0)
        {
            // TODO: Decide state
        }

        foreach(UnitState state in m_deactivation_queue)
        {
            DeactivateState(state);
        }
        m_deactivation_queue.Clear();

        foreach(UnitState state in m_activation_queue)
        {
            ActivateState(state);
        }
        m_activation_queue.Clear();

        foreach(UnitState state in m_activate_states)
        {
            state.OnFrameTick(m_unit, Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        foreach(UnitState state in m_activate_states)
        {
            state.OnPhysicsTick(m_unit, Time.deltaTime);
        }
    }

    Unit m_unit = null; 
    
    [SerializeField]
    List<UnitState> m_activate_states; 

    [SerializeField]
    HashSet<UnitState> m_deactivation_queue; 

    [SerializeField]
    HashSet<UnitState> m_activation_queue; 

    private UnitState m_idle_state_instance;
    private UnitState m_walk_state_instance;
    private UnitState m_attack_state_instance;
}
