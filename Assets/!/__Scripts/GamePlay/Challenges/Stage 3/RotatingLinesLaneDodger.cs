using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class RotatingLinesLaneDodger : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("SequenceSettings")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float obstacleActiveTime = 10f;
    

    [Header("GeneralSettings")]
    [SerializeField] private GameObject spawnObject;

    [SerializeField] private int rowCount = 3;
    [SerializeField] private float rowSpacing = 1f;
    [SerializeField] private int perRowObjectCount = 6;
    [SerializeField] private float horizontalObjectSpacing = 1f;

    [Header("Ellipse")]
    [SerializeField] private Vector2 ellipseCenter = new Vector2(0,0);
    [SerializeField] private float semiMajorAxis = 1f;
    [SerializeField] private float semiMinorAxis = 3f;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float rampRotationSpeedDuration = 2f;


    private Transform _mainAnchor;
    private List<Transform> _rowAmchors;
    private List<GameObject> _spawnObjs;
    private bool _updateMovement;
    private float _rotationSpeed;
    private float _elapsedTime = 0f;


    void Start()
    {
        _spawnObjs = new List<GameObject>();
        _rowAmchors = new List<Transform>();
        _updateMovement = false;
        _rotationSpeed = 0f;
        _elapsedTime = 0f;

        Begin();
    }

    void Setup()
    {
        // We want to create the main parent object which contains the child rows
        // Then we want to create a seperate parent game object container for each row
        // that containes each of the individual prefab objects

        _mainAnchor = new GameObject("Main Anchor").transform;

        for(int i = 0; i < rowCount; i++)
        {
            // Start at the top and work down
            float yOffset = (rowCount * rowSpacing) / 2 - rowSpacing * i;
            float xOffset = (i+1) % 2 == 0 ? 0.5f * horizontalObjectSpacing : 0;
            
            Transform rowTransform = new GameObject($"Item Anchor_{i}").transform;
            rowTransform.position = new Vector3(xOffset, yOffset, 0);
            rowTransform.parent = _mainAnchor;
            _rowAmchors.Add(rowTransform);
        }


        //For each row spawn balls
        for(int i = 0; i < rowCount; i++)
        {
            for(int j = 0; j < perRowObjectCount; j++)
            {
                Transform parent = _rowAmchors[i];

                // Start at the right and work left
                float xOffset = (perRowObjectCount * horizontalObjectSpacing) / 2 - horizontalObjectSpacing * j;
                Vector3 spawnPos = new Vector3(xOffset + parent.position.x,parent.position.y,0);

                

                GameObject obj = Instantiate(
                    spawnObject,
                    spawnPos,
                    Quaternion.identity,
                    parent
                );

                _spawnObjs.Add(obj);
            }
        }
    }

    void Update()
    {
        if(!_updateMovement) return;

        _elapsedTime += Time.deltaTime;
        MoveAnchor();
    }

    void MoveAnchor()
    {
        float _alpha = _elapsedTime * _rotationSpeed;

        float x = ellipseCenter.x + semiMajorAxis * Mathf.Cos(_alpha);
        float y = ellipseCenter.y + semiMinorAxis * Mathf.Sin(_alpha);

        _mainAnchor.position = new Vector2(x, y);
    }

    private IEnumerator ObstacleSequence()
    {
        Setup();
        MoveAnchor();
        
        yield return new WaitForSeconds(startDelay);

        _updateMovement = true;

        DOTween.To(() => _rotationSpeed, x => _rotationSpeed = x, rotationSpeed, rampRotationSpeedDuration)
        .SetLink(gameObject);

        yield return new WaitForSeconds(obstacleActiveTime);

        _updateMovement = false;

        CleanUp();
        End();
    }

    protected override void CleanUp()
    {
        foreach (GameObject obj in _spawnObjs)
        {
            if(obj != null)
                Destroy(obj);
        }

        _spawnObjs.Clear();
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);
        StartCoroutine(ObstacleSequence());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}
