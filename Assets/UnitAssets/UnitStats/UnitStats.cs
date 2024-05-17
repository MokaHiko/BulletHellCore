using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    public float health;
    public float max_energy;
    public float energy_gain_rate;
    public float movement_speed;
    public float damage_multiplier;
}
