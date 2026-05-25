using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class RetroEffectsManager : MonoBehaviour
{
    public static RetroEffectsManager Instance;

    private const string PREF_KEY = "retro_effects";

    [Header("References")]
    [SerializeField] private string scaneLineInflunceProperty = "_Influence";
    [SerializeField] private Material scanlineEffect;
    [SerializeField] private Volume globalVolume;

    [Header("Bloom Settings")]
    [SerializeField] private float retroBloom = 1.6f;
    [SerializeField] private float minimalBloom = 0.2f;

    [Header("Vignette Settings")]
    [SerializeField] private float retroVignette = 0.214f;
    [SerializeField] private float minimalVignette = 0.1f;

    [Header("Chromatic Abberation Settings")]
    [SerializeField] private float retroChomaticAb = 0.15f;
    [SerializeField] private float minimalChomaticAb = 0.1f;

    [Header("Lense Distortion Settings")]
    [SerializeField] private float retroLensDis = 0.2f;
    [SerializeField] private float minimalLensDis = 0.1f;

    private bool retroEnabled = true;

    void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        bool enabled = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;

        SetRetroEffects(enabled);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject obj = GameObject.FindGameObjectWithTag("GlobalVolume");

        if (obj != null)
        {
            globalVolume = obj.GetComponent<Volume>();
        }

        bool enabled = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;

        SetRetroEffects(enabled);
    }

    public void SetRetroEffects(bool enabled)
    {
        retroEnabled = enabled;

        scanlineEffect.SetFloat(scaneLineInflunceProperty, enabled == true ? 1 : 0);

        if(!HasValidVolume())
            return;

        if (globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.LensDistortion>(out var ld))
        {
            ld.intensity.value = enabled
                ? retroLensDis
                : minimalLensDis;
        }

        if (globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom))
        {
            bloom.intensity.value = enabled
                ? retroBloom
                : minimalBloom;
        }

        if (globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out var vignette))
        {
            vignette.intensity.value = enabled
                ? retroVignette
                : minimalVignette;
        }

        if (globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.ChromaticAberration>(out var ca))
        {
            ca.intensity.value = enabled
                ? retroChomaticAb
                : minimalChomaticAb;
        }

        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool RetroEnabled => retroEnabled;

    private bool HasValidVolume()
    {
        return globalVolume != null && globalVolume.gameObject != null;
    }
}