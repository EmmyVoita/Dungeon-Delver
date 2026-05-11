using System.Collections;
using UnityEngine;

public class DoorFadeController : MonoBehaviour
{
    [SerializeField] private DoorWall doorWall;  // make it visible in inspector
    [SerializeField] private float disableDelay = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player triggered door fade.");

        if (doorWall != null)
        {
            StartCoroutine(WaitAndDisable());
        }
        else
            Debug.LogWarning("DoorWall reference not assigned!");
    }

    private IEnumerator WaitAndDisable()
    {
        yield return new WaitForSeconds(disableDelay);
        doorWall.performSequence = false;
    }
}
