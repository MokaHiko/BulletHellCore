using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<Wave> waves;

    public int wave_index;
    public Wave current_wave;

    private void Start()
    {
        Debug.Assert(waves.Count > 0);
        current_wave = Instantiate(waves[wave_index++]);
        current_wave.Init(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (current_wave == null)
        {
            Debug.Assert(false, "Level has waves!"); 
            return;
        }

        current_wave.OnFrameTick(Time.deltaTime);

        if (current_wave.IsComplete)
        {
            if (wave_index < waves.Count)
            {
                Debug.Log("Wave: " + wave_index);
                current_wave = GameObject.Instantiate(waves[wave_index++]);
                current_wave.Init(this);
            }
            else
            {
                Debug.Log("Level Complete");
            }
        }
    }
}


