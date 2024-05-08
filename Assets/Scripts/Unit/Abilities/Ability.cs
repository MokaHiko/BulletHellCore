using UnityEngine;

public enum AbilityType
{
    Movement,
    Offensive,
}

public delegate void AbilityCallback();

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitMovement))]
public class Ability : MonoBehaviour
{
    [SerializeField]
    public float duration;

    [SerializeField]
    public float cooldown;

    [SerializeField]
    public float energy_cost = 15.0f;

    [SerializeField]
    private int stacks = 1;

    [SerializeField]
    private int max_stacks = 1;

    public AbilityCallback ability_begin_callback;
    public AbilityCallback ability_end_callback;
    void Awake()
    {
        m_unit = GetComponent<Unit>(); 
        m_unit_controller = GetComponent<UnitMovement>(); 
    }

    public void IncrementStack()
    {
        stacks = Mathf.Clamp(stacks + 1, 0, max_stacks);
    }

    // Returns whether ability was used succesefully
    public bool UseWithCost(bool burst, Vector3 direction)
    {
        // Burst overrides cooldowns
        if (!burst)
        {
            //if (time_elapsed < cooldown)
            //{
            //    return false;
            //}
            //time_elapsed = 0;
            if (stacks > 0)
            {
                stacks--;
            }
            else
            {
                return false;
            }
        }

        if (m_unit.SpendEnergy(energy_cost))
        {
            Use(burst, direction);
            return true;
        }

        return false;
    }

    public void Update()
    {
        time_elapsed += Time.deltaTime;
        if (time_elapsed > cooldown)
        {
            IncrementStack();
            time_elapsed = 0;
        }
    }

    public virtual void Use(bool burst = false, Vector3 direction = new Vector3()) { }

    // ~ Cooldowns
    protected float time_elapsed;

    // ~ Handles
    protected Unit m_unit;
    protected UnitMovement m_unit_controller;
}
