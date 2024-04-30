using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        // Look for cinemachine camera
    }

    public virtual void Attack() { }

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

    // ~ Effects
    Coroutine m_shake_routine;

    // ~ Handles
    protected Unit m_unit;
    protected PlayerController m_player_controller;
}

