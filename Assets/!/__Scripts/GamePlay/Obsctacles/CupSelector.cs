using UnityEngine;

public class CupSelector : MonoBehaviour
{
    public GameObject destoryPS;
    public AudioClip destroySound;
    public AudioClip selectSound;
    private CupShuffleObstacleManager manager;
    private bool colActive = false;
    private int cupIndex = -1;

    public void Init(CupShuffleObstacleManager m, int index)
    {
        manager = m;
        cupIndex = index;
    }

    public void OnShuffled()
    {
        colActive = true;
    }

    public void DisableCol()
    {
        colActive = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!colActive) return;
        if (!other.CompareTag("Player")) return;

        colActive = false;

        AudioHelpers.PlayMyClipAtPoint(selectSound, AudioChannel.SFX, Camera.main.transform.position);

        CupShuffleObstacleManager m = GetComponentInParent<CupShuffleObstacleManager>();

        if (m != null)
            m.ChooseCup(cupIndex);
    }

    public void OnDeath()
    {
        colActive = false;

        if (destoryPS != null)
            Instantiate(destoryPS, transform.position, Quaternion.identity);

        AudioHelpers.PlayClipWithVariation(destroySound, AudioChannel.SFX, Camera.main.transform.position, pitchRange: 0.2f);

        Destroy(gameObject);
    }
}
