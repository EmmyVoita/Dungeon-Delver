using UnityEngine;

public class OrbitingTreasureController : MonoBehaviour
{
    public static OrbitingTreasureController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject orbitingTreasurePrefab;

    [Header("Default Settings")]
    [SerializeField] private int maxRelicCount = 3;
    [SerializeField] private int currencyPerDestroyedRelic = 25;

    private OrbitingTreasure orbitingTreasure;
    public bool IsAtMaxRelics => orbitingTreasure != null ? orbitingTreasure.ActiveRelicCount == maxRelicCount : true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        RoundManager.OnRoundStart += EnsureTreasureExists;
        RoundManager.OnRoundEnd += ClearRelics;
    }

    private void OnDisable()
    {
        RoundManager.OnRoundStart -= EnsureTreasureExists;
        RoundManager.OnRoundEnd -= ClearRelics;
    }


    private void EnsureTreasureExists()
    {
        if (orbitingTreasure != null)
            return;

        if (orbitingTreasurePrefab == null)
        {
            Debug.LogWarning(
                $"{nameof(OrbitingTreasureController)} has no treasure prefab assigned."
            );

            return;
        }

        GameObject instance = Instantiate(
            orbitingTreasurePrefab,
            Player.Instance.transform.position,
            Quaternion.identity
        );

        orbitingTreasure =
            instance.GetComponent<OrbitingTreasure>();

        if (orbitingTreasure == null)
        {
            Debug.LogError(
                $"{orbitingTreasurePrefab.name} does not contain an " +
                $"{nameof(OrbitingTreasure)} component."
            );
        }
    }

    public void AddRelics(int amount, int currencyAmount)
    {
        EnsureTreasureExists();

        if (orbitingTreasure == null)
            return;

        amount = Mathf.Max(0, amount);

        for (int i = 0; i < amount; i++)
        {
            if (orbitingTreasure.ActiveRelicCount >= maxRelicCount)
                return;

            orbitingTreasure.AddRelic(currencyAmount);
        }
    }

    public void ClearRelics()
    {
        if (orbitingTreasure == null)
            return;

        orbitingTreasure.ClearRelics();
    }

    public void SetCurrencyPerRelic(int amount)
    {
        currencyPerDestroyedRelic = Mathf.Max(0, amount);
    }
}