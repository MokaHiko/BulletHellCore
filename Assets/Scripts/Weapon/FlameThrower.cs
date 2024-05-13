using System.Collections;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;

public class FlameThrower : Weapon
{
    [Header("Regular fire")]
    [SerializeField]
    public ParticleSystem impact_particle_system;

    [SerializeField]
    ParticleEventHandler particle_event_handler;

    [SerializeField]
    public float spread = 0.0f;

    [SerializeField]
    public float charge_rate = 0.35f;

    [SerializeField]
    public ParticleSystem flame_particles;

    [SerializeField]
    public float regular_fire_shake = 2.0f;

    [Header("Burst fire")]
    public float burst_fire_shake = 5.0f;
    public float burst_spread = 20.0f;
    public float burst_multiplier = 20.0f;
    public int shots = 4;

    [SerializeField]
    public ParticleSystem burst_impact_particle_system;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(impact_particle_system != null);
        Debug.Assert(burst_impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(flame_particles != null);

        particle_event_handler.particle_enter_callback += FlameThrowerDamage;

        on_reload += () =>
        {
            flame_particles.Stop();
        };
    }

    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        flame_particles.Stop();
    }

    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        //Shake(regular_fire_shake, flame_particles.main.duration);
        flame_particles.Play();
    }

    public void FlameThrowerDamage(GameObject other)
    {
        if (other.TryGetComponent<Unit>(out Unit unit))
        {
            unit.TakeDamage(base_damage, StatusEffect.Burning, 0, other.transform.position);
        }
    }

}
