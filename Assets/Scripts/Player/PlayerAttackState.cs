using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        m_equiped_weapon = unit.EquipedWeapon;
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

    public override void AltAttack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            m_equiped_weapon.Attack(world_location, true);
            m_player_controller.AbortBurst();
        }
    }

    [SerializeField]
    float hold_time = 0.5f;
    float time_held = 0.0f;

    void Release()
    {
        if(time_held > hold_time) 
        {
            AltAttack(m_player_controller.WorldMousePoint);
        }
        time_held = 0.0f;
    }
    public override void OnFrameTick(Unit unit, float dt)
    {
        // Fire current weapon
        if(Input.GetMouseButton(0)) 
        {
            if (m_player_controller.IsBurst())
            {
                time_held += Time.deltaTime;
                if (time_held > hold_time)
                {
                    Release();
                }
            }
            else
            {
                Attack(m_player_controller.WorldMousePoint);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Release();
        }

        //if (Input.GetMouseButton(0))
        //{
        //    if(m_player_controller.IsBurst())
        //    {
        //        AltAttack(m_player_controller.WorldMousePoint);
        //    }
        //    else
        //    {
        //        Attack(m_player_controller.WorldMousePoint);
        //    }
        //}

        StateMachine.QueueRemoveState(this);
    }

    // ~ Weapons
    [SerializeField]
    private Weapon m_equiped_weapon;

    // ~ Handles
    private PlayerController m_player_controller;
}

