using UnityEngine;
using System.Collections;
using DG.Tweening;

public class WallProjectile : MonoBehaviour
{
    public Transform windupTransform;
    public float windupDistance = 0.3f;
    public event System.Action<int, int> OnWindupTick;
    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;
    private float windupDuration;

    private float shiftDuration;
    private bool firing = false;
    private Vector3 anchorPos;
    private Coroutine shiftRoutine;
    private Vector3 currentShiftOffset;


    public void Init(
        Vector2 _direction,
        float _speed,
        float _windup,
        float _shiftDuration,
        bool allowShift,
        float shiftDist,
        float shiftSpd)
    {
        rb = GetComponent<Rigidbody2D>();

        direction = _direction.normalized;
        speed = _speed;
        windupDuration = _windup;
        shiftDuration = _shiftDuration;
        StartCoroutine(FireSequence(
            allowShift,
            shiftDist,
            shiftSpd
        ));
    }

    private IEnumerator FireSequence(
        bool allowShift,
        float shiftDistance,
        float shiftSpeed)
    {
        // -------------------------
        // PHASE 1: Shift (optional)
        // -------------------------
        if (allowShift)
        {
            shiftRoutine = StartCoroutine(
                ShiftAnim(shiftDistance, shiftSpeed)
            );

            // Let shifting run for the full windup duration
            yield return new WaitForSeconds(shiftDuration);

            // Stop shifting
            StopCoroutine(shiftRoutine);

            // 🔒 LOCK final position explicitly
            transform.position = transform.position; // forces last value
            anchorPos = transform.position;

            // Detach warning lines / VFX
            transform.DetachChildren();
        }

        // Lock final position
        anchorPos = transform.position;

        // -------------------------
        // PHASE 2: Windup pullback
        // -------------------------
        //yield return StartCoroutine(WindupAnim());

        // -------------------------
        // PHASE 3: Fire
        // -------------------------
        //Fire();
    }

    private IEnumerator ShiftAnim(float shiftDistance, float shiftSpeed)
    {
        Vector3 startPos = transform.position;
        Vector2 perp = new Vector2(-direction.y, direction.x).normalized;
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime;
            float offset = Mathf.Sin(t * shiftSpeed) * shiftDistance;
            currentShiftOffset = (Vector3)perp * offset;
            transform.position = startPos + currentShiftOffset;
            yield return null;
        }
    }


    public IEnumerator WindupAnim()
    {
        Sequence s = DOTween.Sequence();

        Vector3 startPos = transform.position;
        Vector3 direction = -transform.up; // respects rotation

        s.Append(
            transform.DOMove(startPos + direction * windupDistance, windupDuration)
                    .SetEase(Ease.OutQuad)
        );

        yield return s.WaitForCompletion();

        Fire();
    }



    public void Fire()
    {
        if (firing) return;
        firing = true;

  

        rb.linearVelocity = direction * speed;


        StartCoroutine(AutoDestroy());
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }
}
