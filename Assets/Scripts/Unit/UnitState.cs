using System.Threading.Tasks;
using UnityEngine;

public interface IUnitState
{
    public bool ShouldEnd();
    public void OnEnter(Unit unit);
    public void OnExit(Unit unit);
    public void OnFrameTick(Unit unit, float dt);
    public void OnPhysicsTick(Unit unit, float dt);
}

public class UnitState : ScriptableObject, IUnitState
{
    public UnitState SubState { get { return m_substate; } set { m_substate = value; } }
    public UnitState SuperState { get { return m_superstate; } set { m_superstate = value; }}
    public bool Exclusive { get { return m_exclusive; }}
    public bool IsInterruptable { get {return m_interruptable; }}

    public UnitState(UnitState substate = null, UnitState superstate = null, bool exclusive = false)
    {
        m_exclusive = exclusive;

        m_superstate = superstate;
        m_superstate = substate;
    }

    // Getters
    public UnitStateMachine StateMachine {get{return m_state_machine;}}
    public Unit Owner {get{return m_state_machine.Owner;}}

    virtual public void OnEnter(Unit unit) {}
    virtual public void OnExit(Unit unit) {}
    virtual public void OnFrameTick(Unit unit, float dt) {}
    virtual public void OnPhysicsTick(Unit unit, float dt){}
    virtual public bool ShouldEnd() {return false;}
    public void Init(UnitStateMachine state_machine)
    {
        m_state_machine = state_machine;
    }

    [SerializeField]
    private UnitState m_substate;
    [SerializeField]
    private UnitState m_superstate;

    private UnitStateMachine m_state_machine;
    protected bool m_exclusive = false;
    protected bool m_interruptable = true;
} 

