using UnityEngine;

public class WorldPathView : MonoBehaviour
{
    public void Setup(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float distance = dir.magnitude;

        transform.localScale = new Vector2(distance, transform.localScale.y);
        transform.position = (from + to) / 2f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}