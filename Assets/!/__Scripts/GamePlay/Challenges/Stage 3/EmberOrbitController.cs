using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EmberOrbitController : MonoBehaviour
{
    
    [Header("Setup")]
    [SerializeField] private BounceBomb controller;
    [SerializeField] private GameObject emberPrefab;

    
    [SerializeField] private Vector2 centerOffset = new Vector2(0f,0f);
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float barspacing = 0.1f;


    private List<Transform> embers = new List<Transform>();

    void Start()
    {
        SpawnEmbers();
    }

    void Update()
    {
        UpdateEmberPositionsBar();
    }

    void OnDestroy()
    {
       CleanUp();
    }
    
    public void CleanUp()
    {
         for(int i = 0; i < embers.Count; i++)
        {
            Transform ember = embers[embers.Count - 1];
            embers.RemoveAt(embers.Count - 1);
            Destroy(ember.gameObject);
        }
    }

    void SpawnEmbers()
    {
        foreach(var e in embers)
        {
            if(e != null)
            {
                embers.Remove(e);
                Destroy(e.gameObject);
            }
        }

        embers.Clear();

        for (int i = 0; i < controller.HitsRequired; i++)
        {
            GameObject ember = Instantiate(emberPrefab, transform.position, Quaternion.identity, parent: this.transform);
            //GameObject ember = Instantiate(emberPrefab, transform.position, Quaternion.identity);
            embers.Add(ember.transform);
        }

        UpdateEmberPositionsBar();
    }

    void UpdateEmberPositions()
    {
        int count = embers.Count;

        for(int i = 0; i < count; i++)
        {
            if(embers[i] == null) continue;

            float rotationOffset = Time.time * rotationSpeed;
            float baseAngle = (i * 360f) / count;

            float angle = baseAngle + rotationOffset;
            float radians = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
            embers[i].localPosition = centerOffset + pos;
        }
    }

    void UpdateEmberPositionsBar()
    {
        int count = embers.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * barspacing;

        for (int i = 0; i < count; i++)
        {
            if (embers[i] == null) continue;

            float x = i * barspacing - totalWidth * 0.5f;
            float y = -0.6f; // offset below bomb (tweak this)

            embers[i].localPosition = centerOffset + new Vector2(x, y);
        }
    }

    public void RemoveEmber()
    {
        if(embers.Count == 0) return;

        Transform ember = embers[embers.Count - 1];
        embers.RemoveAt(embers.Count - 1);

        // Animate pop + disappear
        ember.DOKill();

        ember.DOScale(popScale, popDuration * 0.5f)
            .OnComplete(() =>
            {
                ember.DOScale(0f, popDuration * 0.5f)
                     .OnComplete(() => Destroy(ember.gameObject));
            });
    }
}