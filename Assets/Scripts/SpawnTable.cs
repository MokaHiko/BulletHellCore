using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnData
{
    public List<Unit> spawnable_objects;

    [Header("Spawn")]
    [Range(0f, 1f)] public float weight;
    [Range(0f, 1f)] public float normalized_wave_spawn_time;

    [Header("Drops")]
    public GameObject reward;
    [Range(0f, 1f)] public float drop_chance;
};

[CreateAssetMenu(fileName = "SpawnTable", menuName = "SpawnTable")]
public class SpawnTable : ScriptableObject
{ 
    public new string name;
    public List<SpawnData> data;

    // Returns a list of units based of the weighted spawn table
    public List<Unit> Roll(float normalized_time)
    {
        float roll = Random.Range(0, 1.0f);

        List<Unit> interval_units = new List<Unit>();
        foreach (SpawnData spawnable in data)
        {
            // Check if in time range
            if (spawnable.normalized_wave_spawn_time > normalized_time) continue;

            // Check probablity
            if (spawnable.weight >= roll)
            {
                interval_units.AddRange(spawnable.spawnable_objects);
            }
        }

        return interval_units;
    }

    // Normalizes spawn table
    public void Normalize()
    {
        // Find sum 
        float sum = 0.0f;
        foreach(SpawnData spawnable in data)
        {
            sum += spawnable.weight;
        }

        // Normalize
        Debug.Assert(sum !=  0.0f);
        for(int i = 0; i < data.Count; i++)
        {
            SpawnData normalized_data = new SpawnData();
            normalized_data.spawnable_objects = data[i].spawnable_objects;
            normalized_data.weight = data[i].weight / sum;
            normalized_data.normalized_wave_spawn_time = data[i].normalized_wave_spawn_time;

            data[i] = normalized_data;
        }
    }
};
