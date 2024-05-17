using Cinemachine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PartyLayoutMenu : MenuManager
{
    [Header("Layout")]
    [SerializeField]
    public CinemachineVirtualCamera virtual_camera;

    [Header("LineUp")]
    [SerializeField]
    bool rotate = true;
    [SerializeField]
    public float rotation_speed = 35.0f;
    [SerializeField]
    Transform line_up;
    [SerializeField]
    List<Transform> line_up_containers;
    [SerializeField]
    List<GameObject> merc_views;

    protected override void OnActivate() 
    {
        Debug.Assert(virtual_camera != null);

        GameManager.Instance.IndefinedStop();

        virtual_camera.LookAt = line_up;
        virtual_camera.Follow = line_up;

        virtual_camera.m_Lens.OrthographicSize = 2.5f;
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0.0f, 1.5f, -10.0f);

        // Create party view
        if (GameManager.Instance.GetPlayer() == null)
        {
            return;
        }

        int ctr = 0;
        foreach (PartySlot slot in GameManager.Instance.GetPlayer().party_slots)
        {
            if (slot.merc != null)
            {
                m_mercs.Add(Instantiate(merc_views[(int)slot.merc.type], line_up_containers[ctr++].position, Quaternion.identity, line_up_containers[(int)slot.merc.type]));
            }
        }

    }

    private void Update()
    {
        if (rotate)
        {
            foreach(GameObject merc in m_mercs) 
            {
                merc.transform.rotation = Quaternion.Euler(0.0f, rotation_speed * Time.unscaledTime, 0.0f);
            }
        }
    }

    protected override void OnDeactivate() 
    {
        // Destroy merc views
        foreach (GameObject merc in m_mercs)
        {
            Destroy(merc.gameObject);
        }
        m_mercs.Clear();

        virtual_camera.m_Lens.OrthographicSize = m_default_orthographic_size;
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0.0f, 1.5f, -10.0f);

        GameManager.Instance.ContinueTime();
    }

    public override void OnMenuClose(Menu menu)
    {
        Reset();
    }

    private void Reset()
    {
        // Reset camerae position
        virtual_camera.m_Lens.OrthographicSize = m_default_orthographic_size;
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = m_default_offset;

        m_default_orthographic_size = virtual_camera.m_Lens.OrthographicSize;
        m_default_offset = virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;

        rotate = true;
    }

    public void OnMercSelect(int index)
    {
        m_mercs[index].transform.rotation = Quaternion.identity;

        rotate = false;

        MercInspector menu = FindMenu(nameof(MercInspector)) as MercInspector;
        Merc selected_merc = GameManager.Instance.GetPlayer().party_slots[index].merc;

        if (selected_merc != null) 
        { 
            menu.SetMerc(selected_merc, m_mercs[index].transform);
            ActivateMenu(menu);
        }
        else
        {
            Debug.Assert(false, "Player has no merc at " + index);
        }
    }

    private Vector3 m_default_offset;
    private float m_default_orthographic_size;
    private List<GameObject> m_mercs = new List<GameObject>();
}
