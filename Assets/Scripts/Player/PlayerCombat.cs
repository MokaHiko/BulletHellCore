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
    public Weapon GetEquipedWeapon()
    {
        if (weapons != null)
        {
            return equiped_weapon;
        }

        return null;
    }
    public void Parry()
    {
        // Steal kennetic energy
        m_unit.Health += m_unit.LastDamage() * 0.5f;
        m_unit.energy += 5.0f;

        Collider[] hit_colliders = Physics.OverlapSphere(transform.position, perry_radius);
        foreach (Collider collider in hit_colliders)
        {
            // TODO: Layer mask except self
            if (collider.gameObject == gameObject) continue;

            if(collider.TryGetComponent<Unit>(out Unit unit)) 
            {
                // TODO: Push Back Coroutine
                Vector3 dir = Vector3.Normalize(collider.transform.position - transform.position);
                dir.y = 0;
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

    // Attack and target location
    public void Attack(Vector3 world_location)
    {
        if(equiped_weapon != null)
        {
            equiped_weapon.Attack(world_location);
            m_player_controller.AbortBurst();
        }
    }

    public void Reload()
    {
        if(equiped_weapon != null)
        {
            // TODO: on_reload callback
            equiped_weapon.Reload();
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
