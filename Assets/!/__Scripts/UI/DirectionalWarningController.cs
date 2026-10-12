using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}


public class DirectionalWarningController : MonoBehaviour
{
    [System.Serializable]
    public class DirectionalWarning
    {
        public Direction direction;
        public SpriteRenderer srend;
        public Sprite sprite;
    }
    public SoundEffect warningSound;

    [Header("Directional Warnings")]
    [SerializeField] private List<DirectionalWarning> warnings;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.6f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 1f;

    private Coroutine activeFlash;

    private Dictionary<Direction, DirectionalWarning> lookup;
    private Dictionary<Direction, Coroutine> activeFlashes;


    private void Awake()
    {
        lookup = new Dictionary<Direction, DirectionalWarning>();
        activeFlashes = new Dictionary<Direction, Coroutine>();

        foreach (var w in warnings)
        {
            if (w.srend == null || w.sprite == null)
                continue;

            w.srend.enabled = false;
            lookup[w.direction] = w;
            activeFlashes[w.direction] = null;
        }
    }


    /// <summary>
    /// Flash a directional warning on the screen border.
    /// </summary>
    public void Flash(Direction direction)
    {
        if (!lookup.TryGetValue(direction, out var warning))
        {
            Debug.LogWarning($"No warning configured for direction {direction}");
            return;
        }

        if (activeFlashes[direction] != null)
        {
            StopCoroutine(activeFlashes[direction]);
            activeFlashes[direction] = null;
        }

        activeFlashes[direction] = StartCoroutine(FlashRoutine(warning));
    }

    private IEnumerator FlashRoutine(DirectionalWarning warning)
    {
        SpriteRenderer img = warning.srend;
        img.sprite = warning.sprite;
        img.enabled = true;

        float singleFlashTime = flashDuration / flashCount;
        Color baseColor = img.color;

        //ScreenDimmerManager.Instance.AddDimSource(this.transform.name);   

        
        for (int i = 0; i < flashCount; i++)
        {
            float pitch = 1 + i * 0.1f;
            yield return Fade(img, baseColor, minAlpha, maxAlpha, singleFlashTime * 0.5f);
            AudioHelpers.PlaySoundEffect(warningSound, Camera.main.transform.position, pitch);
            yield return Fade(img, baseColor, maxAlpha, minAlpha, singleFlashTime * 0.5f);
        }

       

        img.enabled = false;
        activeFlashes[warning.direction] = null;

        //yield return new WaitForSeconds(2.0f);
        //ScreenDimmerManager.Instance.RemoveDimSource(this.transform.name);
    }

    private IEnumerator Fade(SpriteRenderer sRend, Color baseColor, float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            sRend.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        sRend.color = new Color(baseColor.r, baseColor.g, baseColor.b, to);
    }
}
