using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Ability
{
    [SerializeField]
    TrailRenderer trail_renderer;

    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    //fmod audio reference
    public FMODUnity.EventReference dashAudio;
    public FMODUnity.EventReference teleportAudio;

    public void Start()
    {
        // Handles
        Debug.Assert(trail_renderer != null);

    }
    public override void Use(bool burst) 
    {
        if (m_dash_routine != null)
        {
            StopCoroutine(m_dash_routine);
            AbortDash();
        }

        m_dash_routine = StartCoroutine(DashEffect(burst));
    }

    void AbortDash()
    {
        m_unit.RemoveStatus(dash_status);
        m_unit.RemoveState(UnitState.ManagedMovement);

        float start_speed = m_unit.BaseStats().movement_speed;
        float start_agility = m_unit.BaseStats().agility;
        m_unit.movement_speed = start_speed;
        m_unit.agility = start_agility;

        m_dash_routine = null;
    }

    private IEnumerator DashEffect(bool burst = false)
    {
        float start_speed = m_unit.BaseStats().movement_speed;
        float start_agility = m_unit.BaseStats().agility;

        if (m_trail == null)
        {
            m_trail = Instantiate(trail_renderer, transform);
            Destroy(m_trail.gameObject, duration);
        }

        if (burst)
        {
            m_unit.ApplyStatus(dash_status);

            // TP
            m_unit.transform.position += m_unit.transform.forward * 10.0f;
            if (m_unit.TryGetComponent(out PlayerCombat combat))
            {
                combat.Perry();
            }
            //fmod teleport audio
            //FMODUnity.RuntimeManager.PlayOneShotAttached(teleportAudio, gameObject);
        }
        else
        {
            m_unit.ApplyState(UnitState.ManagedMovement);

            Rigidbody rb = m_unit.GetComponent<Rigidbody>();
            rb.velocity = rb.velocity.normalized * start_speed;
            rb.AddForce(m_unit.transform.forward * dash_multiplier, ForceMode.Impulse);

            //fmod dash audio
            //FMODUnity.RuntimeManager.PlayOneShot(dashAudio,transform.position);
            FMODUnity.RuntimeManager.PlayOneShotAttached(dashAudio, gameObject);
            
            
        }

        yield return new WaitForSeconds(duration);

        AbortDash();
    }

    // ~ Handles
    private Coroutine m_dash_routine;
    private TrailRenderer m_trail;
}
