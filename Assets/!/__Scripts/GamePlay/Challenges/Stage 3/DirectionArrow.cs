using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private float outterPadding = 0.5f;
    [SerializeField] private float innerPadding = 0.5f;

    private Transform _start;
    private Transform _target;
    private bool _inited = false;

    public void Initialize(Transform start, Transform target)
    {
        _start = start;
        _target = target;
        _inited = true;

        lineRenderer.positionCount = 2;
    }

    private void LateUpdate()
    {
        if(!_inited)
            return;

        if ((_start == null || _target == null) && _inited)
        {
            Destroy(arrowHead.gameObject);
            Destroy(this.gameObject);
            return;
        }
            

        

        Vector3 startPos = _start.position;
        Vector3 targetPos = _target.position;

        Vector2 direction = (targetPos - startPos).normalized;

        startPos += (Vector3)direction * innerPadding;
        targetPos -= (Vector3)direction * outterPadding;


        // Draw line.
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, targetPos);

        // Put arrowhead at target.
        arrowHead.position = targetPos;

        // Point arrowhead toward target.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        arrowHead.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}