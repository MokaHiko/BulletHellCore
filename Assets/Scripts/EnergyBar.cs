using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Gradient gradient;

    [SerializeField]
    private Color short_circuit;

    [SerializeField]
    private Image fill;
    
    public void SetValue(float energy, float energy_max = 100.0f)
    {
        if (energy < 0)
        {
            fill.color = short_circuit;
        }
        else
        {
            fill.color = gradient.Evaluate(slider.value);
        }

        slider.value = energy / energy_max;
    }
}
