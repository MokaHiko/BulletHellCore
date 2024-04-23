using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Rigidbody))]
public class UnitController : MonoBehaviour
{
    public void GoTo(Vector3 position)
    {
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
        Vector3 diff = (m_target_location - transform.position);
        if (diff.magnitude < 0.5)
        {
            m_target_location = transform.position;
            return;
        }

        Vector3 dir = diff.normalized;
        Vector3 forward = Vector3.forward * dir.z * m_unit.agility; forward.y = 0;
        Vector3 right = Vector3.right * dir.x * m_unit.agility; right.y = 0;

        m_rb.AddForce(forward, ForceMode.Impulse);
        m_rb.AddForce(right, ForceMode.Impulse);
        if (m_rb.velocity.magnitude > m_unit.movement_speed)
        {
            m_rb.velocity = m_rb.velocity.normalized * m_unit.movement_speed;
        }
    }

    [SerializeField]
    private Vector3 m_target_location = Vector3.zero;

    private Rigidbody m_rb;
    private Unit m_unit;
}
