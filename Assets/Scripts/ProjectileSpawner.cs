using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum SpawnerEmissionShape
{ 
    None = 0,
    Targeted = 1 << 0,
    Circle = 1 << 1,
    Spiral = 1 << 2,

    ShapedProjectile = 1 << 3,
}

[System.Serializable]
public class SpiralParameters
{
    [Header("Shape")]
    public float angle = 180.0f;
    public float radius = 1.0f;
    public int steps = 20;
    public float step_speed = 1.0f;

    [Header("Motion")]
    public bool oscillate = true;
    public bool increasing_velocity = true;
};

[System.Serializable]
public class CircleParameters
{
    [Header("Shape")]
    public float radius = 1.0f;
    public int steps = 20;

    [Header("Rotation")]
    public bool rotate_with_time = false;
    public float rotation_speed = 60.0f;
};

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Emission")]
    public float spawn_rate = 1f;
    public bool random_spawn_rate = false;

    [Header("Shape")]
    public SpawnerEmissionShape shape;

    [Header("Circle")]
    public CircleParameters circle;

    [Header("Spiral")]
    public SpiralParameters spiral;

    [Header("Burst")]
    public bool burst = false;
    public int burst_count = 0;
    public float burst_rate = 1;

    private const float min = 1;
    private const float max = 10;

    [Header("Projectile")]
    [Range(min, max)]
    public float scale;
    public bool random_scale;
    public float speed;

    public List<Projectile> projectile_types;
    public bool CheckState(SpawnerEmissionShape shape_flags){ return (shape & shape_flags) == shape_flags;}

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(spawn_rate > 0f);
        Debug.Assert(projectile_types.Count > 0);

        // Prepare points
        if (CheckState(SpawnerEmissionShape.Circle))
        {
            float theta = 0.0f;
            float step_size = 2 * Mathf.PI / circle.steps;
            for(int i = 0; i < circle.steps; i++) 
            {
                m_points.Add(new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
                theta += step_size;
            }
        }

        if (CheckState(SpawnerEmissionShape.Spiral))
        {
            float theta = 0.0f;

            float step_size = 2 * Mathf.PI / spiral.steps;
            for (int i = 0; i < spiral.steps; i++)
            {
                m_points.Add(new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
                theta += step_size;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_interval_timer > (1.0f / spawn_rate))
        {
            m_interval_timer = 0.0f;
            if (CheckState(SpawnerEmissionShape.Circle))
            {
                Circle();
            }
            else if (CheckState(SpawnerEmissionShape.Spiral)) 
            {
                Spiral();
            }
            else if (CheckState(SpawnerEmissionShape.Targeted)) 
            {
                if (burst)
                {
                    StartCoroutine(BurstFire());
                }
                else
                {
                    Targeted();
                }
            }

            m_interval_timer = 0.0f;
            if (random_spawn_rate)
            {
                m_interval_timer = Random.Range(0, (1.0f / spawn_rate) * 0.5f);
            }
        }

         m_interval_timer += Time.deltaTime;
         m_time_elapsed += Time.deltaTime;
    }

    void Circle()
    {
        int projectile_type_index = Random.Range(0, projectile_types.Count);
        foreach (Vector3 point in m_points)
        {
            float scale_value = scale;
            if (random_scale)
            {
                scale_value = Random.Range(min, scale);
            }

            Vector3 rotated_point = Vector3.Normalize(transform.rotation * point);

            if (circle.rotate_with_time)
            {
                rotated_point = Vector3.Normalize(Quaternion.AngleAxis(Time.timeSinceLevelLoad * circle.rotation_speed, Vector3.up) * rotated_point);
            }

            var projectile = Instantiate(projectile_types[projectile_type_index], transform.position + rotated_point, Quaternion.identity);
            projectile.transform.localScale *= scale_value;

            projectile.velocity = rotated_point * speed;
        }
    }

    void Spiral()
    {
        StartCoroutine(SpawnSpiralEffect());
    }

    bool forward = true;
    IEnumerator SpawnSpiralEffect()
    {
        float speed = 10.0f;

        float theta = 0.0f;
        if (spiral.oscillate)
        {
          theta = forward ? 0.0f : (Mathf.Deg2Rad * spiral.angle);
        }

        float scale_value = scale;

        float single_bullet_time = (1.0f / spawn_rate) / spiral.steps;

        float step_size = (Mathf.Deg2Rad * spiral.angle) / spiral.steps;
        int step = 0; 

        int projectile_type_index = Random.Range(0, projectile_types.Count);
        while (step < spiral.steps)
        {
            yield return new WaitForSeconds(single_bullet_time / spiral.step_speed);

            theta += forward ? step_size : -step_size;
            step++;

            if (random_scale)
            {
                scale_value = Random.Range(min, scale);
            }

            Vector3 point = (new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
            Vector3 rotated_point = Vector3.Normalize(transform.rotation * point);

            var projectile = Instantiate(projectile_types[projectile_type_index], transform.position + rotated_point * spiral.radius, Quaternion.identity);
            projectile.transform.localScale *= scale_value;

            projectile.velocity = rotated_point * speed;

            if (spiral.increasing_velocity)
            {
                float speed_multiplier = 1 - (step / spiral.steps);
                projectile.velocity *= Mathf.Clamp(speed_multiplier, 0.5f, speed_multiplier);
            }

            yield return null;
        }

        if (spiral.oscillate)
        {
            forward = !forward;
        }
    }

    void Targeted()
    {
        if (m_target == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if(player != null ) 
            { 
                m_target = FindObjectOfType<PlayerController>().transform;
            }
        }

        if (m_target == null)
        {
            Debug.Log("No Target!");
            return;
        }

        int projectile_type_index = Random.Range(0, projectile_types.Count);
        var projectile = Instantiate(projectile_types[projectile_type_index], transform.position, Quaternion.identity);

        projectile.transform.localScale *= scale;
        projectile.transform.LookAt(new Vector3(m_target.position.x, scale, m_target.position.z));

        Vector3 dir = projectile.transform.forward;
        dir.y = 1.0f;
        dir.Normalize();

        projectile.velocity = projectile.transform.forward * speed;
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
    private float m_time_elapsed = 0;
    private float m_interval_timer = 0;

    private Transform m_target;

    // ~ Projectils
    List<Projectile> projectiles;
}
