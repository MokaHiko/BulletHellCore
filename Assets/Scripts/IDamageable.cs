using System.Collections;
using UnityEngine;
using TMPro;

[System.Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1 << 0,
    ShortCircuit = 1 << 1,
    Corrosion = 1 << 2,
    Armored = 1 << 3,
};

// Called whenever the unit is damaged
public delegate void DamagedCallback(float damage);

// Called when a status is applied to the unity
public delegate void StatusCallback(StatusEffect status_effects);

// Called before the unit dies
public delegate void DeathCallback();

public class IDamageable : MonoBehaviour
{
    [Header("IDamageable")]
    public DamageableResources damageable_resources;
    public float damage_multiplier = 1.0f;
    public Renderer damageable_renderer;

    // Getters & Setters
    public float Health { get { return m_health; } set{m_health = value; } }
    public StatusEffect Status { get { return m_status_effect; } set { m_status_effect = value;}}

    // ~ Callbacks
    public DeathCallback death_callback;
    public DamagedCallback damaged_callback;
    public StatusCallback status_callback;

    //fmod variable
    public FMODUnity.EventReference on_damage_sfx;

    // ~ IDamageable
    public float LastDamage() { return m_last_damage; }
    public void TakeDamage(float damage, StatusEffect damage_type = StatusEffect.None, float crit_roll = 0.0f, Vector3 hit_position = new Vector3())
    {
        if (m_damage_effect_routine == null)
        {
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }
        else
        {
            StopCoroutine(m_damage_effect_routine);
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }

        if (m_spring_effect_routine == null)
        {
            m_spring_effect_routine = StartCoroutine(nameof(SpringDampEffect));
        }
        else
        {
            m_spring_velocity += damageable_resources.start_velocity;
        }

        // Check for crit
        Color damage_color = Color.white;
        if (damage_type == StatusEffect.Corrosion)
        {
            damage_color = Color.green;
        }
        else if (crit_roll > 0.95f)
        {
            damage *= UnityEngine.Random.Range(2.0f, 3.0f);
            damage_color = Color.red;
        }
        else if (crit_roll > 0.85f)
        {
            damage *= UnityEngine.Random.Range(1.5f, 2.0f);
            damage_color = Color.yellow;
        }
        else
        {
            damage *= UnityEngine.Random.Range(0.8f, 1.2f);
        }
        damage = (int)damage;

        m_last_damage = damage * damage_multiplier;
        m_health -= damage * damage_multiplier;

        damaged_callback?.Invoke(m_last_damage);

        // Effect
        Vector3 start_pos;
        if (hit_position.magnitude <= 0)
        {
            start_pos = transform.position;
        }
        else
        {
            start_pos = hit_position;
        }

        //var damage_number = Instantiate(damageable_resources.floating_text, hit_position, Quaternion.identity, transform);
        var damage_number = Instantiate(damageable_resources.floating_text, hit_position, Quaternion.identity);
        TMP_Text tmp_text = damage_number.GetComponent<TMP_Text>();
        tmp_text.text = (damage * 10).ToString();
        tmp_text.color = damage_color;

        if (m_health <= 0.0f)
        {
            // Invoke callbacks
            death_callback?.Invoke();
            death_callback = null;

            // Check other death conditions
            if (!ShouldDie())
            {
                return;
            }

            if (damageable_resources.death_particles)
            {
                ParticleSystem death_effect = Instantiate(damageable_resources.death_particles, transform.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                Destroy(death_effect.gameObject, death_effect.main.duration);

                // De parent effects
                damage_number.transform.SetParent(death_effect.transform);
            }

            // ~ Clean up

            // Spring effect
            StopAllCoroutines();

            Destroy(gameObject);
        }
    }
    
    // ~ Status Flags Helpers
    public bool CheckStatus(StatusEffect status_flags){ return (m_status_effect & status_flags) == status_flags;}
    public void ApplyStatus(StatusEffect status_flags) 
    {
        m_status_effect |= status_flags;
        status_callback?.Invoke(status_flags);

        if ((status_flags & StatusEffect.Corrosion) == StatusEffect.Corrosion)
        {
            if (m_corossion_routine != null)
            {
                StopCoroutine(m_corossion_routine);
            }
            m_corossion_routine = StartCoroutine(CorossionEffect());
        }
    }
    public void RemoveStatus(StatusEffect status_flags) { m_status_effect &= ~status_flags; }

    // Returns whether damageable object wants to handle death
    protected virtual bool ShouldDie() { return true; }

    // ~ Damage Effects
    private IEnumerator ShortCircuitEffect()
    {
        // float start_speed = movement_speed;
        // float start_aglity = agility;

        // movement_speed *= 0.5f;

        // short_circuit_particles.Play();

        yield return new WaitForSeconds(2.0f);

        // short_circuit_particles.Stop();
        // movement_speed = start_speed;

        RemoveStatus(StatusEffect.ShortCircuit);

        m_short_circuit_routine = null;
    }
    private IEnumerator CorossionEffect()
    {
        float time = 0.0f;

        float interval_timer = 0.0f;
        while (time < 10.0f)
        {
            time += Time.deltaTime;
            interval_timer += Time.deltaTime;

            if (interval_timer > 0.5f)
            {
                TakeDamage(2.5f, StatusEffect.Corrosion);
                interval_timer = 0.0f;
            }

            yield return null;
        }

        RemoveStatus(StatusEffect.Corrosion);
        m_short_circuit_routine = null;
    }
    private IEnumerator DefaultDamageEffect()
    {
        if(m_start_material == null)
        {
            m_start_material = damageable_renderer.material;
        }

        damageable_renderer.material = damageable_resources.damaged_material;
        yield return new WaitForSeconds(damageable_resources.damageable_duration);
        damageable_renderer.material = m_start_material;

        m_damage_effect_routine = null;

        FMODUnity.RuntimeManager.PlayOneShotAttached(on_damage_sfx, gameObject);
    }
    private IEnumerator SpringDampEffect()
    {
        float start_displacement = damageable_renderer.transform.localScale.y;
        float displacement = 0.0f;

        m_spring_velocity = damageable_resources.start_velocity;

        Vector3 deformed_scale;
        while (true)
        {
            float force = -damageable_resources.spring * displacement - damageable_resources.damp * m_spring_velocity;
            m_spring_velocity += force * Time.deltaTime;
            displacement += m_spring_velocity * Time.deltaTime;

            deformed_scale = Vector3.one * (start_displacement + displacement);
            damageable_renderer.transform.localScale = deformed_scale;

            yield return null;
        }
    }

    // ~ Status
    [SerializeField]
    private StatusEffect m_status_effect = StatusEffect.None;
    Material m_start_material;

    // ~ Effect Coroutines
    private Coroutine m_short_circuit_routine = null;
    private Coroutine m_corossion_routine = null;

    // ~ Effects
    float m_spring_velocity = 0.0f;
    private Coroutine m_spring_effect_routine = null;

    // ~ State 
    public float m_health = 100.0f;
    private Coroutine m_damage_effect_routine = null;
    private float m_last_damage = 0;
}
