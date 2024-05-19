using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


[CreateAssetMenu(fileName = "DashState", menuName = "UnitDashState")]
public class DashState : UnitAbilityState
{
    [SerializeField]
    public float dash_multiplier;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    [Header("Trail effect")]
    public float mesh_refresh_time = 0.1f;
    public Material mesh_trail_material;

    DashState()
    {
        m_exclusive = true;
        m_interruptable = false;
    }

    public override void OnEnter(Unit unit)
    {
        TimeElapsed = 0.0f;

        // Indicate managed movement
        if (skinned_meshes == null)
        {
            skinned_meshes = unit.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        UseWithCost(false, Direction.magnitude > 0.0f ? Direction : unit.transform.forward);
    }

    public override void Use(bool burst, Vector3 direction)
    {
        if (m_dash_routine != null)
        {

            Owner.StopCoroutine(m_dash_routine);
            m_dash_routine = null;
        }

        m_dash_routine = Owner.StartCoroutine(DashEffect(direction));
    }

    private IEnumerator DashEffect(Vector3 direction)
    {
        Owner.ApplyState(UnitStateFlags.ManagedMovement);
        Owner.GetComponent<Rigidbody>().AddForce(direction *  dash_multiplier, ForceMode.Impulse);

        float effect_timer = 0.0f;
        float mesh_refresh_interval_timer = 0.0f;
        while (effect_timer <= duration)
        {
            effect_timer += Time.deltaTime;
            mesh_refresh_interval_timer += Time.deltaTime;

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

        Owner.RemoveState(UnitStateFlags.ManagedMovement);
        m_dash_routine = null;
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
        unit.RemoveState(UnitStateFlags.ManagedMovement);
    }

    // ~ Trail Effect
    SkinnedMeshRenderer[] skinned_meshes;
    Coroutine m_dash_routine;
}

