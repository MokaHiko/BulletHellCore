using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    // TODO: Put in weapon
    // ~ Weapon
    [SerializeField]
    public float damage;

    [SerializeField]
    public float attack_speed = 5f;

    [SerializeField]
    public float range = 1000.0f;

    [SerializeField]
    public ParticleSystem impact_particle_system;

    // ~ Weapon Specific
    [SerializeField]
    public float spread = 0.0f;

    [SerializeField]
    public ParticleSystem muzzle_flash;

    [SerializeField]
    public TrailRenderer bullet_trail;
    public float bullet_trail_speed = 100.0f;

    public void Fire()
    {
        if (m_time_since_last_fire <= (1.0f / attack_speed) )
        {
            // TODO: Jammed sound effect
            return;
        }
        m_time_since_last_fire = 0;

        //muzzle_flash.Play();

        Vector3 dir = m_fire_point.forward + new Vector3(Random.Range(-spread, spread), 0, Random.Range(-spread, spread));
        dir.Normalize();

        // TODO : Add layer mask
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent<Unit>(out Unit unit))
            {
                unit.TakeDamage(damage);
            }

            TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
        }
        else
        {
            TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
        }
    }
    void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(m_fire_point != null);
        Debug.Assert(impact_particle_system != null);


        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        //Debug.Assert(muzzle_flash != null);

        // Defaults
        m_time_since_last_fire = 0.0f;
    }

    void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 end_position, Vector3 normal)
    {
        Vector3 start_postion = trail.transform.position;
        Vector3 diff = (end_position - start_postion);
        //Vector3 dir = diff.normalized;

        float time = 0;
        float travel_time = diff.magnitude / bullet_trail_speed;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(start_postion, end_position, time);
            time += Time.deltaTime / travel_time;

            yield return null;
        }
        trail.transform.position = end_position;

        Destroy(trail.gameObject, trail.time);
        ParticleSystem impact_particles = Instantiate(impact_particle_system, end_position, Quaternion.LookRotation(normal));
        Destroy(impact_particles.gameObject, impact_particles.main.duration);

        yield return null;
    }

    // ~ Combat
    private float m_time_since_last_fire;

    // ~ Handles
    [SerializeField]
    private Transform m_fire_point;
}
