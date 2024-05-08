using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitMovement))]
public class EnemyController : MonoBehaviour
{
    // AI
    [SerializeField]
    Pathfinding path_finding;

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
        m_unit_controller = GetComponent<UnitMovement>();

        // Subscribe to callbacks
        m_unit.death_callback += OnDeath;

        // Target
        PlayerController player;
        if ( player = FindObjectOfType<PlayerController>())
        {
            target = player.transform;
        }

        // Pathfinding
        path_finding.seeker_transform = transform;
        path_finding.target_transform = target.transform;

        path_finding.grid = GetComponentInParent<Room>().GetComponent<PathfindingGrid>();

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
            transform.LookAt(target, Vector3.up);

            if (time_elapsed > 1.0f / tick_rate)
            {
                time_elapsed = 0.0f;
                
                // Attack if in range
                Vector3 diff = target.position - transform.position;
                if (diff.magnitude < combat_distance)
                {
                    // Use movement ability if exists
                    m_unit.UseAbility(AbilityType.Movement, false, diff.normalized);
                }

                // Go To Target 
                path_finding.FindPath();
                if (path_finding.path.Count > 0)
                {
                    if (m_follow_routine != null)
                    {
                        StopCoroutine(m_follow_routine);
                        m_follow_routine = null;
                    }

                    m_follow_routine = StartCoroutine(FollowPath());
                }
            }
        }
    }

    IEnumerator FollowPath()
    {
        int cell = 0;
        Vector3 target_position = path_finding.path[cell].world_position;

        while (cell < path_finding.path.Count - 1)
        {
            transform.LookAt(target, Vector3.up);

            m_unit_controller.GoTo(target_position);
            if ((transform.position - target_position).magnitude < 0.5f)
            {
                target_position = path_finding.path[++cell].world_position;
            }

            yield return null;
        }

        m_follow_routine = null;
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
    Coroutine m_follow_routine;
   
    // ~ Handles
    private Unit m_unit;
    private UnitMovement m_unit_controller;
}
