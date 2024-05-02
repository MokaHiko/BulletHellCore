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
    public LayerMask damageable_layers;

    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera m_cinemachine_camera;

    // ~ Callbacks

    // Guaranteed
    public WeaponFireCallback on_fire; // On Fire

    // Conditional
    public WeaponHitCallback on_hit; // On Hit
    //public WeaponOverHeatCallback on_over_heat; // On Reload (ex. increase fire rate for 3.0f seconds)

    protected void Shake(float intensity, float time)
    {
        if (m_shake_routine != null)
        {
            StopCoroutine(m_shake_routine);
            m_shake_routine = null;
        }

        m_shake_routine = StartCoroutine(ShakeEffect(intensity, time));
    }

    void Awake()
    {
        OnEquip();

        // TODO: Look for cinemachine camera
    }

    public void Attack(Vector3 target_position)
    {
        if(m_time_since_last_fire <= (1.0f / attack_speed))
        {
            return;
        }

        m_time_since_last_fire = 0.0f;

        AttackImpl(transform.position, target_position, m_player_controller.IsBurst());
        on_fire?.Invoke(target_position);
    }

    public virtual void AttackImpl(Vector3 fire_point, Vector3 target_position, bool is_burst = false) { }

    public void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
    }

    void OnEquip()
    {
        m_unit = GetComponentInParent<Unit>();
        m_player_controller = GetComponentInParent<PlayerController>();
    }

    // Effects
    IEnumerator ShakeEffect(float intensity, float shake_time)
    {
        var cinemachine_perlin = m_cinemachine_camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachine_perlin.m_AmplitudeGain = intensity;

        float time = 0.0f;
        while(time < shake_time) 
        {
            time += Time.deltaTime;
            yield return null;
        }

        cinemachine_perlin.m_AmplitudeGain = 0;
        m_shake_routine = null;
    }
    
    // ~ Weapon Common
    protected float m_time_since_last_fire = 0.0f;

    // ~ Effects
    Coroutine m_shake_routine;

    // ~ Handles
    protected Unit m_unit;
    protected PlayerController m_player_controller;
}

