using System.Collections;
using UnityEngine;

public class Slide : Ability
{
    [SerializeField]
    public float slide_multiplier;

    [SerializeField]
    public StatusEffect slide_status = StatusEffect.None;

    private bool burst;
    private Vector3 dir;

    public override void Use(bool burst, Vector3 direction) 
    {
        dir = direction;

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

        //AbortSlide();
        //m_slide_routine = StartCoroutine(SlideEffect(burst, direction));
    }

    void AbortSlide()
    {
        if(m_slide_routine != null) 
        { 
            StopCoroutine(m_slide_routine);
            m_slide_routine = null;
        }

        m_unit.RemoveStatus(slide_status);
        m_unit.RemoveState(UnitState.ManagedMovement);
    }

    private IEnumerator SlideEffect(bool burst = false, Vector3 direction = new Vector3())
    {
        //m_unit.animator.SetTrigger("dash");
        m_unit.ApplyState(UnitState.ManagedMovement);

        Rigidbody rb = m_unit.GetComponent<Rigidbody>();

        direction = m_unit.transform.forward;

        float time = 0.0f;
        while (time < duration / 2.0f)
        {
            rb.AddForce(direction * slide_multiplier, ForceMode.Impulse);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0.0f;
        while (time < duration / 2.0f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, direction * m_unit.BaseStats().movement_speed, time / (duration / 2.0f));
            time += Time.deltaTime;
            yield return null;
        }

        AbortSlide();
    }

    // ~ Handles
    private Coroutine m_slide_routine;
}
