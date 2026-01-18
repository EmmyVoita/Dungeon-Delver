using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum eType { key, health, grappler, sock }
    public static float COLLIDER_DELAY = 0.5f;

    [Header("Set in Inspector")]
    public eType itemType;
    public string itemName;

    // Awake () and Activate() disable the PickUp's Collider for 0.5 secs



    void Awake()
    {
        GetComponent<Collider>().enabled = false;
        Invoke("Activate", COLLIDER_DELAY);
    }

    void Activate()
    {
        GetComponent<Collider>().enabled = true;
    }
}
