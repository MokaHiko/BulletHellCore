using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 10.0f;

    [Header("Projectile")]
    public Vector3 velocity = Vector3.zero;
    public float life_span = 5.0f;

    [Header("Effects")]
    public ParticleSystem impact_effect;
    public ParticleSystem parry_effect;

    [SerializeField]
    public LayerMask collidable_layers;
    public LayerMask immune_layers;

    // Parametric
    Vector3 m_start_position;
    float m_time_alive = 0.0f;

    public void UpdatePosition(float dt)
    {
        transform.position += m_start_position + velocity * dt;
    }

    public void CheckCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x * 0.75f, collidable_layers);
        if (colliders.Length > 0)
        {
            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    // Check if parrried
                    if (damageable.CheckStatus(StatusEffect.Armored))
                    {
                        Instantiate(parry_effect, transform.position, Quaternion.identity, transform);

                        // TODO: Switch if player
                        int player_layer = LayerMask.NameToLayer("Player");
                        int enemy_layer = LayerMask.NameToLayer("Enemy");
                        collidable_layers |= (1 << enemy_layer);
                        collidable_layers &= ~(1 << player_layer);

                        GameManager.Instance.RequestSlowMo(0.15f, 0.01f);
                        GameManager.Instance.RequestShake(1.0f, 0.15f);

                        velocity *= -1.5f;

                        return;
                    }

                    damageable.TakeDamage(damage);
                    break;
                }
            }

            Die();
        }
    }

    private void Start()
    {
        Debug.Assert(impact_effect != null);
    }

    public void Update()
    {
        m_time_alive += Time.deltaTime;

        // Check Collision
        CheckCollision();

        // Update Position
        UpdatePosition(Time.deltaTime);

        if (m_time_alive >= life_span)
        {
            Die();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    // TODO: Use layer mask
    //    if (immune_layers == (immune_layers | (1 << other.gameObject.layer)))
    //    {
    //        return;
    //    }

    //    if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
    //    {
    //        if (damageable.CheckStatus(StatusEffect.Armored))
    //        {
    //            Instantiate(parry_effect, transform.position, Quaternion.identity, transform);
    //            GetComponent<Rigidbody>().velocity *= -4.0f;

    //            GameManager.Instance.RequestSlowMo(0.15f, 0.01f);
    //            GameManager.Instance.RequestShake(1.0f, 0.15f);

    //            // Switch sides on armor
    //            int player_layer = LayerMask.NameToLayer("Player");
    //            int enemy_layer = LayerMask.NameToLayer("Enemy");
    //            immune_layers |= (1 << player_layer);
    //            immune_layers &= ~(1 << enemy_layer);

    //            return;
    //        }

    //        damageable.TakeDamage(damage);
    //        Die();
    //    }
    //    else
    //    {
    //        Die();
    //    }
    //}

    public void Die()
    {
        ParticleSystem particles = Instantiate(impact_effect, transform.position, Quaternion.identity);
        Destroy(particles.gameObject, particles.main.duration);
        Destroy(gameObject);
    }
}
