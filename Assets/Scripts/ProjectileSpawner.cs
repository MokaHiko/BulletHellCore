using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnerEmissionShape
{ 
    Circle,
    Targeted,
}

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Emission")]
    public float spawn_rate = 1f;
    public float radius = 2.0f;
    public SpawnerEmissionShape shape;

    private const float min = 1;
    private const float max = 10;

    [Header("Projectile")]
    [Range(min, max)]
    public float scale;
    public bool random_scale;

    public List<GameObject> projectile_types;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(spawn_rate > 0f);
        Debug.Assert(projectile_types.Count > 0);

        int steps = 6;
        float theta = 0.0f;

        float step_size = 2 * Mathf.PI / steps;
        for(int i = 0; i < steps; i++) 
        {
            m_points.Add(new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
            theta += step_size;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(time_elapsed > (1.0f / spawn_rate)) 
        { 
            foreach(Vector3 point in m_points)
            {
                var projectile = Instantiate(projectile_types[0], transform.position + (radius * point), Quaternion.LookRotation(point, Vector3.up));

                if (random_scale)
                {
                    float scale_value = Random.Range(min, scale);
                    projectile.transform.localScale *= scale_value;
                }
                projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * 15.0f;
            }

            time_elapsed = 0.0f;
        }

        transform.Rotate(new Vector3(0.0f, time_elapsed * (Mathf.PI / 2.0f), 0.0f));
        time_elapsed += Time.deltaTime;
    }

    // ~ Spawner
    private List<Vector3> m_points = new List<Vector3>();
    private float time_elapsed = 0;
}
