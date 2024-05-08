using System.Collections;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    [SerializeField]
    Room room;

    [SerializeField]
    float door_open_duration = 0.25f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController controller))
        {
            if (room.IsActivated())
            {
                return;
            }
            room.Init();
            StartCoroutine(ToggleDoor(door_open_duration));

            room.room_complete_calblack += controller.OnRoomComplete;
            room.room_complete_calblack += ()=> room.room_complete_calblack-=controller.OnRoomComplete;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController controller))
        {
            if (room.IsActivated())
            {
                Invoke(nameof(Close), 1.0f);
            }
        }
    }

    void Close()
    {
        if (is_open)
        {
            StartCoroutine(ToggleDoor(door_open_duration));
        }
    }

    void Open()
    {
        if (!is_open)
        {
            StartCoroutine(ToggleDoor(door_open_duration));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(room != null);
        room.room_complete_calblack += () => StartCoroutine(ToggleDoor(door_open_duration));
    }

    IEnumerator ToggleDoor(float duration = 1.0f)
    {
        float time = 0.0f;

        Vector3 start_position;
        Vector3 end_position;
        if (!is_open)
        {
            start_position = transform.position;
            end_position = transform.position + Vector3.up * 5.0f;

            is_open = true;
        }
        else
        {
            start_position = transform.position;
            end_position = transform.position - Vector3.up * 5.0f;

            is_open = false;
        }

        while (time < door_open_duration)
        {
            transform.position = Vector3.Lerp(start_position, end_position, time / door_open_duration);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private bool is_open = false;
}
