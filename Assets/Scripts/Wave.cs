using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public delegate void WaveCompleteCallback(Wave wave);


public class WaveGroup
{

};

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    [Header("Spawn Effects")]
    public SpawnEffectHandler spawn_effect_prefab;

    public void Init(Spawner spawner)
    {
        Debug.Assert(m_spawner == null, "Wave already initialized");
        Debug.Assert(table != null);

        m_spawner = spawner;

        // Init unit list
        unit_interval_spawns = new List<List<Unit>>();

        // Pre-Roll for all intervals
        m_total_intervals = Mathf.FloorToInt(wave_time / spawn_interval);
        for (int i = 0; i < m_total_intervals; i++)
        {
            // Roll units
            List<Unit> units_roll = table.Roll((spawn_interval * i) / wave_time);
            if (units_roll != null && units_roll.Count > 0)
            {
                unit_interval_spawns.Add(units_roll);
                m_total_units += units_roll.Count;
            }
            else
            {
                Debug.Log("HU OH at interval: " + i);
            }

            // Roll hazards
        }

        Debug.Log($"Intervals: {m_total_intervals} with {unit_interval_spawns.Count} unit spawns");
    }

    public bool IsComplete 
    {   get 
        {
            return m_complete;
        } 
    }

    public void OnFrameTick(float dt)
    {
        Debug.Assert(m_spawner != null, "Wave was not initialized with a spawner!");

        if (IsComplete)
        {
            Debug.Log("Wave already complete!");
            return;
        }

        if (m_interval >= m_total_intervals)
        {
            return;
        }

        // Update wave time
        m_total_time += Time.deltaTime;

        // Spawn interval
        m_interval_timer += dt;
        if(m_interval_timer > spawn_interval) 
        {
            Debug.Log("Interval: " + m_interval);
            SpawnUnits();

            m_interval += 1;
            m_interval_timer = 0.0f;
        }
    }

    void SpawnUnits()
    {
        if(IsComplete)
        {
            Debug.Log("Wave Complete!");
        }

        SpawnEffectHandler spawn_effect = GameObject.Instantiate(spawn_effect_prefab, m_spawner.transform.position, Quaternion.identity);

        foreach (Unit spawnable_unit in unit_interval_spawns[m_interval])
        {
            Unit unit = GameObject.Instantiate(spawnable_unit);

            unit.death_callback += () =>
            {
                m_dead_units++;

                // Complete when in final interval and all units dead
                if(m_total_units == m_dead_units)
                {
                    m_complete = true;
                }
            };

            spawn_effect.spawn_objects.Add(unit.gameObject);
        }

        spawn_effect.LoadObjects();
    }

    public float wave_time = 60.0f;
    public float spawn_interval = 10.0f;
    public SpawnTable table;

    private List<List<Unit>> unit_interval_spawns;

    private int m_total_units = 0;
    private int m_dead_units = 0;

    private int m_interval = 0;
    private int m_total_intervals = 0;
    private float m_interval_timer = 0.0f;

    private float m_total_time = 0.0f;

    private Spawner m_spawner;
    private bool m_complete;
};


