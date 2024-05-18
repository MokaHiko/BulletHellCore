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
    public float charge_rate = 0.35f;

    [SerializeField]
    public ParticleSystem muzzle_flash;

    [SerializeField]
    public TrailRenderer bullet_trail;
    public float bullet_trail_speed = 100.0f;

    [Header("Effects")]
    [SerializeField]
    public float regular_fire_shake = 2.0f;
    public float regular_fire_shake_time = 0.5f;
    [SerializeField]
    public float alt_fire_hit_stop = 0.1f;
    public float alt_fire_hit_slow_mo_duration = 0.45f;

    [Header("Burst fire")]
    public float burst_fire_shake = 5.0f;
    public float burst_spread = 45.0f;
    public float burst_multiplier = 20.0f;
    public int base_shots = 4;

    [SerializeField]
    public ParticleSystem burst_impact_particle_system;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(Stats.attack_speed != 0);
        Debug.Assert(impact_particle_system != null);
        Debug.Assert(burst_impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        Debug.Assert(muzzle_flash != null);
    }

    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        int extra_shots = Mathf.FloorToInt((Stats.max_bullets - m_bullets) * 0.5f);
        int shots = base_shots + Mathf.Clamp(extra_shots, 0, extra_shots);

        if (shots <= 0)
        {
            return;
        }

        GameManager.Instance.RequestZoom(0.5f, 1.05f);
        Shake(burst_fire_shake * shots, muzzle_flash.main.duration);
        muzzle_flash.Play();

        owner.energy -= 50.0f;
        Vector3 diff = target_position - fire_point;
        Vector3 dir = diff.normalized;

        float theta = Mathf.Deg2Rad * (90 - burst_spread);

        // One time effects
        bool effect = false;
        bool hit_stop = false;

        for (int i = 0; i < shots; i++)
        {
            // Recoil
            theta += Mathf.Deg2Rad * burst_spread / shots;
            dir += owner.transform.rotation * new Vector3(Mathf.Cos(theta),0, Mathf.Sin(theta));
            dir.Normalize();

            TrailRenderer trail = Instantiate(bullet_trail, fire_point, Quaternion.identity);
            if (Physics.SphereCast(fire_point, Stats.scan_radius, dir, out RaycastHit hit, Stats.range, damageable_layers))
            {
                hit.collider.TryGetComponent<Unit>(out Unit unit);

                if (unit && !hit_stop && !effect)
                {
                    hit_stop = true;
                    effect = true;
                }

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, burst_impact_particle_system, () =>
                {
                    if (unit)
                    {
                        float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);
                        unit.TakeDamage(Stats.base_damage * burst_multiplier, StatusEffect.None, crit_roll, hit.point);
                    }

                    on_hit?.Invoke(hit.point, dir, hit.normal);
                    on_fire?.Invoke(target_position);
                }, hit_stop));

                if (hit_stop && unit)
                {
                    hit_stop = false;
                }
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, fire_point + (dir * Stats.range), -dir.normalized, burst_impact_particle_system));
            }
        }
    }
    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        Shake(regular_fire_shake, regular_fire_shake_time);
        muzzle_flash.Play();

        Vector3 diff = target_position - fire_point;
        Vector3 dir = diff.normalized;

        // Recoil
        dir += new Vector3(UnityEngine.Random.Range(-spread, spread), 0, UnityEngine.Random.Range(-spread, spread));
        dir.y = Mathf.Clamp(dir.y, -0.01f, dir.y);
        dir.Normalize();

        TrailRenderer trail = Instantiate(bullet_trail, fire_point, Quaternion.identity);
        if (Physics.SphereCast(fire_point, Stats.scan_radius, dir, out RaycastHit hit, Stats.range, damageable_layers))
        {
            hit.collider.TryGetComponent<Unit>(out Unit unit);
            float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);

            if (hit.collider.tag == "CritPoint")
            {
                unit = hit.collider.GetComponentInParent<Unit>();
                crit_roll = 1.0f;
            }

            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, impact_particle_system, () =>
            {
                if (unit)
                {
                    unit.TakeDamage(Stats.base_damage, StatusEffect.None, crit_roll, hit.point);

                    // ~ Property:
                    // Sap energy on hit
                    owner.energy += charge_rate * Stats.base_damage;
                }
                on_hit?.Invoke(hit.point, dir, hit.normal);
            }));
        }
        else
        {
            StartCoroutine(SpawnTrail(trail, fire_point + (dir * Stats.range), -dir.normalized, impact_particle_system));
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 end_position, Vector3 normal, ParticleSystem impact_effect, Action action = null, bool hit_stop = false)
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

        if (hit_stop)
        {
            //GameManager.Instance.RequestStop(alt_fire_hit_stop);
            GameManager.Instance.RequestVignette(0.5f);
            GameManager.Instance.RequestSlowMo(alt_fire_hit_slow_mo_duration, 0.01f);
        }

        // Invoke call back 
        action?.Invoke();


        yield return null;
    }
}
