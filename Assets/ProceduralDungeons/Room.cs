using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct EnemySpawnData
{
    [SerializeField]
    public EnemyController enemy;
    [SerializeField]
    public float spawn_time;
    [SerializeField]
    public int count;
};

public delegate void RoomComplete();

public class Room : MonoBehaviour
{
    [SerializeField]
    float seed;

    [SerializeField]
    RectTransform room_intro_prefab;

    [SerializeField]
    public List<RoomEntrance> entrances;

    [SerializeField]
    public List<Teleporter> teleporters;

    [Header("Enemies")]
    [SerializeField]
    public List<EnemySpawnData> enemy_types;
    public List<Transform> spawn_points;
    public PathfindingGrid grid;

    [Header("Rewards")]
    [SerializeField]
    public List<ParticleCollector> drop_particles;
    [SerializeField]
    public List<Weapon> weapon_drops;
    [SerializeField]
    public List<Merc> merc;

    public RoomComplete room_complete_calblack;
    public bool IsActivated() { return activated; }

    public int difficulty_level = 0;

    // ~ Getters
    public bool IsComplete { get { return is_complete; } }

    private void Start()
    {
        //grid.GenerateGrid();
        //foreach (Teleporter teleporter in teleporters)
        //{
        //    teleporter.teleport_received_callback += () => Init();
        //}
    }

    public void Init()
    {
        if (activated)
        {
            return;
        }
        activated = true;

        // For now generate random color
        Instantiate(room_intro_prefab, GameManager.Instance.GetPlayerHud().transform);

        // Activate room systems
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        for(int i = 0; i < enemy_types.Count;  i++)
        {
            EnemySpawnData spawn_data = enemy_types[i];
            spawn_data.count += difficulty_level;

            // Modify spawn times

            // Spawn new enemies

            enemy_types[i] = spawn_data;
        }

        foreach (EnemySpawnData enemy_data in enemy_types)
        {
            enemy_count += enemy_data.count;
            StartCoroutine(PrimeEnemySpawn(enemy_data));
        }

        // TODO: Remove teleporter
        // Deactivate teleports until level finish
        //foreach (Teleporter teleporter in teleporters)
        //{
        //    teleporter.gameObject.SetActive(false);
        //}

        // Player callbacks
        room_complete_calblack += GameManager.Instance.GetPlayer().OnRoomComplete;
        room_complete_calblack += ()=> room_complete_calblack -= GameManager.Instance.GetPlayer().OnRoomComplete;
    }

    // TODO: Change to modify room
    public void GenerateRoom()
    {
        difficulty_level++;
    }

    IEnumerator PrimeEnemySpawn(EnemySpawnData enemy_data)
    {
        yield return new WaitForSeconds(enemy_data.spawn_time);

        for (int i = 0; i < enemy_data.count; i++)
        {
            int spawn_index = Random.Range(0, spawn_points.Count); 
            EnemyController enemy = Instantiate(enemy_data.enemy, spawn_points[spawn_index].position, Quaternion.identity, gameObject.transform);
            enemy.GetComponent<Unit>().death_callback += () =>
            {
                --enemy_count;

                // Drop rewards if final enemy
                if (enemy_count <= 0)
                {
                    foreach (ParticleCollector drop in drop_particles)
                    {
                        Instantiate(drop, enemy.transform.position, Quaternion.identity);
                    }

                    foreach (Weapon weapon in weapon_drops)
                    {
                        Instantiate(weapon, enemy.transform.position, Quaternion.identity);
                    }

                    Complete();
                }
            };
        }

        yield return null;
    }

    public void Complete()
    {
        room_complete_calblack?.Invoke();

        foreach (Teleporter teleporter in teleporters)
        {
            if (teleporter.destination != null)
            {
                teleporter.gameObject.SetActive(true);
            }
        }

        is_complete = true;
    }

    public void Update()
    {
        time_elapsed += Time.deltaTime;
    }

    private float time_elapsed = 0.0f;
    private int enemy_count = 0;
    private bool activated = false;
    private bool is_complete = false;
}
