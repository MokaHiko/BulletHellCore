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
    public int burst_count = 0;
    public float burst_rate = 1;
    public bool burst = false;

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

        PlayerController player = FindObjectOfType<PlayerController>();
        if(player != null ) 
        { 
            m_target = FindObjectOfType<PlayerController>().transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (time_elapsed > (1.0f / spawn_rate))
        {
            time_elapsed = 0.0f;
            switch (shape)
            {
                case (SpawnerEmissionShape.Targeted):
                {
                    if (burst)
                    {
                        StartCoroutine(BurstFire());
                    }
                    else
                    {
                        Targeted();
                    }
                }break;
                case(SpawnerEmissionShape.Circle):
                {
                    if (burst)
                    {
                        StartCoroutine(BurstFire());
                    }
                    else
                    {
                        Circle();
                    }
                }break;
                default:
                    break;
            }
        }

          time_elapsed += Time.deltaTime;
    }

    void Circle()
    {
        foreach (Vector3 point in m_points)
        {
            float scale_value = scale;
            if (random_scale)
            {
                scale_value = Random.Range(min, scale);
            }

            Vector3 rotated_point = Vector3.Normalize(transform.rotation * point);

            int projectile_type_index = Random.Range(0, projectile_types.Count);
            var projectile = Instantiate(projectile_types[projectile_type_index], transform.position + rotated_point + Vector3.up, Quaternion.identity);
            projectile.transform.localScale *= scale_value;

            //projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * speed;
            projectile.GetComponent<Rigidbody>().velocity = rotated_point * speed;
        }

        time_elapsed = 0.0f;
        if (random_spawn_rate)
        {
            time_elapsed = Random.Range(0, (1.0f / spawn_rate) * 0.5f);
        }
    }
    void Targeted()
    {
        if (m_target == null)
        {
            return;
        }

        int projectile_type_index = Random.Range(0, projectile_types.Count);
        var projectile = Instantiate(projectile_types[projectile_type_index], transform.position + Vector3.up, Quaternion.identity);

        projectile.transform.localScale *= scale;
        projectile.transform.LookAt(new Vector3(m_target.position.x, scale, m_target.position.z));

        Vector3 dir = projectile.transform.forward;
        dir.y = 1.0f;
        dir.Normalize();

        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * speed;
    }

    IEnumerator BurstFire()
    {
        float time = 0.0f;
        for (int i = 0; i < burst_count;)
        {
            if (time > (1.0f / burst_rate))
            {
                switch (shape)
                {
                    case (SpawnerEmissionShape.Targeted):
                    {
                        Targeted();
                    }break;
                    case(SpawnerEmissionShape.Circle):
                    {
                        Circle();
                    }break;
                    default:
                        break;
                }
                i++;
                time = 0.0f;
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    // ~ Spawner
    private List<Vector3> m_points = new List<Vector3>();
    private float time_elapsed = 0;

    private Transform m_target;
}
