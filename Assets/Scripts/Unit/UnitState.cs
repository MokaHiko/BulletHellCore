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
    virtual public void OnEnter(Unit unit) {}
    virtual public void OnExit(Unit unit) {}
    virtual public void OnFrameTick(Unit unit, float dt) {}
    virtual public void OnPhysicsTick(Unit unit, float dt){}
    virtual public bool ShouldEnd() {return false;}
} 

