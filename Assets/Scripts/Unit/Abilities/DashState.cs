using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "DashState", menuName = "UnitDashState")]
public class DashState : UnitAbilityState
{
    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    [Header("Trail effect")]
    public float mesh_refresh_time = 0.3f;
    public Material mesh_trail_material;
    public FMODUnity.EventReference dash_audio;

    DashState()
    {
        //m_exclusive = true;
        //m_interruptable = false;
    }

    public override void OnEnter(Unit unit)
    {
        TimeElapsed = 0.0f;

        // Indicate managed movement
        if (skinned_meshes == null)
        {
            start_size = GameManager.Instance.GetVirtualCamera().m_Lens.OrthographicSize;
            skinned_meshes = unit.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        // Effects
        GameManager.Instance.ZoomTo(1.5f, start_size, start_size / 2.0f);
        GameManager.Instance.SetChromoaticAbberation(1.0f);
        FMODUnity.RuntimeManager.PlayOneShotAttached(dash_audio, unit.gameObject);

        // Slow time
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        if (m_dash_routine != null)
        {
            unit.StopCoroutine(m_dash_routine);
        }
        m_dash_routine = unit.StartCoroutine(DashEffect());
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        if(!Input.GetMouseButton((int)MouseButton.RightMouse)) 
        { 
            StateMachine.QueueRemoveState(this);
        }

        Owner.energy -= 1.0f;
        if (Owner.energy < 1.0f)
        {
        }
    }
    private IEnumerator DashEffect()
    {
        float mesh_refresh_interval_timer = 0.0f;
        while(true)
        {
            mesh_refresh_interval_timer += Time.deltaTime;
            Owner.energy -= 0.2f;

            if (mesh_refresh_interval_timer > mesh_refresh_time)
            {
                mesh_refresh_interval_timer = 0.0f;
                for(int i = 0; i < skinned_meshes.Length; i++) 
                { 
                    GameObject trail_object = new GameObject();
                    trail_object.transform.SetPositionAndRotation(Owner.transform.position, Owner.transform.rotation);

                    MeshRenderer mesh_renderer = trail_object.AddComponent<MeshRenderer>();
                    mesh_renderer.material = mesh_trail_material;

                    MeshFilter mesh_filter = trail_object.AddComponent<MeshFilter>();

                    Mesh trail_mesh = new Mesh();
                    skinned_meshes[i].BakeMesh(trail_mesh);

                    mesh_filter.mesh = trail_mesh;

                    Owner.StartCoroutine(AnimateMaterialFloat(mesh_renderer.material, 0, 0.1f, mesh_refresh_time));
                    Unit.Destroy(trail_object, 1.0f);
                }
            }

            yield return null;
        }
    }

    IEnumerator AnimateMaterialFloat(Material material, float end, float rate, float refresh_rate)
    {
        float value = material.GetFloat("Alpha");
        while (value > end)
        {
            value -= rate;
            material.SetFloat("Alpha", value);
            yield return new WaitForSeconds(refresh_rate);
        }
    }

    public override void OnExit(Unit unit)
    {
        if (m_dash_routine != null)
        {
            unit.StopCoroutine(m_dash_routine);
            m_dash_routine = null;
        }

        // Reset time
        Time.timeScale = 1.0f;

        // Reset Effects
        GameManager.Instance.ZoomTo(1.5f, start_size/2.0f, start_size);
        GameManager.Instance.SetChromoaticAbberation(0.15f);

        unit.RemoveState(UnitStateFlags.ManagedMovement);
    }

    // ~ Trail Effect
    SkinnedMeshRenderer[] skinned_meshes;
    Coroutine m_dash_routine;

    private float start_size = 0.0f;
}

