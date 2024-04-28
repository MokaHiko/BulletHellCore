using System.Collections;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitController))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    [Header("HUD UI")]
    public EnergyBar energy_bar;

    [Header("Burst")]
    [SerializeField]
    public float burst_zoom = 10.0f;

    [SerializeField]
    public ParticleSystem burst_particle_system;

    [SerializeField]
    public bool burst = false;

    [SerializeField]
    public float burst_cost = 25.0f;

    [SerializeField]
    public float burst_duration = 0.75f;

    [SerializeField]
    CinemachineVirtualCamera virtual_camera;
    CinemachineComponentBase cm_component_base;

    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public float dash_duration;

    [SerializeField]
    public float dash_cost = 15.0f;

    public bool IsBurst()
    {
        return burst;
    }

    // Returns whether burst was triggered succesefully
    public bool RequestBurst()
    {
        // Check if already in burst
        if(m_burst_routine != null) 
        {
            return false;
        }

        // Spend energy on burst
        if (m_unit.SpendEnergy(burst_cost))
        {
            m_burst_routine = StartCoroutine(nameof(TriggerBurst));
            return true;
        }
        return false;
    }

    public void Burst()
    {
        if (m_burst_routine != null)
        {
            StopCoroutine(m_burst_routine);
            m_burst_routine = null;
        }

        m_burst_routine = StartCoroutine(nameof(TriggerBurst));
    }

    public void Dash()
    {
        bool with_burst = false;
        if (with_burst = IsBurst())
        {
            // Add modifier to dash
            AbortBurst();
        }

        if (m_dash_routine != null)
        {
            StopCoroutine(m_dash_routine);
            m_dash_routine = null;
        }

        m_dash_routine = StartCoroutine(DashEffect(with_burst));
    }

    void Start()
    {
        // TODO: Find in game 
        // World handles
        Debug.Assert(virtual_camera != null);
        Debug.Assert(energy_bar != null);

        // Handles
        m_unit = GetComponent<Unit>();
        m_unit_controller = GetComponent<UnitController>();
        m_player_combat = GetComponent<PlayerCombat>();

        // Effects
        if (cm_component_base == null)
        {
            cm_component_base = virtual_camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }
        m_camera_start_distance = (cm_component_base as CinemachineFramingTransposer).m_CameraDistance;

        // Subscribe to callbacks
        m_unit.damaged_callback += (float damage) => { AbortBurst(); };
        m_unit.status_callback += OnStatusEffect;
    }
    void Update()
    {
        // ~ HUD UI
        if (m_unit.CheckStatus(StatusEffect.ShortCircuit))
        {
            energy_bar.SetValue(-1.0f);
        }
        else
        {
            energy_bar.SetValue(m_unit.energy);
        }

        // ~ Combat
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (RequestBurst() && m_unit.CheckState(UnitState.TakingDamage))
            {
                m_player_combat.Perry();
                AbortBurst();
            }

            return;
        }

        // Fire current weapon
        if (Input.GetMouseButton(0))
        {
            m_player_combat.Fire();
        }

        // ~ Movement

        // Look at mouose position
        Debug.Assert(Camera.main, "Game running without main camera!");
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 look_at = hit.point;
            look_at.y = transform.position.y;

            transform.LookAt(look_at, Vector3.up);
        }

        // Move to input axis
        m_axis_input.x = Input.GetAxisRaw("Horizontal");
        m_axis_input.z = Input.GetAxisRaw("Vertical");
        if (m_axis_input.magnitude == 0)
        {
            return;
        }

        Vector3 camera_forward = Camera.main.transform.forward;
        Vector3 camera_right = Camera.main.transform.right;

        camera_forward.y = 0;
        camera_right.y = 0;

        Vector3 relative_forward = m_axis_input.z * camera_forward;
        Vector3 relative_right = m_axis_input.x * camera_right;

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(m_dash_routine != null) 
            {
                // TODO: Queue Ability
                return;
            }

            if (m_unit.SpendEnergy(dash_cost))
            {
                Dash();
                return;
            }
        }

        // Move
        Vector3 target_pos = transform.position + Vector3.Normalize(relative_forward + relative_right);
        m_unit_controller.GoTo(target_pos);
    }
    private IEnumerator TriggerBurst()
    {
        burst_particle_system.Play();
        burst = true;

        // Slow time
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        float start_distance = m_camera_start_distance;
        float end_distance = start_distance - burst_zoom;

        float time = 0.0f;
        while (time < burst_duration)
        {
            (cm_component_base as CinemachineFramingTransposer).m_CameraDistance = Mathf.Lerp(start_distance, end_distance, time / burst_duration);
            time += (1.0f / burst_duration) * Time.unscaledDeltaTime;
            
            // Early exit on burst ability
            if(m_abort_burst) 
            {
                m_abort_burst = false;
                break;
            }

            yield return null;
        }

        (cm_component_base as CinemachineFramingTransposer).m_CameraDistance = start_distance;

        // Reset time
        Time.timeScale = 1.0f;
        burst = false;
        m_burst_routine = null;
    }

    private void OnStatusEffect(StatusEffect status_effect_flags)
    {
        if ((status_effect_flags & StatusEffect.ShortCircuit) == StatusEffect.ShortCircuit)
        {
        }
    }
    public void AbortBurst()
    {
        if(m_burst_routine != null)
        {
            m_abort_burst = true;
        }
    }

    private IEnumerator DashEffect(bool burst = false)
    {
        float start_speed = m_unit.movement_speed;
        float start_agility = m_unit.agility;

        m_unit.movement_speed =  start_speed * dash_multiplier;
        m_unit.agility = start_agility * dash_multiplier;

        if (burst)
        {
            m_player_combat.Perry();
            m_unit.ApplyStatus(StatusEffect.Armored);
        }

        yield return new WaitForSeconds(dash_duration);

        m_unit.RemoveStatus(StatusEffect.Armored);
        m_unit.movement_speed = start_speed;
        m_unit.agility = start_agility;

        m_dash_routine = null;
    }

    private Vector3 m_axis_input;

    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
    private PlayerCombat m_player_combat;

    // ~ Burst
    float m_camera_start_distance;
    private Coroutine m_burst_routine;
    private bool m_abort_burst;

    // ~ Movement Abilities
    private Coroutine m_dash_routine;
}
