using UnityEngine;

[CreateAssetMenu(fileName = "EnemyIdleState", menuName = "EnemyUnitIdleState")]
public class EnemyIdleState : UnitIdleState
{
    [SerializeField]
    private float detect_radius = 1.0f;

    [SerializeField]
    private LayerMask aggro_layer;

    public override void OnEnter(Unit unit)
    {
        m_enemy_controller = unit.GetComponent<EnemyController>();
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        foreach(Collider col in Physics.OverlapSphere(unit.transform.position, detect_radius, aggro_layer))
        {
            if (col.TryGetComponent<PlayerController>(out PlayerController player_controller))
            {
                m_enemy_controller.SetTarget(player_controller.transform);
                StateMachine.QueueAddState(StateMachine.WalkState);
                return;
            }
        }
    }

    EnemyController m_enemy_controller;
}
