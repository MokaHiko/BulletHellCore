using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class FloatingText : MonoBehaviour
{
    private void SetGlobalScale(Vector3 global_scale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(global_scale.x / transform.lossyScale.x, global_scale.y / transform.lossyScale.y, global_scale.z / transform.lossyScale.z);
    }

    private void Start()
    {
        float duration = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length; 
        Destroy(gameObject, duration);
    }
    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
