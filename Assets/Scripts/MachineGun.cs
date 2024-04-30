using System.Collections;
using System;
using UnityEngine;
using Cinemachine;

public class MachineGun : Weapon
{
    [Header("Regular fire")]
    [SerializeField]
    public ParticleSystem impact_particle_system;

    [SerializeField]
    public float spread = 0.0f;

    [SerializeField]
    public ParticleSystem muzzle_flash;

    [SerializeField]
    public TrailRenderer bullet_trail;
    public float bullet_trail_speed = 100.0f;

    [SerializeField]
    public float regular_fire_shake = 2.0f;

    [Header("Burst fire")]
    public int bounce_count = 5;
    public float bounce_damage_multipler = 1.15f;
    public float burst_fire_shake = 5.0f;

    [SerializeField]
    public ParticleSystem burst_impact_particle_system;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(m_fire_point != null);
        Debug.Assert(impact_particle_system != null);
        Debug.Assert(burst_impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        Debug.Assert(muzzle_flash != null);

        m_time_since_last_fire = 0.0f;
    }

    private void Update()
    {
        m_time_since_last_fire += Time.deltaTime * Mathf.Clamp(1 - (m_unit.energy / m_unit.BaseStats().max_energy), 0, 1);
    }
    public override void Attack()
    {
        Fire();
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
            Shake(burst_fire_shake, muzzle_flash.main.duration);

            // TODO: Play different muzzle
            muzzle_flash.Play();

            for (int i = 0; i < 12; i++)
            {
                Vector3 dir = m_fire_point.forward + new Vector3( UnityEngine.Random.Range(-spread  * 2.0f, spread * 2.0f), 0, UnityEngine.Random.Range(-spread * 2.0f, spread * 2.0f));
                dir.Normalize();

                // TODO : Add layer mask
                TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
                if (Physics.SphereCast(transform.position, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
                {
                    if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                    {
                        float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);
                        unit.TakeDamage(base_damage, StatusEffect.None, crit_roll);
                    }

                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, burst_impact_particle_system, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, base_damage, bounce_count); }));
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized, burst_impact_particle_system));
                }
            }
            return;
        }

        m_time_since_last_fire = 0;

        // Fire
        {
            muzzle_flash.Play();
            Shake(regular_fire_shake, muzzle_flash.main.duration);

            Vector3 dir = transform.forward + new Vector3( UnityEngine.Random.Range(-spread, spread), 0, UnityEngine.Random.Range(-spread, spread));
            dir.Normalize();

            // TODO : Add layer mask
            TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
            if (Physics.SphereCast(transform.position, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
            {
                if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                {
                    float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);
                    unit.TakeDamage(base_damage, StatusEffect.None, crit_roll);
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, impact_particle_system));
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, impact_particle_system));
                }
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized,impact_particle_system));
            }

            // Sap energy on hit
            m_unit.energy += 2.0f;
        }
    }
    private void Bounce(Vector3 spawn_point, Vector3 dir, float damage, int bounce_count = 0)
    {
        // Decrement bounce
        bounce_count--;

        TrailRenderer trail = Instantiate(bullet_trail, spawn_point, Quaternion.identity);
        if (Physics.SphereCast(spawn_point, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
        {
            if (hit.collider.TryGetComponent<Unit>(out Unit unit))
            {
                float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);
                unit.TakeDamage(damage, StatusEffect.None, crit_roll);
            }

            // Stop bounce if count reached
            if (bounce_count <= 0) 
            { 
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, impact_particle_system));
                return;
            }

            // Bounce again
            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal,impact_particle_system, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, damage * bounce_damage_multipler, bounce_count); }));
        }
        else
        {
            // Stop bounce if nothing hit
            StartCoroutine(SpawnTrail(trail, spawn_point + (dir * range), -dir.normalized, impact_particle_system));
        }
    }
    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 end_position, Vector3 normal, ParticleSystem impact_effect, Action action = null)
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
        ParticleSystem impact_particles = Instantiate(impact_effect, end_position, Quaternion.LookRotation(normal));
        Destroy(impact_particles.gameObject, impact_particles.main.duration);

        // Invoke call back 
        action?.Invoke();

        yield return null;
    }

    // ~ Combat
    private float m_time_since_last_fire;

    [SerializeField]
    private Transform m_fire_point;
}
