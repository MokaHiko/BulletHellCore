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

    [SerializeField]
    public float tick_rate = 1.0f;

    // Drops
    [SerializeField]
    public GameObject drop;

    void Start()
    {
        Debug.Assert(drop != null);
        Debug.Assert(tick_rate > 0.0f, "Cannot have a tick rate of 0 or less!");

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
        time_elapsed += Time.deltaTime;

        if (target != null)
        {
            m_unit_controller.GoTo(target.position);
            transform.LookAt(target, Vector3.up);

            Vector3 diff = target.position - transform.position;
            if (diff.magnitude > 10.0f) 
            {
                return;
            }

            if (time_elapsed > 1.0f / tick_rate)
            {
                m_unit.UseAbility(0);
                time_elapsed = 0.0f;
            }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player_controller))
        {
            player_controller.GetComponent<Unit>().TakeDamage(35.0f);
        }
    }

    // ~ AI
    float time_elapsed = 0.0f;
   
    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
}
