using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton class 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    bool generate_dungeons = false;

    [SerializeField]
    PlayerController player_prefab;

    [SerializeField]
    Merc starting_merc_prefab;

    [SerializeField]
    DungeonGenerator2D dungeon_generator;

    [Header("UI")]
    [SerializeField]
    private PlayerHud player_hud;
    [SerializeField]
    private MenuManager in_game_menu;

    [SerializeField]
    int room_count = 0;

    public void Reward()
    {
        in_game_menu.ActivateMenu("RewardsMenu");
    }

    public void ToggleGameMenu()
    {
        in_game_menu.ToggleMenu("GameMenu");
    }

    // Requests a stop for the time of the game
    public void RequestStop(float duration)
    {
        if (m_stop_effect != null)
        {
            StopCoroutine(m_stop_effect);
            m_stop_effect = null;
        }

        m_stop_effect = StartCoroutine(StopEffect(duration));
    }

    IEnumerator StopEffect(float duration)
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }

    public void RequestShake(float intensity, float time)
    {
        if (m_shake_routine != null)
        {
            StopCoroutine(m_shake_routine);
            m_shake_routine = null;
        }

        m_shake_routine = StartCoroutine(ShakeEffect(intensity, time));
    }
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
        if (player_hud != null)
        {
            return player_hud;
        }

        Debug.Assert(player_hud != null, "No Player Hud");
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

        Debug.Assert(player_prefab != null, "No player prefab");
        Debug.Assert(m_virtual_camera != null, "No virttual camera");
        Debug.Assert(player_hud != null, "No hud");
        Debug.Assert(dungeon_generator != null, "No dungeon generator ");
    }

    [SerializeField]
    Room start_room;
    private void Start()
    {
        // Generate level
        if (generate_dungeons)
        {
            Debug.Log("Generating Dungeons...");
            dungeon_generator.Generate();
            Debug.Log("Finished Generating Dungeons!");
        }

        foreach (Room room in dungeon_generator.rooms)
        {
            room.room_complete_calblack += () =>
            {
                Debug.Log($"Room: {++room_count} / {dungeon_generator.rooms.Count}");
                if (room_count >= dungeon_generator.rooms.Count)
                {
                    // TODO: Spawn teleporter to boss battle
                    Debug.Log("SECTOR COMPLETE!");
                }
                else
                {
                    ModifyRooms();
                }
            };
        }

        // Init player and default character

        // Find Room
        Room start_room = null;
        if (generate_dungeons)
        {
             start_room = dungeon_generator.rooms[0];
        }
        else
        {
            start_room = FindObjectOfType<Room>();
        }

        m_player = FindObjectOfType<PlayerController>();
        if (!m_player)
        {
            m_player = Instantiate(player_prefab, start_room.spawn_points[0].position, Quaternion.identity);
            Merc merc = Instantiate(starting_merc_prefab, start_room.spawn_points[0].position, Quaternion.identity);
            m_player.AddMember(merc);
        }
        CameraTarget camera_target = m_player.GetComponentInChildren<CameraTarget>();
        Debug.Assert(camera_target != null, "Player has no camera target!");
        m_virtual_camera.Follow = camera_target.transform;
        m_virtual_camera.Follow = camera_target.transform;

        // Init Room
        start_room.Init();
    }

    public void ModifyRooms()
    {
        foreach (Room room in dungeon_generator.rooms)
        {
            if (room.IsComplete) continue;

            // Regenerate room with harder parameters
            room.GenerateRoom();
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    // ~ Camera Effects
    IEnumerator ShakeEffect(float intensity, float shake_time)
    {
        var cinemachine_perlin = GameManager.Instance.GetVirtualCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachine_perlin.m_AmplitudeGain = intensity;

        float time = 0.0f;
        while(time < shake_time) 
        {
            time += Time.deltaTime;
            yield return null;
        }

        cinemachine_perlin.m_AmplitudeGain = 0;
        m_shake_routine = null;
    }


    // ~ Handles
    [SerializeField]
    private PlayerController m_player;

    [SerializeField]
    CinemachineVirtualCamera m_virtual_camera;

    // ~ Camera Effects
    Coroutine m_shake_routine;
    float m_current_shake;

    // ~ Game effects
    Coroutine m_stop_effect;
}
