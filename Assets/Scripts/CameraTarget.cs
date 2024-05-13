using UnityEngine;

public class CameraTarget : MonoBehaviour
{ 
    [SerializeField]
    Transform player;

    [SerializeField]
    float threshold;

    [SerializeField]
    Vector3 diff;

    private void Start()
    {
        Debug.Assert(player != null);
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
        {
            Vector3 target_pos = (player.position + hit.point) / 2.0f;
            target_pos.y = player.position.y;

            diff = (player.position + hit.point);

            target_pos.x = Mathf.Clamp(target_pos.x, player.position.x - threshold, player.position.x + threshold);
            target_pos.z = Mathf.Clamp(target_pos.z, player.position.z - threshold, player.position.z + threshold);

            Vector3 pos = Vector3.Lerp(player.position, target_pos, 0.0f);
            transform.position = target_pos;
        }
    }
}
