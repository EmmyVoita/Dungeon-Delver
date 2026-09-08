using UnityEngine;

public class LaneLineObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer laneLineBackSrend;
    [SerializeField] private SpriteRenderer laneLineFrontSrend;
    [SerializeField] private Transform laneLineBackTrans;

    private Vector3 _baseScale;

    private void Awake()
    {
        // /_baseScale = laneLineBackTrans.localScale;
    }

    public void Initialize(float scaleX)
    {
        //laneLineBackTrans.localScale = new Vector3(_baseScale.x * scaleX, _baseScale.y, _baseScale.z);
    }
}