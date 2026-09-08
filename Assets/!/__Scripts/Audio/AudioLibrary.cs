using System.Runtime.CompilerServices;
using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance { get; private set; }

    [SerializeField] private AudioDatabase database;

    public AudioDatabase Database => database;
    public ArrowHitSoundData ArrowHitSoundData => _arrowHitSound;
    public int ArrowHitSoundIndex => _index;

    private ArrowHitSoundData _arrowHitSound;
    private int _index;

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

    public void SetArrowHitSound(int index)
    {
        index = Mathf.Clamp(index, 0, database.arrowSounds.Count-1);
        _index = index;
        _arrowHitSound = database.arrowSounds[index];
    }
}