using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    [SerializeField]
    public Merc merc_prefab;

    [SerializeField]
    public ParticleSystem chosen_particles;

    [SerializeField]
    public ParticleSystem unchosen_particles;

    bool chosen = false;

    private void Start()
    {
        m_room = GetComponentInParent<Room>();

        m_room.room_complete_calblack += () =>
        {
            Instantiate(chosen ? chosen_particles : unchosen_particles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Merc>(out Merc leader))
        {
            if (leader.Party.party_leader.gameObject != leader.gameObject)
            {
                return;
            }

            Merc merc = Instantiate(merc_prefab, transform.position, Quaternion.identity);
            leader.Party.AddMember(merc);

            chosen = true;

            // Complete when shop item is chosen
            GetComponentInParent<Room>().Complete();
        }
    }

    private Room m_room;
}
