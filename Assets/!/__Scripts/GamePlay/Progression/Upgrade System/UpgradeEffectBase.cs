using UnityEngine;
using System.Collections;

public abstract class UpgradeEffectBase : MonoBehaviour
{
    [Header("General Upgrade Settings")]
    public Sprite icon;
    public bool canStack = false;
    public bool isTemporary = false;
    public float duration = 5f;

    [HideInInspector] public GameObject iconReference;
    [HideInInspector] public bool hasBeenSelected;

    protected Player player;

    public virtual void Apply(Player target)
    {
        player = target;
        if (isTemporary)
            StartCoroutine(RemoveAfterDuration());
    }

    protected virtual IEnumerator RemoveAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        Remove();
    }

    public virtual void Remove()
    {
        Debug.Log($"⏳ Buff {name} expired.");
        Destroy(gameObject);
        if (iconReference != null)
            Destroy(iconReference);
    }
}
