using UnityEngine;


[CreateAssetMenu]
public class DamageableResources : ScriptableObject
{
    public float damageable_duration = 0.1f;
    public ParticleSystem short_circuit_particles;
    public ParticleSystem death_particles;
    public GameObject floating_text;
    public Material damaged_material;

    [Header("Spring Damp")]
    public float start_velocity = 1.0f;
    public float spring = 1.0f;
    public float damp = 1.0f;
}
