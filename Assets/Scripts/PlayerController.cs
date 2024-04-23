using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitController))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    void Start()
    {
        m_unit = GetComponent<Unit>();
        m_unit_controller = GetComponent<UnitController>();

        m_player_combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // ~ Combat

        // Fire current weapon
        if (Input.GetMouseButton(0))
        {
            m_player_combat.Fire();
        }

        // ~ Movement

        // Look at mouose position
        if (Camera.main)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 look_at = hit.point;
                look_at.y = transform.position.y;

                transform.LookAt(look_at, Vector3.up);
            }
        }

        // Move to input axis
        m_axis_input.x = Input.GetAxisRaw("Horizontal");
        m_axis_input.y = Input.GetAxisRaw("Vertical");
        if (m_axis_input.magnitude == 0)
        {
            return;
        }
        m_unit_controller.GoTo(new Vector3(transform.position.x + m_axis_input.x, transform.position.y, transform.position.z + m_axis_input.y));
    }

    private Vector2 m_axis_input;

    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
    private PlayerCombat m_player_combat;
}
