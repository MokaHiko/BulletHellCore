using System.Collections;
using System;
using UnityEngine;
using System.Net.Mime;
using UnityEditorInternal;

public class MachineGun : Weapon
{
    [Header("Regular fire")]
    public ParticleSystem impact_particle_system;
    public float spread = 0.0f;
    public float charge_rate = 0.35f;
    public float max_attack_speed = 40.0f;

    [Header("Regular Fire Fx")]
    public ParticleSystem muzzle_flash;
    public TrailRenderer bullet_trail;
    public float bullet_trail_speed = 100.0f;
    public float regular_fire_shake = 2.0f;
    public float regular_fire_shake_time = 0.5f;

    [Header("Laser Fire")]
    public ParticleSystem laser_muzzle_flash;
    public TrailRenderer laser_bullet_trail;
    public ParticleSystem laser_impact_particle_system;
    public LayerMask laser_damageable_layers;
    public float laser_damage = 100.0f;
    public float laser_width = 100.0f;
    public float laser_bullet_trail_speed = 100.0f;
    public float laser_hit_stop = 0.25f;
    public float laser_hit_slow_mo_duration = 0.45f;
    public void Start()
    {
        // Weapon asserts
        Debug.Assert(Stats.attack_speed != 0);
        Debug.Assert(impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(bullet_trail_speed != 0);
        Debug.Assert(bullet_trail != null);
        Debug.Assert(muzzle_flash != null);

        Debug.Assert(laser_muzzle_flash != null);
        Debug.Assert(laser_bullet_trail_speed != 0);
        Debug.Assert(laser_impact_particle_system != null);
    }
    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        // Check if enough energy
        if (owner.GetComponent<Unit>().energy < owner.GetComponent<Unit>().BaseStats.max_energy * 0.65)
        {
            return;
        };

        // Reset energy
        owner.GetComponent<Unit>().energy = 0.0f;
        Stats.attack_speed = resource.base_stats.attack_speed;

        Shake(regular_fire_shake, regular_fire_shake_time);
        laser_muzzle_flash.Play();

        // Effect
        GameManager.Instance.RequestZoom(1.0f, 1.15f);
        GameManager.Instance.RequestVignette(1.0f, 0.5f);

        Vector3 diff = target_position - fire_point;
        Vector3 dir = diff.normalized;

        // Clamp
        dir.y = Mathf.Clamp(dir.y, -0.01f, dir.y);
        dir.Normalize();

        TrailRenderer trail = Instantiate(laser_bullet_trail, fire_point, Quaternion.identity);
        trail.startWidth = laser_width / 2.0f;

        Vector3 start_point = fire_point;
        float distance = Stats.range;
        while (distance > 0.1f)
        {
            if (Physics.SphereCast(start_point, laser_width, dir, out RaycastHit hit, Stats.range, laser_damageable_layers))
            {
                distance -= hit.distance;
                start_point = hit.point;

                hit.collider.TryGetComponent<Projectile>(out Projectile projectile);
                hit.collider.TryGetComponent<Unit>(out Unit unit);

                float crit_roll = UnityEngine.Random.Range(0.0f, 1.0f);
                if (unit != null)
                {
                    if (hit.collider.tag == "CritPoint")
                    {
                        unit = hit.collider.GetComponentInParent<Unit>();
                        crit_roll = 1.0f;
                    }
                }

                StartCoroutine(SpawnTrail(trail, laser_bullet_trail_speed, hit.point, hit.normal, laser_impact_particle_system, () =>
                {
                    if (projectile != null)
                    {
                        projectile.Die();
                    }

                    if (unit != null)
                    {
                        unit.TakeDamage(laser_damage, StatusEffect.None, crit_roll, hit.point);
                        unit.ApplyStatus(StatusEffect.Burning);
                    }

                    on_hit?.Invoke(hit.point, dir, hit.normal);
                }, true, distance < 0.1f));
            }
            else
            {
                StartCoroutine(SpawnTrail(trail,laser_bullet_trail_speed, fire_point + (dir * Stats.range), -dir.normalized, impact_particle_system));
                return;
            }
        }
    }
    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        // Heat
        Stats.attack_speed = Mathf.Lerp(resource.base_stats.attack_speed, max_attack_speed, owner.energy / owner.BaseStats.max_energy);

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

            StartCoroutine(SpawnTrail(trail,bullet_trail_speed, hit.point, hit.normal, impact_particle_system, () =>
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
            StartCoroutine(SpawnTrail(trail, bullet_trail_speed, fire_point + (dir * Stats.range), -dir.normalized, impact_particle_system));
        }
    }
    private IEnumerator SpawnTrail(TrailRenderer trail, float trail_speed, Vector3 end_position, Vector3 normal, ParticleSystem impact_effect, Action action = null, bool hit_stop = false, bool destroy_tail = true)
    {
        Vector3 start_postion = trail.transform.position;
        Vector3 diff = (end_position - start_postion);

        float time = 0;
        float travel_time = diff.magnitude / trail_speed;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(start_postion, end_position, time);
            time += Time.deltaTime / travel_time;

            yield return null;
        }
        trail.transform.position = end_position;

        if (destroy_tail)
        {
            Destroy(trail.gameObject, trail.time);
        }

        ParticleSystem impact_particles = Instantiate(impact_effect, end_position, Quaternion.LookRotation(normal));
        Destroy(impact_particles.gameObject, impact_particles.main.duration);

        if (hit_stop)
        {
            GameManager.Instance.RequestVignette(0.5f);
            GameManager.Instance.RequestSlowMo(laser_hit_slow_mo_duration, 0.01f);
        }

        // Invoke call back 
        action?.Invoke();

        yield return null;
    }
}
