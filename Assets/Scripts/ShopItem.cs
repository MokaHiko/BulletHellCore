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
    [SerializeField]
    ItemType type;

    [SerializeField]
    public Merc merc_prefab;

    [SerializeField]
    public ModifierAttributes attributes;

    [SerializeField]
    public WeaponModifiers modifier_flags;

    [SerializeField]
    public ParticleSystem chosen_particles;

    public ItemChosenCallback item_chosen_callback;

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
                    Merc merc = Instantiate(merc_prefab, transform.position, Quaternion.identity);
                    leader.Party.AddMember(merc);
                }break;
                case ItemType.ModifierAttribute:
                {
                    leader.GetComponent<Unit>().EquipedWeapon.modifiers.Add(attributes);
                    GameManager.Instance.Reward(leader);
                }break;
                case ItemType.Modifier:
                {
                    // TODO: Find merc with corresponding weapon type
                    Modifier modifier = leader.GetComponent<Unit>().EquipedWeapon.gameObject.AddComponent<Modifier>();
                    modifier.Equip(leader);
                    modifier.ApplyModifier(modifier_flags);

                    GameManager.Instance.Reward(leader);
                }break;
                default:
                    break;
            }

            item_chosen_callback?.Invoke();

            Destroy(Instantiate(chosen_particles, transform.position, Quaternion.identity), chosen_particles.main.duration);
            Destroy(gameObject);
        }
    }

    private Room m_room;
}
