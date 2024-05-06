using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PlayerController))]
[CreateAssetMenu(fileName = "PlayerWalkState", menuName = "PlayerUnitWalkState")]
public class PlayerWalkState : UnitWalkState
{
    public float target_radius;
    public LayerMask targetable;

    public override void OnEnter(Unit unit)
    {
        m_player_controller = unit.GetComponent<PlayerController>();
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        // Rotate character
        Vector3 look_at = m_player_controller.WorldMousePoint;
        look_at.y = unit.transform.position.y;
        unit.transform.LookAt(look_at, Vector3.up);

        // Move
        Vector3 target_pos = unit.transform.position + m_player_controller.RelativeAxisInput;
        GoTo(unit, target_pos);

        // Dash
        // if (Input.GetMouseButtonDown((int)MouseButton.RightMouse) || Input.GetKeyDown(KeyCode.LeftShift))
        // {
        //     unit.UseAbility(AbilityType.Movement, m_player_controller.IsBurst(), m_player_controller.RelativeAxisInput);
        //     m_player_controller.AbortBurst();
        // }

        // ~ Combat
        // Parry
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (RequestBurst() && m_unit.CheckState(UnitState.TakingDamage))
        //     {
        //         m_player_combat.Parry();
        //         AbortBurst();
        //     }
        // }


        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     m_player_combat.Reload();
        // }
    }

    PlayerController m_player_controller;
}
