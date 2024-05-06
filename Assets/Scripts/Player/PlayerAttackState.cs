using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[CreateAssetMenu(fileName = "PlayerAttackState", menuName = "PlayerUnitAttackState")]
public class PlayerAttackState : UnitAttackState
{
    [SerializeField]
    public List<Weapon> weapons;

    [SerializeField]
    public float target_radius = 1.0f;

    [SerializeField]
    public LayerMask targetable;

    public override void OnEnter(Unit unit)
    {
        m_player_controller = unit.GetComponent<PlayerController>();
        m_state_machine = unit.GetComponent<UnitStateMachine>();

        // TODO: Turn into combat scriptable object
        m_equiped_weapon = m_player_controller.GetComponent<PlayerCombat>().GetEquipedWeapon();
    }

    public override void OnExit(Unit unit)
    {
    }

    public Weapon GetEquipedWeapon()
    {
        if (weapons != null)
        {
            return m_equiped_weapon;
        }
        return null;
    }

    // Attack and target location
    public override void Attack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            m_equiped_weapon.Attack(world_location);
            m_player_controller.AbortBurst();
        }
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        // Fire current weapon
        if (Input.GetMouseButton(0))
        {
            Attack(m_player_controller.WorldMousePoint);
        }
        else
        {
            m_state_machine.QueueRemoveState(this);
        }
    }

    // ~ Weapons
    [SerializeField]
    private Weapon m_equiped_weapon;

    // ~ Handles
    private PlayerController m_player_controller;
    private UnitStateMachine m_state_machine;
}

