using UnityEngine;

public class UnitWalkState : UnitState
{
    public Vector3 TargetLocation { get{return m_target_location;} set{m_target_location = value;}}
    protected void GoTo(Unit unit, Vector3 position)
    {
        if (unit == null)
        {
            return;
        }

        if (unit.CheckState(UnitStateFlags.ManagedMovement))
        {
            return;
        }

        unit.ApplyState(UnitStateFlags.Moving);
        m_target_location = position;
    }

    public sealed override void OnPhysicsTick(Unit unit, float dt) 
    {
        if (unit.CheckState(UnitStateFlags.ManagedMovement))
        {
            return;
        }

        Vector3 diff = (m_target_location - unit.transform.position);
        Debug.DrawLine(m_target_location, unit.transform.position, Color.red);

        // Velocity based movement
        Vector3 dir = diff.normalized;
        Vector3 velocity = (dir * unit.movement_speed);
        unit.GetRigidbody.velocity = new Vector3(velocity.x, unit.GetRigidbody.velocity.y, velocity.z);
    }

    [SerializeField]
    private Vector3 m_target_location = Vector3.zero;
}
