using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DoorWall : MonoBehaviour
{
    public AudioClip openSound;
    public AudioClip closeSound;
    public GameObject doorObject; // assign in inspector
    private float openCloseDuration = 1f; // time to open/close door
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float moveSpeed;
    private float lifeDuration;
    public bool performSequence = false;
    private bool openState = false;

    public void Init(Vector2 _direction, float _speed, float _lifeDuration, float OpenCloseBaseDuration, float OpenCloseRandomOffset)
    {
        rb = GetComponent<Rigidbody2D>();
        moveDirection = _direction.normalized;
        moveSpeed = _speed;
        lifeDuration = _lifeDuration;
        openCloseDuration = OpenCloseBaseDuration + Random.Range(-OpenCloseRandomOffset, OpenCloseRandomOffset);

        // Rotate wall to face movement direction (optional)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(StartMovement());
        StartCoroutine(OpenCloseDoor());
    }

    IEnumerator StartMovement()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
        yield return new WaitForSeconds(lifeDuration);
        Destroy(gameObject); // cleanup
    }

    IEnumerator OpenCloseDoor()
    {
        performSequence = true;
        // 🔹 Add random offset before starting open/close loop
        float randomOffset = Random.Range(0f, openCloseDuration);
        yield return new WaitForSeconds(randomOffset);

        // 🔄 Now start normal open/close toggling
        while (performSequence)
        {
            openState = !openState;
            doorObject.SetActive(openState);
            if (!openState)
            {
                AudioHelpers.PlayMyClipAtPoint(openSound, AudioChannel.SFX, transform.position, 1);
            }
            else
            {
                AudioHelpers.PlayMyClipAtPoint(closeSound, AudioChannel.SFX, transform.position, 1);
            }
            yield return new WaitForSeconds(openCloseDuration);
        }
    }




}
