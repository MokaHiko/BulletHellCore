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
    public bool random_spawn_rate = false;
    public float radius = 1.0f;
    public int steps = 20;
    public SpawnerEmissionShape shape;

    private const float min = 1;
    private const float max = 10;

    [Header("Projectile")]
    [Range(min, max)]
    public float scale;
    public bool random_scale;
    public float speed;

    public List<GameObject> projectile_types;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(spawn_rate > 0f);
        Debug.Assert(projectile_types.Count > 0);

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
                float scale_value = scale;
                if (random_scale)
                {
                    scale_value = Random.Range(min, scale); 
                }

                Vector3 rotated_point = Vector3.Normalize(transform.rotation * point);

                int projectile_type_index = Random.Range(0, projectile_types.Count);
                var projectile = Instantiate(projectile_types[projectile_type_index],transform.position + rotated_point, Quaternion.identity);
                projectile.transform.localScale *= scale_value;

                //projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * speed;
                projectile.GetComponent<Rigidbody>().velocity = rotated_point * speed;
            }

            time_elapsed = 0.0f;
            if(random_spawn_rate)
            {
                time_elapsed = Random.Range(0, (1.0f / spawn_rate) * 0.5f);
            }
        }

        transform.Rotate(new Vector3(0.0f, Mathf.PI * 100, 0.0f) * Time.deltaTime);
        time_elapsed += Time.deltaTime;
    }

    // ~ Spawner
    private List<Vector3> m_points = new List<Vector3>();
    private float time_elapsed = 0;
}
