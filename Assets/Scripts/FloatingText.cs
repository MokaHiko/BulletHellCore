using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField]
    public Color CorossionColor;

    [SerializeField]
    public Color CriticalColor;

    private void Start()
    {
    }
    private void Update()
    {
        //transform.LookAt(Camera.main.transform.position);
        // TODO: put as billboard
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
