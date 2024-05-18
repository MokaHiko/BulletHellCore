using System.Collections;
using UnityEngine;
using UnityEngine.VFX;


[CreateAssetMenu(fileName = "BlinkState", menuName = "UnitBlinkState")]
public class BlinkState : UnitAbilityState
{
    [SerializeField]
    TrailRenderer trail_renderer;

    [SerializeField]
    public float teleport_distance;

    [SerializeField]
    public LayerMask none_passable;

    [SerializeField]
    public VisualEffect teleport_effect;

    [SerializeField]
    public StatusEffect dash_status = StatusEffect.None;

    BlinkState() 
    { 
        //m_exclusive = true;
    }

    public override void OnEnter(Unit unit)
    {
        TimeElapsed = 0.0f;
        UseWithCost(true, Direction);
    }

    public override void Use(bool burst, Vector3 direction)
    {
        Owner.StartCoroutine(BlinkEffect());
    }

    IEnumerator BlinkEffect()
    {
        float time = 0.0f;
        Vector3 dir = Owner.GetComponent<Merc>().Party.RelativeAxisInput;
        Vector3 target_position = Owner.transform.position + dir * teleport_distance;
        //Vector3 target_position = Owner.transform.position + Owner.transform.forward * teleport_distance;

        // Do not pass through
        //if (Physics.Raycast(Owner.transform.position, Owner.transform.forward, out RaycastHit hit, teleport_distance, none_passable))
        if (Physics.Raycast(Owner.transform.position, dir, out RaycastHit hit, teleport_distance, none_passable))
        {
            target_position = hit.point;
        }

        Owner.damageable_renderer.enabled = false;
        {
            VisualEffect avatar = Instantiate(teleport_effect, Owner.transform);
            while (time < duration / 2.0f)
            {
                time += Time.deltaTime;
                yield return null;
            }
            Destroy(avatar.gameObject, 1.0f);
        }

        {
            VisualEffect avatar = Instantiate(teleport_effect, target_position, Owner.transform.rotation);
            while (time < duration)
            {
                time += Time.deltaTime;
                yield return null;
            }
            Destroy(avatar.gameObject, 1.0f);
        }

        Owner.damageable_renderer.enabled = true;
        Owner.transform.position = target_position;
    }

    public override void OnExit(Unit unit)
    {
        Owner.movement_speed = Owner.BaseStats.movement_speed;
    }
}


