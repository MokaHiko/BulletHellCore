using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class DamageableResources : ScriptableObject
{
    public ParticleSystem short_circuit_particles;
    public ParticleSystem death_particles;
    public GameObject floating_text;
    public Material damaged_material;
}
