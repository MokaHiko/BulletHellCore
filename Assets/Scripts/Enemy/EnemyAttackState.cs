using UnityEngine;


[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "EnemyUnitAttackState")]
public class EnemyAttackState : UnitAttackState
{
    public override void OnEnter(Unit unit)
    {
        m_enemy_controller = unit.GetComponent<EnemyController>();
        m_equiped_weapon = unit.EquipedWeapon;
    }

    public override void OnExit(Unit unit)
    {
    }

    public Weapon GetEquipedWeapon()
    {
        return m_equiped_weapon;
    }

    // Attack and target location
    public override void Attack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            m_equiped_weapon.Attack(world_location);
        }
    }

    public override void AltAttack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            m_equiped_weapon.Attack(world_location, true);
        }
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        // Exit condition
        float distance_to_target = (m_enemy_controller.target.position - Owner.transform.position).magnitude;
        if (distance_to_target > m_enemy_controller.combat_distance)
        {
            StateMachine.QueueRemoveState(this);
            return;
        }

        Attack(m_enemy_controller.target.position + Vector3.up);
    }

    // ~ Weapons
    [SerializeField]
    private Weapon m_equiped_weapon;

    // ~ Handles
    private EnemyController m_enemy_controller;

}
