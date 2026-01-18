using UnityEngine;

public class ShieldAbility : MonoBehaviour
{
    public float expandSpeed = 10f;
    public float maxScale = 2f;
    public float lifetime = 1.5f;

    public ParticleSystem shieldBlockEffect;

    void Start()
    {
        Destroy(gameObject, lifetime);
        //ComboManager.Instance.CacheCombo();

        
    }

    void Update()
    {
        // Clamp to max size
        if (transform.localScale.x <= maxScale)
        {
            // Expand uniformly
            transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        ArrowBase arrow = coll.GetComponent<ArrowBase>();
        if (arrow != null)
        {
            ComboManager.Instance.AddHit(); // Increment combo for each arrow hit
            arrow.OnArrowHit(); // clear arrow
             // VFX
            if (shieldBlockEffect != null)
                Instantiate(shieldBlockEffect, arrow.transform.position, arrow.transform.rotation);
        }
    }
}
