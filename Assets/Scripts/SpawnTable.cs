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

public class SpawnGroup
{
    public List<Unit> units;
};

[CreateAssetMenu(fileName = "SpawnTable", menuName = "SpawnTable")]
public class SpawnTable : ScriptableObject
{ 
    public new string name;
    public List<SpawnData> data;

    // Returns a list of units based of the weighted spawn table
    public List<SpawnGroup> Roll(float normalized_time, int n_groups = 1)
    {
        List<SpawnGroup> groups = new List<SpawnGroup>();

        for (int i = 0; i < n_groups; i++)
        {
            float roll = Random.Range(0, 1.0f);

            SpawnGroup group = new SpawnGroup();
            group.units = new List<Unit>();

            foreach (SpawnData spawnable in data)
            {
                // Check if in time range
                if (spawnable.normalized_wave_spawn_time > normalized_time) continue;

                // Check probablity
                if (spawnable.weight >= roll)
                {
                    group.units.AddRange(spawnable.spawnable_objects);
                }
            }

            groups.Add(group);
        }

        return groups;
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
