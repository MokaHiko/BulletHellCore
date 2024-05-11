using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void ParticleEnterCallback(GameObject other);
public class ParticleEventHandler : MonoBehaviour
{
    public ParticleEnterCallback particle_enter_callback;
    public int every_n_particle = 1;

    uint ctr = 0;
    //private void OnParticleTrigger()
    //{
    //    if(ctr % every_n_particle == 0) 
    //    {
    //        Debug.Log(ctr++);
    //        particle_enter_callback?.Invoke(null);
    //    }
    //}
    private void OnParticleCollision(GameObject other)
    {
        if(ctr++ % every_n_particle == 0) 
        {
            particle_enter_callback?.Invoke(other);
        }
    }
}
