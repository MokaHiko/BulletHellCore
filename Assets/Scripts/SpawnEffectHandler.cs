using System.Collections.Generic;
using UnityEngine;

public class SpawnEffectHandler : MonoBehaviour
{
    [Header("SpawnEffect")]
    public List<GameObject> spawn_objects;
    public float spawn_radius = 1.0f;
    public Transform spawn_transform;
    public Transform meteor_transform;
    public float duration = 0.0f;
    public float landing_shake_intensity = 2.0f;

    [SerializeField]
    private ParticleSystem fire_particle_system;
    [SerializeField]
    private GameObject targetting_quad;

    [SerializeField]
    private ParticleSystem impact_particle_system;

    private void Awake()
    {
        spawn_objects = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_start_position = meteor_transform.transform.position;
    }

    public void LoadObjects()
    {
        int ctr = 1;
        foreach (GameObject spawn_object in spawn_objects)
        {
            float x = Mathf.Sin((2 * Mathf.PI / spawn_objects.Count) * ctr);
            float z = Mathf.Cos((2 * Mathf.PI / spawn_objects.Count) * ctr);

            spawn_object.SetActive(false);
            Instantiate(targetting_quad, spawn_transform.position + new Vector3(x, 0, z) * spawn_radius, targetting_quad.transform.rotation, transform);
            Instantiate(fire_particle_system, meteor_transform.position + new Vector3(x, 0, z) * spawn_radius, Quaternion.identity, meteor_transform);

            ctr++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_time += Time.deltaTime;
        meteor_transform.transform.position = Vector3.Lerp(m_start_position, spawn_transform.position, m_time / duration);

        if (m_time > duration)
        {
            int ctr = 0;
            foreach (GameObject spawn_object in spawn_objects)
            {
                float x = Mathf.Sin((2 * Mathf.PI / spawn_objects.Count) * ctr);
                float z = Mathf.Cos((2 * Mathf.PI / spawn_objects.Count) * ctr);
                ctr++;

                GameManager.Instance.RequestShake(landing_shake_intensity, 1.0f);
                Destroy(Instantiate(impact_particle_system, transform.position + new Vector3(x, 0, z) * spawn_radius, Quaternion.identity), impact_particle_system.main.duration);
                
                // Reposition objects
                spawn_object.transform.position = spawn_transform.position + new Vector3(x, 0, z) * spawn_radius;
                spawn_object.transform.rotation = Quaternion.identity;
                spawn_object.transform.SetParent(transform.parent);
                spawn_object.SetActive(true);
            }

            Destroy(gameObject);
        }
    }

    private float m_time;
    private Vector3 m_start_position;
}
