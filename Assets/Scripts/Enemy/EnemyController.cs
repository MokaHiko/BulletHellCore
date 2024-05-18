using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Pathfinding))]
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
    public List<ShopItem> drop_attributes;

    [SerializeField]
    UnitStateMachine unit_state_machine;

    // Getters
    public Vector3 TargetPosition {get{return m_target_position;}}
    public Vector3 AttackTargetPosition {get{return m_target_position;}}

    public void SetTarget(Transform target_)
    {
        target = target_;
    }

    void Start()
    {
        Debug.Assert(drop_particles != null);
        Debug.Assert(tick_rate > 0.0f, "Cannot have a tick rate of 0 or less!");

        // Handles
        m_unit = GetComponent<Unit>();

        // Subscribe to callbacks
        m_unit.death_callback += OnDeath;

        TrackPlayer();

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

    void TrackPlayer()
    {
        // Target
        PlayerController player;
        if ( player = FindObjectOfType<PlayerController>())
        {
            target = player.transform;
        }
        else
        {
            return;
        }

        // Pathfinding
        path_finding.seeker_transform = transform;
        path_finding.target_transform = target.transform;

        // TODO: Remove room system
        path_finding.grid = FindObjectOfType<Room>().GetComponent<PathfindingGrid>();
    }

    void Update()
    {
        time_elapsed += Time.deltaTime;

        if (target != null)
        {
            transform.LookAt(target, Vector3.up);
            m_target_position = target.position;

            if (time_elapsed > 1.0f / tick_rate)
            {
                time_elapsed = 0.0f;

                // TODO: Make raycast
                // Attack if in range
                Vector3 diff = m_target_position - transform.position;

                if (diff.magnitude < combat_distance)
                {
                    unit_state_machine.QueueRemoveState(unit_state_machine.WalkState);
                    unit_state_machine.QueueAddState(unit_state_machine.AttackState);
                    return;
                }

                if (false)
                {
                    // Use movement ability if exists
                    //m_unit.UseAbility(AbilityType.Movement, false, diff.normalized);
                }

                // Go To Target 
                path_finding.FindPath();
                if (path_finding.path.Count > 0)
                {
                    unit_state_machine.QueueAddState(unit_state_machine.WalkState);
                    if (m_follow_routine != null)
                    {
                        StopCoroutine(m_follow_routine);
                        m_follow_routine = null;
                    }

                    m_follow_routine = StartCoroutine(FollowPath());
                }
            }
        }
        else
        {
            TrackPlayer();
        }
    }

    IEnumerator FollowPath()
    {
        int cell = 0;
        Vector3 target_position = path_finding.path[cell].world_position;

        while (cell < path_finding.path.Count - 1)
        {
            m_target_position = target_position;
            if ((transform.position - target_position).magnitude < 0.5f)
            {
                target_position = path_finding.path[++cell].world_position;
            }

            yield return null;
        }

        // Path complete 

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
        GameManager.Instance.RequestSlowMo(0.45f);
 
        // Drop modifier
        var particles = Instantiate(drop_particles, transform.position, Quaternion.identity);
        Vector3 back = -transform.forward;

        var module = particles.velocityOverLifetime;
        float magnitude = 10.0f;
        module.x = back.x * magnitude; module.y = back.y * magnitude; module.z = back.z * magnitude;

        // Drop Items
        foreach (ShopItem attrib in drop_attributes)
        {
            Instantiate(attrib, transform.position, Quaternion.identity);
        }
    }

    // ~ AI
    Vector3 m_target_position;

    // ~ AI
    float time_elapsed = 0.0f;
    Coroutine m_follow_routine;
   
    // ~ Handles
    private Unit m_unit;
}
