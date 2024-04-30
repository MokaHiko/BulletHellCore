using UnityEngine;
using UnityEngine.UI;

public class PropertyBar: MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Gradient gradient;

    [SerializeField]
    private Color short_circuit;

    [SerializeField]
    private Image fill;

    [SerializeField]
    private bool billboard = false;

    public void SetValue(float energy, float energy_max)
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
