using UnityEngine;


[CreateAssetMenu(fileName = "EnemyPatrolState", menuName = "EnemyUnitPatrolState")]
public class EnemyPatrolState : UnitState
{
    [SerializeField]
    public Vector2 start_patrol_offset;

    [SerializeField]
    public Vector2 end_patrol_offset;

    public override void OnEnter(Unit unit) 
    {
    }

    public override void OnExit(Unit unit)
    {
    }

    public override void OnFrameTick(Unit unit, float dt)
    {

    }
}
