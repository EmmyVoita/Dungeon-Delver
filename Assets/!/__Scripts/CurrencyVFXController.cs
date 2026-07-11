using UnityEngine;
using UnityEngine.VFX;

public class CurrencyVFXController : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;

    [SerializeField] private string onPlayEvent = "OnPlay";
    [SerializeField] private string onAccentEvent = "OnAccent";

    [SerializeField] private string lifetimeProperty = "lifetimeMult";
    [SerializeField] private string particleAmountProperty = "particleAmount";

    /*
    [SerializeField] private float minLifetime = 0.3f;
    [SerializeField] private float maxLifetime = 2f;
    */

    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 20;

    [SerializeField] private int minCurrency = 1;
    [SerializeField] private int maxCurrency = 300;

    public void Play(int currencyAmount, bool overrideLifetime = true)
    {
        if (visualEffect == null)
            return;
        /*
        float lifetime = Mathf.Lerp(
            minLifetime,
            maxLifetime,
            Mathf.InverseLerp(minCurrency, maxCurrency, currencyAmount)
        );
        */

        /*
        if(overrideLifetime)
            visualEffect.SetFloat(lifetimeProperty, lifetime);
        */

        int particleAmount = (int) Mathf.Lerp(
            minCoins,
            maxCoins,
            Mathf.InverseLerp(minCurrency, maxCurrency, currencyAmount)
        );

        visualEffect.SetInt(particleAmountProperty, particleAmount);

        visualEffect.SendEvent(onPlayEvent);
    }

    public void PlayAccent()
    {
        if (visualEffect == null)
            return;

        visualEffect.SendEvent(onAccentEvent);
    }

    [ContextMenu("Play VFX")]
    private void PlayFromInspector()
    {
        Play(0, false);
    }

    [ContextMenu("Play Accent VFX")]
    private void PlayAccentFromInspector()
    {
        PlayAccent();
    }
}