using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Rigidbody))]
public class UnitMovement : MonoBehaviour
{
    public void GoTo(Vector3 position)
    {
        if (m_unit == null)
        {
            return;
        }

        if (m_unit.CheckState(UnitStateFlags.ManagedMovement))
        {
            return;
        }

        m_unit.ApplyState(UnitStateFlags.Moving);
        m_target_location = position;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get Handles
        m_rb = GetComponent<Rigidbody>();
        m_rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        m_unit = GetComponent<Unit>();

        // Default values
        m_target_location = transform.position;
    }

    private void FixedUpdate()
    {
        if (m_unit.CheckState(UnitStateFlags.ManagedMovement))
        {
            return;
        }

        Vector3 diff = (m_target_location - transform.position);
        Debug.DrawLine(m_target_location, transform.position, Color.red);

        // Velocity based movement
        Vector3 dir = diff.normalized;
        Vector3 velocity = (dir * m_unit.movement_speed);
        m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);
    }

    [SerializeField]
    private Vector3 m_target_location = Vector3.zero;

    // ~ Handles
    private Rigidbody m_rb;
    private Unit m_unit;
}
