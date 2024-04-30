using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField]
    float spawn_interval = 0.0f;

    [SerializeField]
    float spawn_radius = 0.0f;

    [SerializeField]
    List<GameObject> units;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(units.Count > 0);
    }

    // Update is called once per frame
    void Update()
    {
        time_elapsed += Time.deltaTime;
        if (time_elapsed > spawn_interval)
        {
            int unit_index = Random.Range(0, units.Count);
            Vector3 location = transform.position + new Vector3(Random.Range(5, spawn_radius), 0, Random.Range(5, spawn_radius));
            Instantiate(units[unit_index], location, Quaternion.identity);
            time_elapsed = 0.0f;
        }
    }

    private float time_elapsed = 0.0f;
}

