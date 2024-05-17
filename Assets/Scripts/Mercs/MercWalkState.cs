using UnityEngine;


[CreateAssetMenu(fileName = "MercWalkState", menuName = "MercWalkState")]
public class MercWalkState : UnitWalkState
{
    [SerializeField]
    float threshold = 0.25f;

    public override void OnEnter(Unit unit)
    {
        m_merc = unit.GetComponent<Merc>();
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        if (m_merc == null || m_merc.Party == null)
        {
            Debug.Log("Merc or Merc Party unassigned");
            return;
        }

        Vector3 target_pos = m_merc.TargetPosition;
        if ((unit.transform.position - target_pos).magnitude < threshold)
        {
            StateMachine.QueueRemoveState(this);
            return;
        }

        // Move
        GoTo(unit, target_pos);
    }

    Merc m_merc;
}
