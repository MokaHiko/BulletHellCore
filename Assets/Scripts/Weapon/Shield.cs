using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Shield : Weapon
{
    [Header("Shield")]
    [SerializeField]
    public float charge_rate = 0.35f;
    [SerializeField]
    public float max_health = 100.0f;

    [SerializeField]
    private Barrier barrier;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);
    }

    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        barrier.gameObject.SetActive(false);

        if (m_recharge_routine != null)
        {
            StopCoroutine(m_recharge_routine);
        }
        m_recharge_routine = StartCoroutine(Recharge());
    }

    IEnumerator Recharge()
    {
        barrier.Health = Mathf.Clamp(barrier.Health + charge_rate * Time.deltaTime, 0, max_health);
        yield return null;
    }

    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        if (barrier.gameObject.activeSelf)
        {
            return;
        }

        barrier.gameObject.SetActive(true);

        if (m_recharge_routine != null)
        {
            StopCoroutine(m_recharge_routine);
        }
    }

    private Coroutine m_recharge_routine;
}
