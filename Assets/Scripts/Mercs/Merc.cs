using UnityEngine;

public enum MercType
{
    Gunner,
    Pyro,
    Vanguard,
    Medic
};

public delegate void MercLevelUpCallback();

[RequireComponent(typeof(Unit))]
public class Merc : MonoBehaviour
{
    [SerializeField]
    private float experience = 0.0f;

    //[Header("HUD UI")]
    //public PropertyBar exp_bar;

    [SerializeField]
    UnitStateMachine unit_state_machine;

    // Player events
    public MercLevelUpCallback player_level_up_callback;

    //fmod ref
    public FMODUnity.EventReference particle_pickup_sfx;
    

    // ~ Getters
    public PlayerController Party { get; set; }
    public Vector3 TargetPosition{ get; set; }
    public UnitStateMachine StateMachine{ get { return unit_state_machine; } }

    void Start()
    {
        // World handles
        //exp_bar = GameManager.Instance.GetPlayerHud().exp_bar;
        //Debug.Assert(exp_bar != null, "Player no exp bar!");

        // Handles
        m_unit = GetComponent<Unit>();

        // Subscribe to callbacks
        m_unit.damaged_callback += (float damage) => { GameManager.Instance.RequestShake(5, 0.5f); };
        m_unit.death_callback += () => { GameManager.Instance.RequestShake(0, 0.0f); };
        m_unit.status_callback += OnStatusEffect;
    }
    void Update()
    {
        // ~ HUD UI
        //exp_bar.SetValue(experience, 100.0f);
    }
    public void OnParticleEnter(ParticleType type)
    {
        //fmod call not working
        //Debug.Log("particlesoundplayed");
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
                m_unit.energy += 1.0f;
                    
                }
                break;
            default:
                break;
        }
        
    }

    private void LevelUp()
    {
        player_level_up_callback?.Invoke();
    }

    private void OnStatusEffect(StatusEffect status_effect_flags)
    {
        if ((status_effect_flags & StatusEffect.ShortCircuit) == StatusEffect.ShortCircuit)
        {
        }
    }

    // ~ Handles
    private PlayerController m_party;
    private Unit m_unit;
}

