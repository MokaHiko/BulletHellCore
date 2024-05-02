using System.Collections;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitController))]
[RequireComponent(typeof(PlayerCombat))]
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

    //fmod variables
    private FMOD.Studio.EventInstance burstAudio;
    public FMODUnity.EventReference burstAudioReference;
    

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

    void Start()
    {
        // TODO: Find in game 

        // World handles
        Debug.Assert(Camera.main, "Game running without main camera!");
        Debug.Assert(virtual_camera != null);
        Debug.Assert(exp_bar != null);

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

        //fmod set instance
        burstAudio = FMODUnity.RuntimeManager.CreateInstance(burstAudioReference);
    }

    void Update()
    {
        // ~ HUD UI
        exp_bar.SetValue(experience, 100.0f);

        // ~ Movement

        // Move to input axis
        m_axis_input.x = Input.GetAxisRaw("Horizontal");
        m_axis_input.z = Input.GetAxisRaw("Vertical");

        Vector3 camera_forward = Camera.main.transform.forward;
        Vector3 camera_right = Camera.main.transform.right;

        camera_forward.y = 0;
        camera_right.y = 0;

        Vector3 relative_forward = m_axis_input.z * Vector3.Normalize(camera_forward);
        Vector3 relative_right = m_axis_input.x * Vector3.Normalize(camera_right);

        // Look at mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000.0f, targetable))
        {
            return;
        }

        // Rotate character
        if (!m_unit.CheckState(UnitState.ManagedMovement))
        {
            Vector3 look_at = hit.point;
            look_at.y = transform.position.y;
            transform.LookAt(look_at, Vector3.up);
        }

        // Dash
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            m_unit.UseAbility(AbilityType.Movement, IsBurst(), Vector3.Normalize(relative_forward + relative_right));
            AbortBurst();
        }
        else
        {
            // Move
            Vector3 target_pos = transform.position + Vector3.Normalize(relative_forward + relative_right);
            m_unit_controller.GoTo(target_pos);
        }

        // ~ Combat

        // Parry
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (RequestBurst() && m_unit.CheckState(UnitState.TakingDamage))
            {
                m_player_combat.Parry();
                AbortBurst();
            }
        }

        // Fire current weapon
        if (Input.GetMouseButton(0))
        {
            m_player_combat.Attack(hit.point);
        }
    }

    public void OnParticleEnter(ParticleType type)
    {
        switch (type) 
        {
            case (ParticleType.Exp):
            {
                experience = (experience + 1.0f) % 100;

                // TODO: Level up
                //experience = Mathf.Clamp(experience + 1.0f, 0.0f, 100.0f);
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

    private IEnumerator TriggerBurst()
    {
        burst_particle_system.Play();
        burst = true;
        GetComponentInChildren<Animator>().SetTrigger("burst");

        //fmod burst audio
        burstAudio.start();

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

        //stop fmod audio
        burstAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

    private Vector3 m_axis_input;

    // ~ Handles
    private Unit m_unit;
    private UnitController m_unit_controller;
    private PlayerCombat m_player_combat;

    // ~ Burst
    float m_camera_start_distance;
    private Coroutine m_burst_routine;
    private bool m_abort_burst;
}
