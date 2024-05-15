using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PartySlotLocation
{
    Up, 
    Down, 
    Left, 
    Right, 
    Center
}

[System.Serializable]
public class PartySlot
{
    public PartySlotLocation slot;
    public Transform location;
    public Merc merc;
};

public class PlayerController : MonoBehaviour
{
    [Header("Party")]
    [SerializeField]
    public Unit party_leader;
    [SerializeField]
    public List<PartySlot> party_slots;

    [Header("Camera")]
    public LayerMask targetable;
    public float target_radius = 1.0f;

    // Getters
    public Vector2 AxisInput {get{return m_axis_input;}}
    public Vector3 RelativeAxisInput {get{return m_relative_axis_input;}}
    public Vector3 WorldMousePoint {get{return m_world_mouse_point;}}
    public void AddMember(Merc merc)
    {
        // Find slot
        PartySlot free_slot = null;
        foreach (PartySlot slot in party_slots)
        {
            if (slot.merc == null)
            {
                free_slot = slot; 
                break;
            }
        }

        if (free_slot == null)
        {
            Debug.Log("Could not find slot. Party Full!");
            return;
        }

        // Assign merc
        free_slot.merc = merc;
        merc.Party = this;
        if (free_slot.slot == PartySlotLocation.Center)
        {
            party_leader = merc.GetComponent<Unit>();
        }
        
        // Instantiate weapon Icons
        foreach (Weapon weapon in merc.GetComponent<Unit>().weapons)
        {
            Instantiate(weapon.resource.weapon_icon, GameManager.Instance.GetPlayerHud().weapons_container.transform);
        }

        merc.GetComponent<Unit>().death_callback += () =>
        {
            PartySlot center_slot = null;
            if (merc.gameObject == party_leader.gameObject)
            {
                GameManager.Instance.GameOver();

                // TODO: Switch party leaders 
                foreach(PartySlot slot in party_slots) 
                { 
                    if (party_leader != null)
                    {
                        break;
                    }

                    if (slot.merc == merc && slot.slot == PartySlotLocation.Center)
                    {
                        center_slot = slot;
                        continue;
                    }

                    party_leader = slot.merc.GetComponent<Unit>();
                }

                // Reassign to center
                if (party_leader != null)
                {
                    center_slot.merc = party_leader.GetComponent<Merc>();
                }
                else
                {
                    // Lose
                    GameManager.Instance.GameOver();
                }
            }

            RemoveMember(merc);
        };
    }

    public void RemoveMember(Merc merc)
    {
        int index = -1;
        for(int i = 0; i <  party_slots.Count; i++) 
        { 
            if (party_slots[i].merc == merc)
            {
                index = i;
                break;
            }
        }

        if (index > 0)
        {
            party_slots.RemoveAt(index);
        }

        // TODO: Remove any weapon icons
    }

    private void Start()
    {
        Debug.Assert(party_leader != null, "Party must have leader!");
    }

    private void Update()
    {
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
        if (Physics.SphereCast(ray, target_radius, out RaycastHit hit, 1000.0f, targetable))
        {
            m_world_mouse_point = hit.point;
        }

        // Rotate party transforms
        Vector3 look_at = WorldMousePoint; 
        look_at.y = transform.position.y;
        transform.LookAt(look_at, Vector3.up);

        // Fire current weapon
        transform.position = new Vector3(party_leader.transform.position.x, transform.position.y, party_leader.transform.position.z);
        foreach(PartySlot slot in party_slots)  
        {
            if (slot.merc == null)
            {
                continue;
            }

            Merc merc = slot.merc;
            merc.transform.LookAt(look_at, Vector3.up);

            if (slot.merc.gameObject == party_leader.gameObject && m_axis_input.magnitude > 0)
            {
                merc.TargetPosition = merc.transform.position + RelativeAxisInput;
                merc.StateMachine.QueueAddState(merc.StateMachine.WalkState);
            }
            else if (m_axis_input.magnitude > 0|| Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                if ((merc.TargetPosition - slot.location.position).magnitude > 0.15f)
                {
                    merc.TargetPosition = slot.location.position;
                    merc.StateMachine.QueueAddState(merc.StateMachine.WalkState);
                }
            }

            if (Input.GetMouseButton(0))
            {
                merc.StateMachine.QueueAddState(merc.StateMachine.AttackState);
            }

            if (Input.GetMouseButtonDown((int)MouseButton.RightMouse) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                merc.StateMachine.QueueAddState(merc.StateMachine.EmpoweredMovementAbilityState);
            }
        }

        // Shield/Parry
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //m_player_combat.Reload();
        //}
    }

    public void OnRoomComplete()
    {
        StartCoroutine(IncreaseCollector(1000.0f, 5.0f, 1.0f));
    }
    private IEnumerator IncreaseCollector(float radius, float duration, float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);

        ParticleSystemForceField particle_field = party_leader.GetComponentInChildren<ParticleSystemForceField>();
        if (particle_field == null)
        {
            yield break;
        }

        float start_radius = particle_field.endRange;
        particle_field.endRange = radius;

        yield return new WaitForSeconds(duration);

        particle_field.endRange = start_radius;
    }

    // ~ Input 
    private Vector2 m_axis_input;
    private Vector3 m_relative_axis_input;
    private Vector3 m_world_mouse_point;
}
