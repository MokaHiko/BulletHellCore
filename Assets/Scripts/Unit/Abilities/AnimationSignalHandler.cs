using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnimationSignalCallback();
public class AnimationSignalHandler : MonoBehaviour
{
    public AnimationSignalCallback begin_callback;
    public AnimationSignalCallback end_callback;
    public void BeginAnimation()
    {
        begin_callback?.Invoke();
    }

    public void EndAnimation()
    {
        end_callback?.Invoke();
    }
}
