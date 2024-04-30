using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCanvas : MonoBehaviour
{
    private Quaternion start_rotation;

    // Start is called before the first frame update
    void Start()
    {
        start_rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
         transform.rotation = start_rotation * Camera.main.transform.rotation;
    }
}
