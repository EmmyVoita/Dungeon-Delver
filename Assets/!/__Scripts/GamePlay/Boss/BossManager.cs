using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [SerializeField] private bool _practiceBoss = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private bool _activeBoss;

    public bool IsBossActive => _activeBoss;


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelEndState)
        {
            Debug.Log($"Setting active boss to false. Active boss was => {IsBossActive}");
            _activeBoss = false;
        }
    }

    private void Start()
    {   
        Debug.LogError("StartingBoss");
        _practiceBoss = GameSessionBootstrap.Config.Mode == GameMode.ObstaclePracticeBoss;
        if(_practiceBoss)
         _activeBoss = true;

    }

    public void StartBoss()
    {
        Debug.Log("StartingBoss");
        _activeBoss = true;
    }
}
