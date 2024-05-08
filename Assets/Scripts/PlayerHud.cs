using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    [SerializeField]
    public PropertyBar health_bar;

    [SerializeField]
    public PropertyBar energy_bar;

    [SerializeField]
    public PropertyBar exp_bar;

    [SerializeField]
    public GameObject rewards;

    public void Reward()
    {
        // TODO: Generate Random Rewards
        rewards.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
