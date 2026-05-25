using UnityEngine;

public class DestroyAfterDuration : MonoBehaviour
{
    [SerializeField] private Transform rootTransform;
    [SerializeField] private float duration;
    private void Start()
    {
        Destroy(rootTransform.gameObject,duration);
    }
}