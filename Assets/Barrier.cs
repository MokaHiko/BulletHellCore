using UnityEngine;

public class Barrier : IDamageable
{
    [SerializeField]
    float charge_rate = 1.0f;
    [SerializeField]
    public MeshRenderer barrier_renderer;

    protected override bool ShouldDie() { return false; }

    private void OnEnable()
    {
        barrier_renderer.enabled = false;    
        Invoke(nameof(ActivateShield), 2.2f);

        death_callback += () =>
        {
            Deactivate();
        };
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
    }

    public void Deactivate()
    {

    }

    public void ActivateShield()
    {
        barrier_renderer.enabled = true;
    }
}
