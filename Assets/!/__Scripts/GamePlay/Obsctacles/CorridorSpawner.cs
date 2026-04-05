using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class CorridorSpawner : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("ObstacleSettings")]
    [SerializeField] private float obstacleDuration = 12f;


    [Header("File")]
    [SerializeField] private string fileName = "Patterns/corridor1";

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2 origin = Vector2.zero;

    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Scrolling")]
    [SerializeField] private float maxScrollSpeed = 5f;
    [SerializeField] private float rampScrollSpeedDuration = 8f;
    [SerializeField] private AnimationCurve easeCurve;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool registered = false;
    private bool _updateMovement;
    private float _scrollSpeed;

    void Start()
    {
        _scrollSpeed = 0f;

        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

        StartCoroutine(ObstacleSequence());
    }

    private IEnumerator ObstacleSequence()
    {
        BuildFromFile();
        _updateMovement = true;

        DOTween.To(() => _scrollSpeed, x => _scrollSpeed = x, maxScrollSpeed, rampScrollSpeedDuration).SetEase(easeCurve);

        yield return new WaitForSeconds(obstacleDuration);
        _updateMovement = false;
        
        CleanUp();

        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
            Destroy(this.gameObject);
        }
    }

    void CleanUp()
    {
        foreach(var obj in spawnedObjects)
        {
            Destroy(obj);
        }
    }


    void Update()
    {
        if(!_updateMovement) return;
        // Move entire corridor left
        transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
    }

    void BuildFromFile()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset == null)
        {
            Debug.LogError($"File not found: {fileName}");
            return;
        }

        string[] lines = textAsset.text.Split('\n');

        int height = lines.Length;

        for (int y = 0; y < height; y++)
        {
            string line = lines[y].Trim();

            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];

                if (c == '#')
                {
                    Vector3 pos = new Vector3(
                        origin.x + x * cellSize,
                        origin.y - y * cellSize,
                        0f
                    );

                    GameObject obj = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    spawnedObjects.Add(obj);
                }
            }
        }
    }
}