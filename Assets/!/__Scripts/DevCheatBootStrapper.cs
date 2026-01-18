using UnityEngine;
using UnityEngine.SceneManagement;

public class DevCheatBootstrapper : MonoBehaviour
{
    public static DevCheatBootstrapper Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private bool invincibleByDefaultInEditor = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if UNITY_EDITOR
        if (invincibleByDefaultInEditor)
            DevCheats.SetInvincible(true);
#endif
    }
}
