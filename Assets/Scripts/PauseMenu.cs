using Cinemachine;
using System.Collections;
using UnityEngine;

public class PauseMenu : Menu
{
    [Header("Layout")]
    [SerializeField]
    public CinemachineVirtualCamera virtual_camera;
    private GameManager gameManager;

    protected override void OnActivate()
    {
        gameManager = FindObjectOfType<GameManager>();

        Debug.Assert(gameManager != null, "No GameManager found");
        Debug.Assert(virtual_camera != null, "No camera selected");

        gameManager.IndefinedStop();

        virtual_camera.m_Lens.OrthographicSize = 2.5f;  
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0.0f, 1.5f, -10.0f);

        // Create party view
        if (GameManager.Instance.GetPlayer() == null)
        {
            return;
        }
    }

    protected override void OnDeactivate()
    {
        gameManager.ContinueTime();

        virtual_camera.m_Lens.OrthographicSize = m_default_orthographic_size;
        virtual_camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0.0f, 1.5f, -10.0f);
    }

    private Vector3 m_default_offset;
    private float m_default_orthographic_size;
}
