using UnityEngine;
using UnityEngine.VFX;

public class Shield : Weapon
{
    [Header("Shield")]
    [SerializeField]
    public float charge_rate = 0.35f;

    [SerializeField]
    public VisualEffect shield_effect;

    [SerializeField]
    public GameObject barrier_prefab;

    private GameObject barrier;

    public void Start()
    {
        // Weapon asserts
        Debug.Assert(attack_speed != 0);

        // Weapon specific asserts
        Debug.Assert(shield_effect != null);
        on_reload += () =>
        {
            shield_effect.Stop();
        };
    }

    public override void AltAttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        if(barrier != null) 
        { 
            barrier.SetActive(false);
        }
    }

    public override void AttackImpl(Vector3 fire_point, Vector3 target_position)
    {
        if (barrier != null && barrier.activeSelf)
        {
            return;
        }

        shield_effect.Play();
        barrier = Instantiate(barrier_prefab, transform);
        barrier.GetComponent<Renderer>().enabled = false;    
        Invoke(nameof(ActivateShield), 2.5f);
    }

    public void ActivateShield()
    {
        barrier.GetComponent<Renderer>().enabled = true;    
    }
}
