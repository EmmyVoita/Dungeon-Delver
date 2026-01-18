using System;
using UnityEngine;

public class RingHoleTrigger : MonoBehaviour
{
    public static Action<GameObject> RingHolePassedThrough;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        RingHolePassedThrough?.Invoke(this.transform.parent.gameObject);
    }
}
