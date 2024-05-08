using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitStateMachine : MonoBehaviour
{
    [SerializeField] private UnitState idle_state;
    [SerializeField] private UnitState walk_state;
    [SerializeField] private UnitState attack_state;
    [SerializeField] private UnitState movement_ability_state;
    [SerializeField] private UnitState empowered_movement_ability_state;
    public UnitState IdleState { get { return m_idle_state_instance; } }
    public UnitState WalkState { get { return m_walk_state_instance; } }
    public UnitState AttackState { get { return m_attack_state_instance; } }
    public UnitState MovementAbilityState { get { return m_movement_ability_state_instance; } }
    public UnitState EmpoweredMovementAbilityState { get { return m_empowered_movement_ability_state_instance; } }

    // ~ Getters
    public Unit Owner { get { return m_unit; } }

    public void QueueAddState(UnitState state)
    {
        if (m_activation_queue.Contains(state)) return;
        m_activation_queue.Add(state);
    }

    public void QueueRemoveState(UnitState state)
    {
        if (m_deactivation_queue.Contains(state)) return;
        m_deactivation_queue.Add(state);
    }

    // Activates a state and its substates
    private void AddState(UnitState state, bool deactivate_others = false)
    {
        Debug.Assert(state != null, "Cannot activate null state!");

        if (m_active_states.Contains(state))
        {
            return;
        }

        // Check if top level state
        if (state.substate == null && state.superstate == null)
        {
            m_top_level_states.Add(state);
        }
        state.OnEnter(m_unit);
        m_active_states.Add(state);

        // Activate all substates
        UnitState substate = state.substate;
        while(substate != null)
        {
            substate.OnEnter(m_unit);
            m_active_states.Add(substate);
            substate = substate.substate;
        }

        if (deactivate_others || state.Exclusive)
        {
            List<UnitState> to_deactivate = new List<UnitState>();

            foreach (UnitState active_state in m_active_states)
            {
                if (active_state == state) continue;
                active_state.OnExit(m_unit);
                QueueRemoveState(active_state);
                //to_deactivate.Add(active_state);
            }

            //foreach (UnitState active_state in m_active_states)
            //{
            //    m_active_states.Remove(active_state);
            //}
        }
    }

    // Removes a state and its substates
    private void RemoveState(UnitState state)
    {
        Debug.Assert(state != null, "Cannot deactivate null state");

		// Remove state
        state.OnExit(m_unit);
        if (!m_active_states.Remove(state))
        {
            Debug.Assert(false, "Cannot remove state that is not in active list!");
        }

        // Check if top level state
        if (state.substate == null && state.superstate == null)
        {
            m_top_level_states.Remove(state);
        }

		// Remove from parent
        if(state.superstate != null)
        {
            state.superstate.substate = null;
        }

        // Remove all substates
        UnitState substate = state.substate;
        while(substate != null)
        {
            substate.OnExit(m_unit);
            if(!m_active_states.Remove(substate))
            {
                Debug.Assert(false, "Cannot remove state that is not active!");
            }
            substate = substate.substate;
        }
    }

    void Start()
    {
        m_unit = GetComponent<Unit>();
        Debug.Assert(m_unit != null);

        m_active_states = new List<UnitState>();
        m_top_level_states = new List<UnitState>();

        m_activation_queue = new HashSet<UnitState>();
        m_deactivation_queue = new HashSet<UnitState>();

        if (walk_state)
        {
            m_walk_state_instance = UnitStateFactory.CreateUnitState(walk_state, this);
        }

        if (attack_state)
        {
            m_attack_state_instance = UnitStateFactory.CreateUnitState(attack_state, this);
        }

        if (movement_ability_state)
        {
            m_movement_ability_state_instance = UnitStateFactory.CreateUnitState(movement_ability_state, this);
        }

        if (empowered_movement_ability_state)
        {
            m_empowered_movement_ability_state_instance = UnitStateFactory.CreateUnitState(empowered_movement_ability_state, this);
        }

        QueueAddState(WalkState);
    }

    void Update()
    {
        if (m_top_level_states.Count <= 0)
        {
            // TODO: Decide state
            QueueAddState(m_walk_state_instance);
        }

        foreach (UnitState state in m_deactivation_queue)
        {
            RemoveState(state);
        }
        m_deactivation_queue.Clear();

        foreach (UnitState state in m_activation_queue)
        {
            AddState(state);
        }
        m_activation_queue.Clear();

        foreach (UnitState state in m_top_level_states)
        {
            state.OnFrameTick(m_unit, Time.deltaTime);

            // Recursive call children
            UnitState substate = state.substate;
            while(substate != null)
            {
                substate.OnFrameTick(m_unit, Time.deltaTime);
            }
        }
    }

    void FixedUpdate()
    {
        foreach (UnitState state in m_active_states)
        {
            state.OnPhysicsTick(m_unit, Time.deltaTime);
        }
    }

    Unit m_unit = null;

    [SerializeField]
    List<UnitState> m_active_states;

    [SerializeField]
    List<UnitState> m_top_level_states;

    [SerializeField]
    HashSet<UnitState> m_deactivation_queue;

    [SerializeField]
    HashSet<UnitState> m_activation_queue;

    private UnitState m_idle_state_instance;
    private UnitState m_movement_ability_state_instance;
    private UnitState m_empowered_movement_ability_state_instance;
    private UnitState m_walk_state_instance;
    private UnitState m_attack_state_instance;

    public class UnitStateFactory
    {
        // Creates a unit state and its substates to the state machine
        public static UnitState CreateUnitState(UnitState base_state, UnitStateMachine state_machine)
        {
            Debug.Assert(state_machine != null && base_state != null);

            UnitState new_state = UnitStateMachine.Instantiate(base_state);
            new_state.Init(state_machine);

            UnitState prevsubstate = new_state;
            UnitState substate = base_state.substate;
            while(substate != null)
            {
                UnitState new_substate = UnitStateMachine.Instantiate(substate);
                new_substate.Init(state_machine);

                // Parent
                prevsubstate.substate = new_substate;
                new_substate.superstate = prevsubstate;

                substate = substate.substate;
                prevsubstate = new_substate;
            }

            return new_state;
        }
    }
}
