using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetListenerPosition : MonoBehaviour
{

    public GameObject camera_reference;
    public GameObject player_reference;
    private Vector3 mid_point;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mid_point = (camera_reference.transform.position + player_reference.transform.position)/2.0f;
        transform.rotation = camera_reference.transform.rotation;
        transform.position = mid_point;

    }
}
