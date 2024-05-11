using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWalkState", menuName = "EnemyUnitWalkState")]
public class EnemyWalkState : UnitWalkState
{
    public override void OnEnter(Unit unit)
    {
        m_enemy_controller = unit.GetComponent<EnemyController>();
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        // Rotate unit
        Vector3 look_at = m_enemy_controller.TargetPosition;
        look_at.y = unit.transform.position.y;
        unit.transform.LookAt(look_at, Vector3.up);

        // Move
        GoTo(unit, m_enemy_controller.TargetPosition);
    }

    EnemyController m_enemy_controller;
}
