using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffect : MonoBehaviour
{
    [SerializeField]
    LayerMask damageable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Unit>(out Unit unit))
        {
            unit.ApplyStatus(StatusEffect.Corrosion);
        }
    }
}
