using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum DropType
{
    None = 0,
    Energy = 1 << 0,
    Burst = 1 << 1,
    Dash = 1 << 2
};

[RequireComponent(typeof(Renderer))]
public class Drop : MonoBehaviour
{
    [SerializeField]
    public DropType drop_type = DropType.None;

    [SerializeField]
    public float magnetic_radius = 0;

    [SerializeField]
    public float magnetic_ms = 1.0f;

    private void Start()
    {
        if (drop_type == DropType.None)
        {
            drop_type = (DropType)(1 << Random.Range(0, 3));
        }

        Color drop_color = Color.white;
        switch (drop_type)
        {
            case DropType.Energy:
            {
                drop_color = Color.green;
            }break;
            case DropType.Burst:
            {
                drop_color += Color.blue;
            }break;
            case DropType.Dash:
            {
                drop_color += Color.yellow;
            }break;
        };

        GetComponent<Renderer>().material.color = drop_color;

        // Magnetic Defaults
        start_position = transform.position;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.Slerp(start_position, target.position, time_elapsed);
            time_elapsed += Time.deltaTime * magnetic_ms;
            return;
        }

        foreach(Collider col in Physics.OverlapSphere(transform.position, magnetic_radius, LayerMask.GetMask("Player")))
        {
            if(col.TryGetComponent(out PlayerController player_controller))
            {
                target = col.transform;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Only trigger w player
        if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController player_controller))
        {
            if((drop_type & DropType.Energy) == DropType.Energy)
            {
                player_controller.GetComponent<Unit>().energy += 50.0f;
            }

            if((drop_type & DropType.Dash) == DropType.Dash)
            {
                player_controller.GetComponent<Unit>().IncrementAblilityStack(AbilityType.Movement);
            }

            // Use up effect
            Destroy(gameObject);
        }
    }

    // ~ Magnet
    Vector3 start_position;
    float time_elapsed = 0;
    Transform target;
}
