using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    // General Combat
    [SerializeField] 
    float perry_radius = 5.0f;

    [SerializeField] 
    float perry_force = 100.0f;

    [SerializeField] 
    ParticleSystem perry_particles;

    // TODO: Put in weapon
    // ~ Weapon
    [SerializeField]
    public float base_damage;

    [SerializeField]
    public float attack_speed = 5f;

    [SerializeField]
    public float scan_radius = 1.0f;

    [SerializeField]
    public float range = 1000.0f;

    [SerializeField]
    public ParticleSystem impact_particle_system;

    // ~ Weapon Specific
    [SerializeField]
    LayerMask damageable_layers;

    [SerializeField]
    public float spread = 0.0f;

    [SerializeField]
    public ParticleSystem muzzle_flash;

    [SerializeField]
    public TrailRenderer bullet_trail;
    public float bullet_trail_speed = 100.0f;
    public void Perry()
    {
        // Steal kennetic energy
        m_unit.health += 10.0f;
        m_unit.energy += 10.0f;

        Collider[] hit_colliders = Physics.OverlapSphere(transform.position, perry_radius);
        foreach (Collider collider in hit_colliders)
        {
            // TODO: Layer mask except self
            if (collider.gameObject == gameObject) continue;

            if(collider.TryGetComponent<Unit>(out Unit unit)) 
            { 
                // TODO: Push Back Coroutine
                Vector3 dir = Vector3.Normalize(collider.transform.position - transform.position);
                unit.GetComponent<Rigidbody>().AddForce(dir * perry_force, ForceMode.Impulse);
            }

            if(collider.TryGetComponent<Projectile>(out Projectile projectile)) 
            {
                projectile.Die();
            }
        }

        // Perry effect
        perry_particles.Play();
    }

    public void Fire()
    {
        if (m_time_since_last_fire <= (1.0f / attack_speed) )
        {
            // TODO: Jammed sound effect
            return;
        }

        if (m_player_controller.IsBurst())
        {
            for (int i = 0; i < 10; i++)
            {
                muzzle_flash.Play();
                Vector3 dir = m_fire_point.forward + new Vector3( UnityEngine.Random.Range(-spread  * 5.0f, spread * 5.0f), 0, UnityEngine.Random.Range(-spread, spread));
                dir.Normalize();

                // TODO : Add layer mask
                TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
                if (Physics.SphereCast(transform.position, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
                {
                    // Stop bounce if doing damage
                    if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                    {
                        unit.TakeDamage(base_damage);
                        StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
                    }
                    else
                    {
                        // Bounce
                        int bounces = m_player_controller.IsBurst() ? 3 : 1;
                        StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, base_damage * 2.0f, bounces); }));
                    }
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
                }
            }

            m_player_controller.AbortBurst();
            return;
        }

        m_time_since_last_fire = 0;

        // Fire
        {
            muzzle_flash.Play();
            Vector3 dir = m_fire_point.forward + new Vector3( UnityEngine.Random.Range(-spread, spread), 0, UnityEngine.Random.Range(-spread, spread));
            dir.Normalize();

            // TODO : Add layer mask
            TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
            if (Physics.SphereCast(transform.position, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
            {
                // Stop bounce if doing damage
                if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                {
                    unit.TakeDamage(base_damage);
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
                }
                else
                {
                    // Bounce
                    int bounces = m_player_controller.IsBurst() ? 10 : 1;
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, base_damage * 2.0f, bounces); }));
                }
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
            }
        }

        // Abort and use burst
        m_player_controller.AbortBurst();
    }
    private void Bounce(Vector3 spawn_point, Vector3 dir, float damage, int bounce_count = 0)
    {
        // Decrement bounce
        bounce_count--;

        TrailRenderer trail = Instantiate(bullet_trail, spawn_point, Quaternion.identity);
        if (Physics.SphereCast(spawn_point, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
        {
            // Stop bounce if doing damage
            if (hit.collider.TryGetComponent<Unit>(out Unit unit))
            {
                unit.TakeDamage(damage);
                StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
                return;
            }

            // Stop bounce if count reached
            if (bounce_count <= 0) 
            { 
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
                return;
            }

            // Bounce again
            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, damage * 2.0f, bounce_count); }));
        }
        else
        {
            // Stop bounce if nothing hit
            StartCoroutine(SpawnTrail(trail, spawn_point + (dir * range), -dir.normalized));
        }
    }
    void Start()
    {
        // Combat Asserts
        Debug.Assert(perry_particles != null);

        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(m_fire_point != null);
        Debug.Assert(impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        Debug.Assert(muzzle_flash != null);

        // Handles
        m_unit = GetComponent<Unit>();
        m_player_controller = GetComponent<PlayerController>();

        // Defaults
        m_time_since_last_fire = 0.0f;
    }

    void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 end_position, Vector3 normal, Action action = null)
    {
        Vector3 start_postion = trail.transform.position;
        Vector3 diff = (end_position - start_postion);

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

        // Invoke call back 
        action?.Invoke();

        yield return null;
    }

    // ~ Combat
    private float m_time_since_last_fire;

    // ~ Handles
    private Unit m_unit;
    private PlayerController m_player_controller;

    [SerializeField]
    private Transform m_fire_point;
}
