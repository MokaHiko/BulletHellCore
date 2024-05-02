using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    Exp,
    Money,
    Energy,
};

public class ParticleCollector : MonoBehaviour
{
    [SerializeField]
    ParticleSystem particle_system;

    [SerializeField]
    ParticleType particle_type;

    List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();

    public delegate void ParticleEnteredCallback(ParticleType type);

    public ParticleEnteredCallback particle_entered_callback;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        particle_system.trigger.AddCollider(player.GetComponent<Collider>());

        particle_entered_callback += player.OnParticleEnter;
    }

    private void OnParticleTrigger()
    {
        int triggered_particles = particle_system.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        for(int i = 0; i < triggered_particles; i++) 
        {
            ParticleSystem.Particle p = particles[i];
            p.remainingLifetime = 0;
            particles[i] = p;

            particle_entered_callback?.Invoke(particle_type);
        }

        particle_system.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
    }
}
