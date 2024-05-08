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
    public UnitState substate;
    public UnitState superstate;

    public bool Exclusive { get { return m_exclusive; } }

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

    private UnitStateMachine m_state_machine;
    protected bool m_exclusive = false;
} 

