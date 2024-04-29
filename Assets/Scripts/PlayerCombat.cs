using System.Collections.Generic;
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

    [SerializeField]
    public List<Weapon> weapons;

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

    public void Attack()
    {
        if(equiped_weapon != null)
        {
            equiped_weapon.Attack();
            m_player_controller.AbortBurst();
        }
    }

    void Start()
    {
        // Combat Asserts
        Debug.Assert(perry_particles != null);

        // Handles
        m_unit = GetComponent<Unit>();
        m_player_controller = GetComponent<PlayerController>();

        // Equip wepaon
        if(weapons.Count > 0) 
        { 
            equiped_weapon = weapons[0];
        }
    }

    // ~ Weapons
    [SerializeField]
    private Weapon equiped_weapon;

    // ~ Handles
    private Unit m_unit;
    private PlayerController m_player_controller;
}
