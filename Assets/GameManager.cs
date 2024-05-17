using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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

    [Header("Effects")]
    public Volume post_process_volume;

    [Header("UI")]
    public PlayerHud player_hud;
    public MenuManager in_game_menu;

    [SerializeField]
    int room_count = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            in_game_menu.ToggleMenu("PauseMenu");
        }
    }

    public void Reward(Merc merc)
    {
        //in_game_menu.ActivateMenu("RewardsMenu");
        PartyLayoutMenu layout_menu = in_game_menu.FindMenu("PartyLayoutMenu") as PartyLayoutMenu;
        in_game_menu.ActivateMenu(layout_menu);
        //in_game_menu.ActivateMenu("PartyLayoutMenu");
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

    public void IndefinedStop()
    {
        Time.timeScale = 0.0f;
        PlayerController pc = GetPlayer();
        if (pc) { pc.lockedControls = true; }
    }

    public void ContinueTime()
    {
        Time.timeScale = 1.0f;
        PlayerController pc = GetPlayer();
        if (pc) { pc.lockedControls = false; }
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

    public void RequestVignette(float duration, float intensity = 0.45f)
    {
        if (m_vignette_routine != null)
        {
            StopCoroutine(m_vignette_routine);
            m_vignette_routine = null;
        }

        m_vignette_routine = StartCoroutine(VignetteEffect(duration, intensity));
    }

    public void RequestSlowMo(float duration, float scale = 0.02f)
    {
        if (m_slowmo_routine != null)
        {
            StopCoroutine(m_slowmo_routine);
            m_slowmo_routine = null;
        }

        m_slowmo_routine = StartCoroutine(SlowMoEffect(duration, scale));
    }
    public void RequestZoom(float round_trip_duration, float zoom_scale = 2.0f) 
    {
        if (m_zoom_routine != null)
        {
            return;
        }

        m_zoom_routine = StartCoroutine(ZoomEffect(round_trip_duration, zoom_scale));
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
    IEnumerator ZoomEffect(float round_trip_duration, float zoom_scale)
    {
        float start_ortho_size = m_virtual_camera.m_Lens.OrthographicSize;

        float time = 0.0f;
        float duration = round_trip_duration / 2.0f;

        while(time < duration) 
        {
            time += Time.deltaTime;
            m_virtual_camera.m_Lens.OrthographicSize = Mathf.Lerp(start_ortho_size, start_ortho_size / zoom_scale, time / duration);
            yield return null;
        }

        time = 0.0f;
        while(time < duration) 
        {
            time += Time.deltaTime;
            m_virtual_camera.m_Lens.OrthographicSize = Mathf.Lerp(start_ortho_size / zoom_scale, start_ortho_size, time / duration);
            yield return null;
        }

        m_virtual_camera.m_Lens.OrthographicSize = start_ortho_size;
        m_zoom_routine = null;
    }

    private IEnumerator VignetteEffect(float duration, float intensity)
    {
        if (!post_process_volume.profile.TryGet<Vignette>(out Vignette vignette))
        {
            yield break;
        }

        float start_intensity = vignette.intensity.value;
        vignette.intensity.value = intensity;

        yield return new WaitForSeconds(duration);

        vignette.intensity.value = start_intensity;
    }

    private IEnumerator SlowMoEffect(float duration, float scale)
    {
        //burst_particle_system.Play();
        //burst = true;

        // Slow time
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * scale;

        //float start_distance = m_camera_start_distance;
        //float end_distance = start_distance - burst_zoom;

        float time = 0.0f;
        while (time < duration)
        {
            //(m_cm_component_base as CinemachineFramingTransposer).m_CameraDistance = Mathf.Lerp(start_distance, end_distance, time / duration);
            time += (1.0f / duration) * Time.unscaledDeltaTime;

            yield return null;
        }

        //(m_cm_component_base as CinemachineFramingTransposer).m_CameraDistance = start_distance;

        // Reset time
        Time.timeScale = 1.0f;
        m_slowmo_routine = null;

        //burst = false;
        //m_burst_routine = null;
    }

    // ~ Handles
    [SerializeField]
    private PlayerController m_player;

    [SerializeField]
    CinemachineVirtualCamera m_virtual_camera;
    CinemachineComponentBase m_cm_component_base;

    // ~ Camera Effects
    Coroutine m_shake_routine;
    Coroutine m_zoom_routine;
    float m_current_shake;

    // ~ Game effects
    Coroutine m_stop_effect;
    Coroutine m_slowmo_routine;
    Coroutine m_vignette_routine;
}
