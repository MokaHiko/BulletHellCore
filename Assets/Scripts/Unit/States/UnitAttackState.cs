using UnityEngine;

public class UnitAttackState : UnitState
{
    public virtual void Attack(Vector3 world_location) {}
    public virtual void AltAttack(Vector3 world_location) {}
}

