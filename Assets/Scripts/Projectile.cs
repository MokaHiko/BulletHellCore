using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    public float damage = 10.0f;

    [SerializeField]
    public float life_span = 5.0f;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        Destroy(gameObject, life_span);
    }

    public void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Unit>(out Unit unit))
        {
            if (unit.CheckStatus(StatusEffect.Armored))
            {
                GetComponent<Rigidbody>().velocity *= -1.0f;
                return;
            }

            unit.TakeDamage(10);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
