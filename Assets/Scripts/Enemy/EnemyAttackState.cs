using System;
using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "EnemyUnitAttackState")]
public class EnemyAttackState : UnitAttackState
{
    [SerializeField]
    float attack_delay = 1.0f;
    public override void OnEnter(Unit unit)
    {
        m_enemy_controller = unit.GetComponent<EnemyController>();
        m_equiped_weapon = unit.EquipedWeapon;
    }

    public override void OnExit(Unit unit)
    {
        if (m_delayed_attack_routine != null)
        {
            unit.StopCoroutine(m_delayed_attack_routine);
            m_delayed_attack_routine = null;
        }
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

        if (m_equiped_weapon == null)
        {
            return;
        }

        if (m_delayed_attack_routine == null)
        {
            Vector3 target_position = m_enemy_controller.target.position + Vector3.up;
            m_delayed_attack_routine = unit.StartCoroutine(DelayedAttack(attack_delay, target_position));
        }
    }

    IEnumerator DelayedAttack(float delay, Vector3 target_position)
    {
        // TODO: wind up
        yield return new WaitForSeconds(delay);

        Attack(target_position);
        m_delayed_attack_routine = null;
    }

    // ~ Weapons
    [SerializeField]
    private Weapon m_equiped_weapon;

    // ~ Handles
    private EnemyController m_enemy_controller;

    private Coroutine m_delayed_attack_routine;
}
