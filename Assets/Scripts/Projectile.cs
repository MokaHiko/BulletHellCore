using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    public float damage = 10.0f;

    [SerializeField]
    public float life_span = 5.0f;

    [SerializeField]
    public ParticleSystem impact_effect;

    [SerializeField]
    public LayerMask immune_layers;

    private void Start()
    {
        Debug.Assert(impact_effect != null);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }

    public void Update()
    {
        life_span -= Time.deltaTime;

        if (life_span <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Use layer mask
        if (immune_layers == (immune_layers | (1 << other.gameObject.layer)))
        {
            return;
        }

        if (other.TryGetComponent<Unit>(out Unit unit))
        {
            if (unit.CheckStatus(StatusEffect.Armored))
            {
                GetComponent<Rigidbody>().velocity *= -1.0f;
                return;
            }

            unit.TakeDamage(damage);
            Die();
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        ParticleSystem particles = Instantiate(impact_effect, transform.position, Quaternion.identity);
        Destroy(particles.gameObject, particles.main.duration);
        Destroy(gameObject);
    }
}
