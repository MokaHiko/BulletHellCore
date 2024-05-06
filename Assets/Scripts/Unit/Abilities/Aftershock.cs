using UnityEngine;

public class Aftershock : Ability
{
    [SerializeField]
    private float radius = 5.0f;

    [SerializeField]
    private float damage = 35.0f;

    [SerializeField]
    private LayerMask damageable;

    [SerializeField]
    public ParticleSystem shock_particles;

    public override void Use(bool burst, Vector3 direction) 
    {
        ParticleSystem shock = Instantiate(shock_particles, m_unit.transform.position, Quaternion.identity);
        Destroy(shock, shock.main.duration);
        foreach (Collider col in Physics.OverlapSphere(transform.position, radius, damageable))
        {
            if (col.TryGetComponent<Unit>(out Unit unit))
            {
                unit.TakeDamage(damage);
                break;
            }
        }
    }
}
