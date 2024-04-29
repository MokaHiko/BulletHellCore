using System.Collections;
using System;
using UnityEngine;

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

    [Header("Burst fire")]
    public int bounce_count = 5;
    public float bounce_damage_multipler = 1.15f;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(m_fire_point != null);
        Debug.Assert(impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        Debug.Assert(muzzle_flash != null);

        m_time_since_last_fire = 0.0f;
    }

    private void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
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
            for (int i = 0; i < 8; i++)
            {
                muzzle_flash.Play();
                Vector3 dir = m_fire_point.forward + new Vector3( UnityEngine.Random.Range(-spread  * 2.0f, spread * 2.0f), 0, UnityEngine.Random.Range(-spread * 2.0f, spread * 2.0f));
                dir.Normalize();

                // TODO : Add layer mask
                TrailRenderer trail = Instantiate(bullet_trail, m_fire_point.position, Quaternion.identity);
                if (Physics.SphereCast(transform.position, scan_radius, dir, out RaycastHit hit, range, damageable_layers))
                {
                    // Stop bounce if doing damage
                    if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                    {
                        unit.TakeDamage(base_damage);
                    }

                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, base_damage * bounce_damage_multipler, bounce_count); }));
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
                }
            }
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
                if (hit.collider.TryGetComponent<Unit>(out Unit unit))
                {
                    unit.TakeDamage(base_damage);
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));

                    // Sap energy on hit
                    if (unit.SpendEnergy(2.5f))
                    {
                        m_unit.energy += 2.5f;
                    }
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
                }
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, m_fire_point.position + (dir * range), -dir.normalized));
            }
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
                unit.TakeDamage(damage);
            }

            // Stop bounce if count reached
            if (bounce_count <= 0) 
            { 
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
                return;
            }

            // Bounce again
            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, () => { Bounce(hit.point, Vector3.Reflect(dir, hit.normal).normalized, damage * bounce_damage_multipler, bounce_count); }));
        }
        else
        {
            // Stop bounce if nothing hit
            StartCoroutine(SpawnTrail(trail, spawn_point + (dir * range), -dir.normalized));
        }
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

    [SerializeField]
    private Transform m_fire_point;
}
