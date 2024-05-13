using System.Collections;
using System.ComponentModel;
using UnityEngine;

public delegate void OnTeleportReceived();

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    public Teleporter destination;

    [SerializeField]
    public float teleport_duration = 1.0f;

    [Header("Status")]
    [SerializeField]
    private Transform receieving;
    [SerializeField]
    private Teleporter sender;
    [SerializeField]
    private Transform sending;

    public OnTeleportReceived teleport_received_callback;

    IEnumerator TeleportEffect(Transform unit_transform, float duration)
    {
        //yield return new WaitForSeconds(duration);
        // Send entire party

        unit_transform.position = destination.transform.position;
        if (unit_transform.TryGetComponent<Merc>(out Merc leader))
        {
            foreach(PartySlot slot in leader.Party.party_slots)
            {
                if (slot.merc == null) continue;
                if (slot.merc == leader) continue;
                slot.merc.transform.position = destination.transform.position;
            }
        }

        // Sent
        sending = null;
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Merc>(out Merc merc))
        {
            if (merc.Party.party_leader != merc.GetComponent<Unit>())
            {
                return;
            }

            // Check if already sending
            if(merc.transform == sending) 
            {
                return;
            }

            if(merc.transform == receieving) 
            {
                Receieve(merc.transform);
                return;
            }

            Send(merc.transform);
        }
    }

    void Send(Transform transform)
    {
        // Check if only receiving telorter
        if (destination == null)
        {
            GetComponent<Renderer>().material.color = Color.black;
            return;
        }

        Debug.Assert(transform != null);

        sending = transform;
        destination.sender = this;
        destination.receieving = transform;
        StartCoroutine(TeleportEffect(transform, teleport_duration));
        //Debug.Log($"Sent {transform.position} successefully!");
    }

    void Receieve(Transform transform)
    {
        Debug.Assert(transform != null);
        if (receieving == transform)
        {
            teleport_received_callback?.Invoke();
            //Debug.Log($"Recieved {transform.position} successefully!");

            // Notify sender
            receieving = null;
            sender = null;
        }
    }
}
