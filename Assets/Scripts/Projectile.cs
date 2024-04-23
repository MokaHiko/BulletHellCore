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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Unit>(out Unit unit))
        {
            unit.TakeDamage(10);
        }
    }
}
