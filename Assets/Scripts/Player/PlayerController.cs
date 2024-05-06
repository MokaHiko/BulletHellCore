using System.Collections;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.UIElements;

public delegate void PlayerLevelUpBack();

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(CameraTarget))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float experience = 0.0f;

    [SerializeField]
    private int credits = 0;

    [Header("HUD UI")]
    public PropertyBar exp_bar;

    [Header("Camera")]
    public LayerMask targetable;
    public float target_radius = 1.0f;

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
    CinemachineComponentBase cm_component_base;

    [SerializeField]
    UnitStateMachine unit_state_machine;

    // Getters
    public Vector2 AxisInput {get{return m_axis_input;}}
    public Vector3 RelativeAxisInput {get{return m_relative_axis_input;}}
    public Vector3 WorldMousePoint {get{return m_world_mouse_point;}}

    // Player events
    public PlayerLevelUpBack player_level_up_callback;

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

    public void RequestShake(float intensity, float time)
    {
        if (m_shake_routine != null)
        {
            StopCoroutine(m_shake_routine);
            m_shake_routine = null;
        }

        m_shake_routine = StartCoroutine(ShakeEffect(intensity, time));
    }

    void Start()
    {
        // TODO: Find in game 

        // World handles
        exp_bar = GameManager.Instance.GetPlayerHud().exp_bar;
        Debug.Assert(exp_bar != null, "Player no exp bar!");

        // Handles
        m_unit = GetComponent<Unit>();
        m_unit_controller = GetComponent<UnitMovement>();
        m_player_combat = GetComponent<PlayerCombat>();

        // Effects
        if (cm_component_base == null)
        {
            var virtual_camera = GameManager.Instance.GetVirtualCamera();
            cm_component_base = virtual_camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }
        m_camera_start_distance = (cm_component_base as CinemachineFramingTransposer).m_CameraDistance;

        // Subscribe to callbacks
        m_unit.damaged_callback += (float damage) => { AbortBurst(); };
        m_unit.damaged_callback += (float damage) => { RequestShake(5, 0.5f); };
        m_unit.death_callback += () => { RequestShake(0, 0.0f); };
        m_unit.status_callback += OnStatusEffect;
    }

    void Update()
    {
        // ~ HUD UI
        exp_bar.SetValue(experience, 100.0f);

        // Input axis
        m_axis_input.x = Input.GetAxisRaw("Horizontal");
        m_axis_input.y = Input.GetAxisRaw("Vertical");

        Vector3 camera_forward = Camera.main.transform.forward;
        Vector3 camera_right = Camera.main.transform.right;

        camera_forward.y = 0;
        camera_right.y = 0;

        Vector3 relative_forward = m_axis_input.y * Vector3.Normalize(camera_forward);
        Vector3 relative_right = m_axis_input.x * Vector3.Normalize(camera_right);
        m_relative_axis_input = (relative_forward + relative_right).normalized;

        // Look at mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.SphereCast(ray, target_radius,out RaycastHit hit, 1000.0f, targetable))
        {
            m_world_mouse_point = hit.point;
        }

        // Fire current weapon
        if (Input.GetMouseButton(0))
        {
            unit_state_machine.QueueAddState(unit_state_machine.AttackState);
        }

        return;

        // Rotate character
        if (!m_unit.CheckState(UnitStateFlags.ManagedMovement))
        {
            // Vector3 look_at = hit.point;
            // look_at.y = transform.position.y;
            // transform.LookAt(look_at, Vector3.up);
        }

        // Dash
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            // m_unit.UseAbility(AbilityType.Movement, IsBurst(), Vector3.Normalize(relative_forward + relative_right));
            // AbortBurst();
        }
        else
        {
        }

        // // ~ Combat

        // // Parry
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (RequestBurst() && m_unit.CheckState(UnitStateFlags.TakingDamage))
        //     {
        //         m_player_combat.Parry();
        //         AbortBurst();
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     m_player_combat.Reload();
        // }

        // // Fire current weapon
        // if (Input.GetMouseButton(0))
        // {
        //     m_player_combat.Attack(hit.point);
        // }
    }
    public void OnRoomComplete()
    {
        StartCoroutine(IncreaseCollector(1000.0f, 5.0f, 1.0f));
    }

    private IEnumerator IncreaseCollector(float radius, float duration, float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);

        ParticleSystemForceField particle_field = GetComponentInChildren<ParticleSystemForceField>();

        float start_radius = particle_field.endRange;
        particle_field.endRange = radius;

        yield return new WaitForSeconds(duration);

        particle_field.endRange = start_radius;
    }

    public void OnParticleEnter(ParticleType type)
    {
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
            case (ParticleType.Money):
            {
                credits += 1;
            }break;
            case (ParticleType.Energy):
            {
                m_unit.energy += 1.0f;
            }break;
            default:
                break;
        }
    }

    private void LevelUp()
    {
        player_level_up_callback?.Invoke();
    }

    private IEnumerator TriggerBurst()
    {
        burst_particle_system.Play();
        burst = true;
        GetComponentInChildren<Animator>().SetTrigger("burst");

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

    // Effects
    IEnumerator ShakeEffect(float intensity, float shake_time)
    {
        var cinemachine_perlin = GameManager.Instance.GetVirtualCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachine_perlin.m_AmplitudeGain = intensity;

        float time = 0.0f;
        while(time < shake_time) 
        {
            time += Time.deltaTime;
            yield return null;
        }

        cinemachine_perlin.m_AmplitudeGain = 0;
        m_shake_routine = null;
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

    private Vector2 m_axis_input;
    private Vector3 m_relative_axis_input;
    private Vector3 m_world_mouse_point;

    // ~ Handles
    private Unit m_unit;
    private UnitMovement m_unit_controller;
    private PlayerCombat m_player_combat;

    // ~ Burst
    float m_camera_start_distance;
    private Coroutine m_burst_routine;
    private bool m_abort_burst;

    // ~ Effects
    Coroutine m_shake_routine;
    float m_current_shake;

}
