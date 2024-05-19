using UnityEngine;

[CreateAssetMenu(fileName = "MercAttackState", menuName = "MercAttackState")]
public class MercAttackState : UnitAttackState
{
    [SerializeField]
    public float target_radius = 1.0f;

    [SerializeField]
    public LayerMask targetable;

    [SerializeField]
    float time_held = 0.0f;

    public override void OnEnter(Unit unit)
    {
        m_merc = unit.GetComponent<Merc>();
        m_equiped_weapon = unit.EquipedWeapon;

    }

    public override void OnExit(Unit unit)
    {
        unit.movement_speed = unit.BaseStats.movement_speed;
    }

    // Attack and target location
    public override void Attack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            m_equiped_weapon.Attack(world_location);
        }
    }

    public override void AltAttack(Vector3 world_location)
    {
        if(m_equiped_weapon != null)
        {
            // Alt attacks are merc specials
            m_equiped_weapon.Attack(world_location, true);
        }
    }

    void Release()
    {
        if(time_held >= m_equiped_weapon.Stats.hold_threshold) 
        {
            AltAttack(m_merc.Party.WorldMousePoint);
        }
        time_held = 0.0f;
    }

    public override void OnFrameTick(Unit unit, float dt)
    {
        // Fire current weapon
        if(Input.GetMouseButton(0)) 
        {
            time_held += Time.deltaTime;
            Attack(m_merc.Party.WorldMousePoint);
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Release();
            return;
        }

        StateMachine.QueueRemoveState(this);
    }

    // ~ Weapons
    [SerializeField]
    private Weapon m_equiped_weapon;

    // ~ Handles
    private Merc m_merc;
}
