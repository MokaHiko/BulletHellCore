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

    [SerializeField]
    public float combat_distance = 0.0f;

    // Drops
    [SerializeField]
    public ParticleSystem drop_particles;

    void Start()
    {
        Debug.Assert(drop_particles != null);
        Debug.Assert(tick_rate > 0.0f, "Cannot have a tick rate of 0 or less!");

        // Handles
        m_unit = GetComponent<Unit>();
        m_unit_controller = GetComponent<UnitController>();

        // Subscribe to callbacks
        m_unit.death_callback += OnDeath;

        // Target
        target = FindObjectOfType<PlayerController>().transform;

        // Ability Chains
        if (m_unit.abilities.Count > 1)
        {
            m_unit.abilities[(int)AbilityType.Movement].ability_end_callback += () => {
                if (m_unit)
                {
                    m_unit.UseAbility(AbilityType.Offensive);
                }
            };
        }
    }

    void Update()
    {
        time_elapsed += Time.deltaTime;

        if (target != null)
        {
            if ((target.position - transform.position).magnitude > combat_distance)
            {
                m_unit_controller.GoTo(target.position);
            }
            transform.LookAt(target, Vector3.up);

            Vector3 diff = target.position - transform.position;
            if (diff.magnitude > 25.0f) 
            {
                return;
            }

            // Use movement if ever
            if (time_elapsed > 1.0f / tick_rate)
            {
                m_unit.UseAbility(AbilityType.Movement, false, transform.forward);
                time_elapsed = 0.0f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if( collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.GetComponent<Unit>().TakeDamage(10.0f);
        }
    }

    void OnDeath()
    {
        // Drop modifier
        float will_drop = Random.Range(-1.0f, 1f);
        var particles = Instantiate(drop_particles, transform.position, Quaternion.identity);
        Vector3 back = -transform.forward;
        var module = particles.velocityOverLifetime;
        float magnitude = 10.0f;
        module.x = back.x * magnitude; module.y = back.y * magnitude; module.z = back.z * magnitude;
    }

    // ~ AI
    float time_elapsed = 0.0f;
   
    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
}
