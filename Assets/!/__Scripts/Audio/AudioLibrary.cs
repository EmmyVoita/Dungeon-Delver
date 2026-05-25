using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance { get; private set; }

    [SerializeField] private AudioDatabase database;

    public AudioDatabase Database => database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}