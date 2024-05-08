using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton class 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField]
    PlayerController player_prefab;

    [SerializeField]
    PlayerHud player_hud_prefab;

    [SerializeField]
    DungeonGenerator2D dungeon_generator;

    [SerializeField]
    int room_count = 0;

    public CinemachineVirtualCamera GetVirtualCamera()
    {
        return m_virtual_camera;
    }

    public PlayerController GetPlayer()
    {
        if (m_player != null)
        {
            return m_player;
        }

        Debug.Assert(m_player != null, "No Player");
        return null;
    }

    public PlayerHud GetPlayerHud()
    {
        if (m_player_hud != null)
        {
            return m_player_hud;
        }

        Debug.Assert(m_player_hud != null, "No Player Hud");
        return null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // Camera
        m_virtual_camera = FindObjectOfType<CinemachineVirtualCamera>();

        Debug.Assert(player_prefab != null, "No player prefab");
        Debug.Assert(player_hud_prefab != null, "No player hud prefab");
        Debug.Assert(m_virtual_camera != null, "No virttual camera");
        Debug.Assert(dungeon_generator != null, "No dungeon generator ");

        // Hud
        m_player_hud = Instantiate(player_hud_prefab);
    }

    [SerializeField]
    Room start_room;
    private void Start()
    {
        // Generate level
        //Debug.Log("Generating Dungeons...");
        //dungeon_generator.Generate();
        //Debug.Log("Complete!...");

        // Find Room
        Room start_room = dungeon_generator.rooms[0];
        foreach (Room room in dungeon_generator.rooms)
        {
            room.room_complete_calblack += () =>
            {
                Debug.Log($"Room: {++room_count} / {dungeon_generator.rooms.Count}");
                if (room_count >= dungeon_generator.rooms.Count)
                {
                    Debug.Log("SECTOR COMPLETE!");
                    // TODO: Spawn teleporter to boss battle
                }
            };
        }

        // Init player
        m_player = FindObjectOfType<PlayerController>();
        if (!m_player)
        {
            m_player = Instantiate(player_prefab, start_room.spawn_points[0].position, Quaternion.identity);
        }
        CameraTarget camera_target = m_player.GetComponentInChildren<CameraTarget>();
        Debug.Assert(camera_target != null, "Player has no camera target!");
        m_virtual_camera.Follow = camera_target.transform;
        m_virtual_camera.Follow = camera_target.transform;

        // Init Room
        start_room.Init();

        // Register player callbacks
        m_player.GetComponent<Unit>().death_callback += () => { 
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        };

        m_player.player_level_up_callback += () =>
        {
            m_player_hud.Reward();
        };
    }

    private void GenerateModifier()
    {

    }

    // ~ Handles
    [SerializeField]
    private PlayerController m_player;
    private PlayerHud m_player_hud;

    [SerializeField]
    CinemachineVirtualCamera m_virtual_camera;
}
