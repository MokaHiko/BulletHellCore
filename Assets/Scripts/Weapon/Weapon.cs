using System.Collections;
using UnityEngine;

public delegate void WeaponFireCallback(Vector3 world_position);
public delegate void WeaponHitCallback(Vector3 point, Vector3 dir, Vector3 normal = new Vector3());
public delegate void WeaponReloadCallback();

public class Weapon : MonoBehaviour
{
    [SerializeField]
    public WeaponResource resource;

    [Header("Weapon")]
    [SerializeField]
    public float base_damage;
    [SerializeField]
    public float attack_speed = 5f;
    [SerializeField]
    public float scan_radius = 1.0f;
    [SerializeField]
    public float range = 1000.0f;
    [SerializeField]
    public int max_bullets = 32;
    [SerializeField]
    public float reload_time = 1.0f;
    [SerializeField]
    public float hold_threshold = 1.0f;
    [SerializeField]
    public LayerMask damageable_layers;

    // ~ Handles
    public Unit owner;
    //public PlayerController player_controller;

    // ~ Callbacks

    // Guaranteed
    public WeaponFireCallback on_fire;

    // Conditional
    public WeaponHitCallback on_hit; 

    public WeaponReloadCallback on_reload; 

    //public WeaponOverHeatCallback on_over_heat; // On Reload (ex. increase fire rate for 3.0f seconds)

    // TODO: Move to callback
    protected void Shake(float intensity, float time)
    {
        GameManager.Instance.RequestShake(intensity, time);
    }

    void Awake()
    {
        // Copy base stats
        max_bullets = resource.max_bullets;
        bullets = max_bullets;

        OnEquip();
    }

    public void Attack(Vector3 target_position, bool alt_attack = false)
    {
        if (bullets < 0)
        {
            Reload();
            return;
        }

        if(alt_attack)
        {
            AltAttackImpl(transform.position, target_position);
        }
        else
        {
            if(m_time_since_last_fire <= (1.0f / attack_speed))
            {
                return;
            }
            m_time_since_last_fire = 0.0f;
            bullets--;
            AttackImpl(transform.position, target_position);
        }
        on_fire?.Invoke(target_position);
    }

    public virtual void AttackImpl(Vector3 fire_point, Vector3 target_position) { }
    public virtual void AltAttackImpl(Vector3 fire_point, Vector3 target_position) { }

    public void Reload()
    {
        // Check if already reloading
        if (m_reload_coroutine == null)
        {
            on_reload?.Invoke();
            m_reload_coroutine = StartCoroutine(ReloadEffect());
        }
    }

    public IEnumerator ReloadEffect()
    {
        yield return new WaitForSeconds(reload_time);
        bullets = max_bullets;
        m_reload_coroutine = null;
    }

    public void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
    }

    void OnEquip()
    {
        owner = GetComponentInParent<Unit>();
    }
    
    // ~ Weapon Common
    protected float m_time_since_last_fire = 0.0f;
    [SerializeField]
    protected int bullets = 0;

    Coroutine m_reload_coroutine = null;
}

