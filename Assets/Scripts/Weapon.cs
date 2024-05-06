using Cinemachine;
using System.Collections;
using UnityEngine;

public delegate void WeaponFireCallback(Vector3 world_position);
public delegate void WeaponHitCallback(Vector3 point, Vector3 dir, Vector3 normal = new Vector3());

public class Weapon : MonoBehaviour
{
    // ~ Weapon
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
    public LayerMask damageable_layers;

    // ~ Handles
    public Unit owner;
    public PlayerController player_controller;

    // ~ Callbacks

    // Guaranteed
    public WeaponFireCallback on_fire; // On Fire

    // Conditional
    public WeaponHitCallback on_hit; // On Hit
    //public WeaponOverHeatCallback on_over_heat; // On Reload (ex. increase fire rate for 3.0f seconds)

    protected void Shake(float intensity, float time)
    {
        player_controller.RequestShake(intensity, time);
    }

    void Awake()
    {
        bullets = max_bullets;
        OnEquip();
    }

    public void Attack(Vector3 target_position)
    {
        if (bullets < 0)
        {
            Reload();
            return;
        }

        if(m_time_since_last_fire <= (1.0f / attack_speed))
        {
            return;
        }

        m_time_since_last_fire = 0.0f;

        bullets--;
        // On reload
        AttackImpl(transform.position, target_position, player_controller.IsBurst());
        on_fire?.Invoke(target_position);
    }

    public virtual void AttackImpl(Vector3 fire_point, Vector3 target_position, bool is_burst = false) { }

    public void Reload()
    {
        if (m_reload_coroutine == null)
        {
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
        player_controller = GetComponentInParent<PlayerController>();
    }
    
    // ~ Weapon Common
    protected float m_time_since_last_fire = 0.0f;
    [SerializeField]
    protected int bullets = 0;

    Coroutine m_reload_coroutine = null;
}

