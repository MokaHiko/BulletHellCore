using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton class 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField]
    PlayerController player_prefab;

    [SerializeField]
    DungeonGenerator2D dungeon_generator;

    [SerializeField]
    private PlayerHud player_hud;

    [SerializeField]
    int room_count = 0;
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

        // Camera
        m_virtual_camera = FindObjectOfType<CinemachineVirtualCamera>();

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
        //m_player.GetComponent<Unit>().death_callback += () =>
        //{
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        //};

        // TODO: Move to individual party members
        //m_player.player_level_up_callback += () =>
        //{
        //    player_hud.Reward();
        //};
    }

    public void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    private void GenerateModifier()
    {

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
}
