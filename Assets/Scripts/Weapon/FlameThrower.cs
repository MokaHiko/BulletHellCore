using UnityEngine;

public class FlameThrower : Weapon
{
    [Header("Flamethrower")]
    [SerializeField]
    public ParticleSystem impact_particle_system;
    [SerializeField]
    public float spread = 0.0f;
    [SerializeField]
    public float charge_rate = 0.35f;
    [SerializeField]
    public ParticleSystem flame_particles;
    [SerializeField]
    public float regular_fire_shake = 2.0f;

    [SerializeField]
    public ParticleSystem burst_impact_particle_system;

    public FMODUnity.EventReference flamerSFX;
    private FMOD.Studio.EventInstance flamerSFXInstance;

    [SerializeField]
    public ParticleEventHandler particle_event_handler;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
        Debug.Assert(impact_particle_system != null);
        Debug.Assert(burst_impact_particle_system != null);

        // Weapon specific asserts
        Debug.Assert(flame_particles != null);

        particle_event_handler.particle_enter_callback += FlameThrowerDamage;

        //fmodsfx
        flamerSFXInstance = FMODUnity.RuntimeManager.CreateInstance(flamerSFX);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(flamerSFXInstance, gameObject.transform);

        on_reload += () =>
        {
            StopFlameThrower();
            flamerSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        };

       
    }

    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        StopFlameThrower();
        flamerSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        if (!flame_particles.isPlaying)
        {
            Debug.Log("Play!");
            flame_particles.Play();
        flamerSFXInstance.start();
        }
    }

    private void StopFlameThrower()
    {
        if(flame_particles.isPlaying)
        {
            Debug.Log("STOP!");
            flame_particles.Stop();
        }
    }

    public void FlameThrowerDamage(GameObject other)
    {
        if (other.TryGetComponent<Unit>(out Unit unit))
        {
            unit.TakeDamage(base_damage, StatusEffect.Burning, 0, other.transform.position);
        }
    }
}
