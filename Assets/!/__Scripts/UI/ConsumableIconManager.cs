using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConsumableUIAnimator : MonoBehaviour
{
    public static ConsumableUIAnimator Instance;
    public RectTransform floatingParent;  // Empty UI object (no layout group!)
    public Image floatingIconPrefab;      // Prefab with just an Image component
    public float launchHeight = 250f;
    public float launchDuration = 1.2f;
    public AudioClip launchSound;

    void Awake() => Instance = this;

    public void PlayUseAnimation(Image sourceIcon)
    {
        // Get world position of source icon
        RectTransform sourceRect = sourceIcon.GetComponent<RectTransform>();
        Vector3 worldPos = sourceRect.position;

        // Create floating copy under non-layout parent
        Image floating = Instantiate(floatingIconPrefab, floatingParent);
        floating.sprite = sourceIcon.sprite;
        floating.transform.position = worldPos;

        StartCoroutine(WaitAndPlaySound(0.2f));

        StartCoroutine(AnimateIcon(floating.rectTransform));
    }

    private IEnumerator AnimateIcon(RectTransform icon)
    {
        Vector2 start = icon.anchoredPosition;
        Vector2 end = start + new Vector2(Random.Range(-200, 200), -500f);

        float time = 0f;

        Image img = icon.GetComponent<Image>();
        Color startColor = img.color;

        while (time < launchDuration)
        {
            time += Time.deltaTime;
            float t = time / launchDuration;

            // curved arc
            float yOffset = Mathf.Sin(t * Mathf.PI) * launchHeight;
            Vector2 pos = Vector2.Lerp(start, end, t);
            icon.anchoredPosition = pos + Vector2.up * yOffset;

            // fade + shrink
            img.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            icon.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, t);

            yield return null;
        }

        Destroy(icon.gameObject);
    }

    private IEnumerator WaitAndPlaySound(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (launchSound != null) AudioHelpers.PlayClipWithVariation(launchSound, AudioChannel.SFX, Camera.main.transform.position);
    }
}
