using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitController))]
public class EnemyController : MonoBehaviour
{
    // Combat
    [SerializeField]
    public Transform target;

    // Drops
    [SerializeField]
    public GameObject drop;

    void Start()
    {
        Debug.Assert(drop != null);

        // Handles
        m_unit = GetComponent<Unit>();
        m_unit_controller = GetComponent<UnitController>();

        // Subscribe to callbacks
        m_unit.death_callback += OnDeath;

        // Target
        target = FindObjectOfType<PlayerController>().transform;
    }

    void Update()
    {
        if (target != null)
        {
            m_unit_controller.GoTo(target.position);
            transform.LookAt(target, Vector3.up);
        }
    }

    void OnDeath()
    {
        // Drop modifier
        float will_drop = Random.Range(-1.0f, 1f);
        if (will_drop > 0.0f)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }

    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
}
