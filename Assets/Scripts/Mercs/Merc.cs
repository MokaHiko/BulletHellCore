using UnityEngine;

public enum MercType
{
    Gunner = 0,
    Pyro = 1,
    Vanguard = 2,
    Medic = 3
};

public delegate void MercLevelUpCallback();

[RequireComponent(typeof(Unit))]
public class Merc : MonoBehaviour
{
    [Header("Merc")]
    [SerializeField]
    public MercType type;
    [SerializeField]
    public float experience = 0.0f;
    [SerializeField]
    UnitStateMachine unit_state_machine;

    [Header("Sfx")]
    public FMODUnity.EventReference particle_pickup_sfx;

    // ~ Merc Events
    public MercLevelUpCallback merc_level_up_callback;

    // ~ Getters
    public PlayerController Party { get; set; }
    public Vector3 TargetPosition{ get; set; }
    public UnitStateMachine StateMachine{ get { return unit_state_machine; } }

    void Start()
    {
        // Handles
        m_unit = GetComponent<Unit>();

        // Subscribe to callbacks
        m_unit.damaged_callback += (float damage) => { GameManager.Instance.RequestShake(5, 0.5f); };
        m_unit.death_callback += () => { GameManager.Instance.RequestShake(0, 0.0f); };
        m_unit.status_callback += OnStatusEffect;
    }
    public void OnParticleEnter(ParticleType type)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(particle_pickup_sfx, gameObject);

        switch (type) 
        {
            case (ParticleType.Exp):
            {
                float new_exp = experience + 1.0f;
                experience = (new_exp) % 100;
                if (new_exp >= 100)
                {
                    LevelUp();
                }
            }break;
            case (ParticleType.Energy):
            {
                m_unit.energy += 0.25f;
            }break;
            default:
                break;
        }
    }

    public void LevelUp()
    {
        // TODO: Level up effects
        merc_level_up_callback?.Invoke();
        GameManager.Instance.Reward(this);
    }
    private void OnStatusEffect(StatusEffect status_effect_flags)
    {
        if ((status_effect_flags & StatusEffect.ShortCircuit) == StatusEffect.ShortCircuit)
        {
        }
    }

    // ~ Handles
    private Unit m_unit;
}

