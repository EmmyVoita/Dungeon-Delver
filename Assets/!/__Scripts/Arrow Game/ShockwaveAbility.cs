using UnityEngine;

public class ShockwaveAbility : MonoBehaviour
{
    public float expandSpeed = 5f;
    public float maxScale = 5f;
    public float lifetime = 1.5f;

    //[Header("Set Dynamically")]
    //private int arrowHitCount = 0;

    void Start()
    {
        Destroy(gameObject, lifetime);
        //ComboManager.Instance.CacheCombo();
    }

    void Update()
    {
        // Expand uniformly
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // Clamp to max size
        if (transform.localScale.x >= maxScale)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        ArrowBase arrow = coll.GetComponent<ArrowBase>();
        if (arrow != null)
        {
            arrow.OnArrowHit(); // clear arrow
        }
    }
}
