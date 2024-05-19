using System.Collections;
using UnityEngine;

public delegate void ItemChosenCallback();

public enum ItemType
{
    Merc,
    ModifierAttribute,
    Modifier,
};

public class ShopItem : MonoBehaviour
{
    [Header("DropItem")]
    public ItemType type;
    public ParticleSystem chosen_particles;
    public ItemChosenCallback item_chosen_callback;
    public LayerMask clear_mask;
    public float clear_radius = 1.0f;

    [Header("Merc")]
    public Merc merc_prefab;
    public float duration = 10.0f;

    [Header("Attributes")]
    public ModifierAttributes attributes;

    [Header("Modifier")]
    public WeaponModifiers modifier_flags;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Merc>(out Merc leader))
        {
            if (leader.Party.party_leader.gameObject != leader.gameObject)
            {
                return;
            }

            switch(type)
            {
                case ItemType.Merc:
                {
                    leader.Party.AddMember(merc_prefab, duration);
                }break;
                case ItemType.ModifierAttribute:
                {
                    leader.GetComponent<Unit>().EquipedWeapon.AddModifier(attributes);
                }break;
                case ItemType.Modifier:
                {
                    Modifier modifier = leader.GetComponent<Unit>().EquipedWeapon.gameObject.AddComponent<Modifier>();
                    modifier.Equip(leader);
                    modifier.ApplyModifier(modifier_flags);
                }break;
                default:
                    break;
            }

            item_chosen_callback?.Invoke();

            // Clear projectiles 
            // TODO: Delay
            if(clear_radius > 0.0f) 
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, clear_radius, clear_mask);

                if (cols.Length > 0)
                {
                    GameManager.Instance.RequestSlowMo(0.25f);
                }

                foreach(Collider col in cols)
                {
                    Destroy(Instantiate(chosen_particles, col.transform.position, Quaternion.identity).gameObject, chosen_particles.main.duration);
                    col.GetComponent<Projectile>().Die();
                }
            }

            Destroy(Instantiate(chosen_particles, transform.position, Quaternion.identity).gameObject, chosen_particles.main.duration);
            Destroy(gameObject);
        }
    }


    private Room m_room;
}
