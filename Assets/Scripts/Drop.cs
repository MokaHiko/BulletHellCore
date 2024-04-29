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
                //player_controller.GetComponent<Unit>().UseAbility(Dash);
                player_controller.GetComponent<Unit>().UseAbility(0, true);
            }

            if((drop_type & DropType.Burst) == DropType.Burst)
            {
                player_controller.Burst();
            }

            // Use up effect
            Destroy(gameObject);
        }
    }
}
