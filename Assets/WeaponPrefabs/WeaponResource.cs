using UnityEngine;

[CreateAssetMenu(fileName = "WeaponResource", menuName = "WeaponResource")]
public class WeaponResource : ScriptableObject
{
    [SerializeField]
    public WeaponUI weapon_icon;

    [SerializeField]
    public float base_damage;

    [SerializeField]
    public float attack_speed = 5f;

    [SerializeField]
    public float scan_radius = 1.0f;

    [SerializeField]
    public float range = 1000.0f;

    [SerializeField]
    public int max_bullets = 32;

    [SerializeField]
    public float reload_time = 1.0f;

    [SerializeField]
    public float hold_threshold = 1.0f;
}
