using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DropType
{
    None = 0,
    Energy = 1,
    Burst = 2,
    Dash = 3
};

[RequireComponent(typeof(Renderer))]
public class Drop : MonoBehaviour
{
    [SerializeField]
    public DropType drop_type = DropType.None;

    private void Start()
    {
        if (drop_type == DropType.None)
        {
            drop_type = (DropType)(Random.Range(1, 3));
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
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Only trigger w player
        if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController player_controller))
        {
            switch(drop_type)
            {
                case DropType.Energy:
                {
                    player_controller.GetComponent<Unit>().energy += 50.0f;
                }break;
                case DropType.Burst:
                {
                    player_controller.Burst();
                }break;
                case DropType.Dash:
                {
                    player_controller.Dash();
                }break;
            }

            // Use up effect
            Destroy(gameObject);
        }
    }
}
