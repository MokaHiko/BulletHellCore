using System.Collections;
using UnityEngine;

public class Slide : Ability
{
    [SerializeField]
    public float slide_multiplier = 1.15f;

    [SerializeField]
    public StatusEffect slide_status = StatusEffect.None;

    private bool burst;
    private Vector3 dir;

    public override void Use(bool burst, Vector3 direction) 
    {
        dir = direction;
        dir.Normalize();

        m_unit.animator.SetTrigger("dash");
        m_unit.animator.GetComponent<AnimationSignalHandler>().begin_callback += () =>
        {
            ability_begin_callback?.Invoke();

            AbortSlide();
            m_slide_routine = StartCoroutine(SlideEffect(burst, dir));
        };

        m_unit.animator.GetComponent<AnimationSignalHandler>().end_callback += () =>
        {
            ability_end_callback?.Invoke();
        };
    }

    void AbortSlide()
    {
        if(m_slide_routine != null) 
        { 
            StopCoroutine(m_slide_routine);
            m_slide_routine = null;
        }

        m_unit.RemoveStatus(slide_status);
        m_unit.RemoveState(UnitStateFlags.ManagedMovement);
        m_unit_controller.GoTo(transform.position);
    }

    private IEnumerator SlideEffect(bool burst = false, Vector3 direction = new Vector3())
    {
        m_unit.ApplyState(UnitStateFlags.ManagedMovement);

        Rigidbody rb = m_unit.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        if (direction.magnitude == 0)
        {
            direction = transform.forward;
        }

        float time = 0.0f;
        while (time < duration / 2.0f)
        {
            //m_unit.transform.forward = rb.velocity.normalized;
            m_unit.transform.LookAt(m_unit.transform.position + rb.velocity.normalized);
            rb.velocity = direction * slide_multiplier;
            time += Time.deltaTime;
            yield return null;
        }

        time = 0.0f;
        while (time < duration / 2.0f)
        {
            m_unit.transform.LookAt(m_unit.transform.position + rb.velocity.normalized);
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.normalized * m_unit.BaseStats.movement_speed, time / (duration / 2.0f));
            time += Time.deltaTime;
            yield return null;
        }

        AbortSlide();
    }

    // ~ Handles
    private Coroutine m_slide_routine;
}
